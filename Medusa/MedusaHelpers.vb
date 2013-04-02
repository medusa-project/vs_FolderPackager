Imports System.IO
Imports Uiuc.Library.Premis
Imports Uiuc.Library.Medusa
Imports Uiuc.Library.Ldap
Imports Uiuc.Library.NetFedora
Imports Uiuc.Library.MetadataUtilities
Imports System.Text.RegularExpressions
Imports System.Configuration
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Net.Mail
Imports System.Security.Cryptography

''' <summary>
''' In order to keep the core Premis classes as generic as possible, any code that is unique to the Medusa Repository project should go here.
''' </summary>
''' <remarks></remarks>
Public Class MedusaHelpers

  Public Const MedusaNamespace As String = "http://medusa.library.illinois.edu/ns#"
  Public Const PremisOwlNamespace As String = "http://multimedialab.elis.ugent.be/users/samcoppe/ontologies/Premis/premis.owl#"

  ''' <summary>
  ''' Uisng values from configuration, create Premis Rights Statement
  ''' </summary>
  ''' <param name="idType"></param>
  ''' <param name="id"></param>
  ''' <param name="premisObj"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetPremisDisseminationRights(idType As String, id As String, premisObj As PremisObject) As PremisRights
    Dim ret As PremisRights = Nothing

    If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationRights) Then
      Dim pRgtStmt As New PremisRightsStatement(idType, id, MedusaAppSettings.Settings.PremisDisseminationRightsBasis)
      pRgtStmt.RightsGranted.Add(New PremisRightsGranted(MedusaAppSettings.Settings.PremisDisseminationRights))
      pRgtStmt.LinkToObject(premisObj)
      If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationRightsRestrictions) Then
        pRgtStmt.RightsGranted.FirstOrDefault.Restrictions.Add(MedusaAppSettings.Settings.PremisDisseminationRightsRestrictions)
      End If
      If MedusaAppSettings.Settings.PremisDisseminationRightsBasis = MedusaAppSettings.COPYRIGHT And
        Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationCopyrightStatus) Then
        Dim cpyRt As PremisCopyrightInformation
        If String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationCopyrightJurisdiction) Then
          cpyRt = New PremisCopyrightInformation(MedusaAppSettings.Settings.PremisDisseminationCopyrightStatus, "United States")
        Else
          cpyRt = New PremisCopyrightInformation(MedusaAppSettings.Settings.PremisDisseminationCopyrightStatus, MedusaAppSettings.Settings.PremisDisseminationCopyrightJurisdiction)
        End If
        If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationCopyrightNote) Then
          cpyRt.CopyrightNotes.Add(MedusaAppSettings.Settings.PremisDisseminationCopyrightNote)
        End If
        pRgtStmt.CopyrightInformation = cpyRt

      ElseIf MedusaAppSettings.Settings.PremisDisseminationRightsBasis = MedusaAppSettings.STATUTE And
                    Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationStatuteCitation) Then
        Dim stat As PremisStatuteInformation
        If String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationStatuteJurisdiction) Then
          stat = New PremisStatuteInformation(MedusaAppSettings.Settings.PremisDisseminationStatuteCitation, "United States")
        Else
          stat = New PremisStatuteInformation(MedusaAppSettings.Settings.PremisDisseminationStatuteCitation, MedusaAppSettings.Settings.PremisDisseminationStatuteJurisdiction)
        End If
        pRgtStmt.StatuteInformation.Add(stat)

      ElseIf MedusaAppSettings.Settings.PremisDisseminationRightsBasis = MedusaAppSettings.OTHER And
                    Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationOtherRightsBasis) Then
        Dim oth As New PremisOtherRightsInformation(MedusaAppSettings.Settings.PremisDisseminationOtherRightsBasis)
        pRgtStmt.OtherRightsInformation = oth
      End If

      ret = New PremisRights(pRgtStmt)

    End If

    Return ret
  End Function

  Public Shared Sub SavePremisContainer(pContainer As PremisContainer, destPath As String)
    Select Case MedusaAppSettings.Settings.SaveFilesAs
      Case SaveFileAsType.ONE
        'save one big premis file for the whole object
        pContainer.SaveXML(Path.Combine(destPath, pContainer.Objects.First.GetDefaultFileName("premis_", "xml")))
      Case SaveFileAsType.MULTIPLE
        'save a separate file for each premis entity
        pContainer.SaveEachXML(destPath)
      Case SaveFileAsType.REPRESENTATIONS
        'create directory structure and save a separate premis file for each premis representation object and associated entities, 
        'also save a premis file for each file object (not metadata)
        'TODO:  This function has not been tested for the folder packager, so should not be used yet
        pContainer.SavePartitionedXML(destPath, True)
      Case SaveFileAsType.MEDUSA, SaveFileAsType.MEDUSA_MULTIPLE
        'create directory structure and save a separate premis file for each fedora object as defined for our medusa content model
        'pContainer.SaveXML(Path.Combine(recPath, pContainer.Objects.First.GetDefaultFileName("test_premis_", "xml")))
        MedusaHelpers.SavePartitionedXML(pContainer, destPath, True)
      Case SaveFileAsType.MEDUSA_FOXML
        MedusaHelpers.SaveFoxml(pContainer, destPath)
      Case Else
        Throw New MedusaException(String.Format("Unknown save format '{0}'", MedusaAppSettings.Settings.SaveFilesAs))
    End Select

  End Sub

  ''' <summary>
  ''' Save each PREMIS Entity to its own FoxML ingest package, converting all links and relationships into RELS-EXT
  ''' </summary>
  ''' <param name="cont"></param>
  ''' <param name="origFolder"></param>
  ''' <remarks></remarks>
  Public Shared Sub SaveFoxml(cont As PremisContainer, origFolder As String)
    If cont.PersistedEntityTypes.HasFlag(PremisEntityTypes.Objects) Then
      For Each pr As PremisObject In cont.Objects
        Dim fxObj As New FoxmlDigitalObject(pr.LocalIdentifierValue)

        'Convert the PREMIS Object Links and Relationships into RELS_EXT and (maybe?) remove those links from the PREMIS Object
        Dim relsext As New RelsExt(pr.LocalIdentifierValue)

        For Each relat As PremisRelationship In pr.Relationships
          For Each obj As PremisObject In relat.RelatedObjects
            relsext.AddRelationship("medusa", String.Format("{0}.{1}", relat.RelationshipType, relat.RelationshipSubType),
                                    MedusaHelpers.MedusaNamespace, obj.LocalIdentifierValue)
          Next
          'NOTE: PREMIS Relationships can also have a corresponding Premis Event; if we remove the relationship from the PREMIS document we also lose the
          'correspondence of the event to the relationship
          For Each evt As PremisEvent In relat.RelatedEvents
            relsext.AddRelationship("medusa", String.Format("{0}.{1}.EVENT", relat.RelationshipType, relat.RelationshipSubType),
                                    MedusaHelpers.MedusaNamespace, evt.EventIdentifier.IdentifierValue)
          Next
        Next

        'linked events
        For Each linkEvt As PremisEvent In pr.LinkedEvents
          relsext.AddRelationship("premis", "linkingEvent",
                                  MedusaHelpers.PremisOwlNamespace, linkEvt.EventIdentifier.IdentifierValue)
        Next

        'linked rights statements
        For Each linkRts As PremisRightsStatement In pr.LinkedRightsStatements
          relsext.AddRelationship("premis", "linkingRightsStatement",
                                  MedusaHelpers.PremisOwlNamespace, linkRts.RightsStatementIdentifier.IdentifierValue)
        Next

        'linked intellectual entities
        For Each intEntId As PremisIdentifier In pr.LinkedIntellectualEntityIdentifiers
          relsext.AddRelationship("premis", "linkingIntellectualEntity ",
                                  MedusaHelpers.PremisOwlNamespace, intEntId.IdentifierValue)
        Next

        fxObj.DataStreams.Add(relsext.Datastream)

        'TODO: If this object is linked to a related DC or MODS metadata file then use that to create a DC datastream


        Dim fxDS As New FoxmlDatastream("PREMIS-OBJECT", ControlGroups.X)
        Dim fxDSV As New FoxmlDatastreamVersion("text/xml", pr.GetXmlDocument(cont))
        fxDSV.FormatUri = New Uri(MedusaHelpers.PremisOwlNamespace & "Object")
        fxDS.DatastreamVersions.Add(fxDSV)
        fxObj.DataStreams.Add(fxDS)

        If pr.ObjectCategory = PremisObjectCategory.File Then
          'TODO: add a datastream for the file and convert the FILENAME Identifier into a URL to use for this datastream
          Dim fxDSFile As New FoxmlDatastream("FILE", ControlGroups.M)
          Dim fldr As String = origFolder.Replace(MedusaAppSettings.Settings.WorkingFolder, MedusaAppSettings.Settings.WorkingUrl).Replace("\", "/")
          Dim baseUrl As New Uri(New Uri(MedusaAppSettings.Settings.WorkingUrl), fldr & "/")
          Dim url As New Uri(baseUrl, pr.GetFilenameIdentifier.IdentifierValue)
          Dim fxDSFileV As New FoxmlDatastreamVersion(pr.ObjectCharacteristics.First.Formats.First.FormatName, url)
          fxDSFile.DatastreamVersions.Add(fxDSFileV)
          fxObj.DataStreams.Add(fxDSFile)
        End If

        'Save the XML
        fxObj.ValidateXML = True
        Dim fname As String = Path.Combine(origFolder, pr.GetDefaultFileName("foxml_", "xml"))
        fxObj.SaveXML(fname)

      Next
    End If

    If cont.PersistedEntityTypes.HasFlag(PremisEntityTypes.Events) Then
      For Each pr As PremisEvent In cont.Events
        Dim fxEvt As New FoxmlDigitalObject(pr.EventIdentifier.IdentifierValue)

        'Convert the PREMIS Object Links and Relationships into RELS_EXT and (maybe?) remove those links from the PREMIS Object
        Dim relsext As New RelsExt(pr.EventIdentifier.IdentifierValue)

        'linked events
        For Each kvp As KeyValuePair(Of PremisObject, List(Of String)) In pr.LinkedObjects
          If kvp.Value.Count = 0 Then
            relsext.AddRelationship("premis", "linkingObject",
                                    MedusaHelpers.PremisOwlNamespace, kvp.Key.LocalIdentifierValue)
          Else
            For Each role In kvp.Value
              relsext.AddRelationship("medusa", String.Format("linkingObject.{0}", role),
                                      MedusaHelpers.PremisOwlNamespace, kvp.Key.LocalIdentifierValue)
            Next
          End If

        Next

        'linked agents
        For Each kvp As KeyValuePair(Of PremisAgent, List(Of String)) In pr.LinkedAgents
          If kvp.Value.Count = 0 Then
            relsext.AddRelationship("premis", "linkingAgent",
                                    MedusaHelpers.PremisOwlNamespace, kvp.Key.LocalIdentifierValue)
          Else
            For Each role In kvp.Value
              relsext.AddRelationship("medusa", String.Format("linkingAgent.{0}", role),
                                      MedusaHelpers.PremisOwlNamespace, kvp.Key.LocalIdentifierValue)
            Next
          End If

        Next

        fxEvt.DataStreams.Add(relsext.Datastream)

        'create premis-event datastream
        Dim fxDS As New FoxmlDatastream("PREMIS-EVENT", ControlGroups.X)
        Dim fxDSV As New FoxmlDatastreamVersion("text/xml", pr.GetXmlDocument(cont))
        fxDSV.FormatUri = New Uri(MedusaHelpers.PremisOwlNamespace & "Event")
        fxDS.DatastreamVersions.Add(fxDSV)
        fxEvt.DataStreams.Add(fxDS)

        'save file
        fxEvt.ValidateXML = True
        Dim fname As String = Path.Combine(origFolder, pr.GetDefaultFileName("foxml_", "xml"))
        fxEvt.SaveXML(fname)

      Next
    End If

    If cont.PersistedEntityTypes.HasFlag(PremisEntityTypes.Agents) Then
      For Each pr As PremisAgent In cont.Agents

      Next
    End If

    If cont.PersistedEntityTypes.HasFlag(PremisEntityTypes.Rights) Then
      For Each pr As PremisRights In cont.Rights

      Next
    End If

  End Sub

  Public Function CamelCase(s As String) As String
    Dim ret As String = s
    Return ret
  End Function

  ''' <summary>
  '''  This function partitions the given premisContainer and then saves a separate XML file for each grouping of files according to the Medusa content models
  ''' </summary>
  ''' <param name="cont"></param>
  ''' <param name="origFolder"></param>
  ''' <param name="createSubDirs"></param>
  ''' <remarks></remarks>
  Public Shared Sub SavePartitionedXML(cont As PremisContainer, origFolder As String, createSubDirs As Boolean)
    'TODO: Adjust folder value to remove the base object identifier part

    Dim rootObj = cont.Objects.First 'assume the first object in the container is the root object
    Dim rootPath = rootObj.GetDefaultFileName("", "")
    Dim conList As New List(Of KeyValuePair(Of String, PremisContainer))

    MedusaHelpers.PartitionContainer(rootObj, rootPath, conList, cont.PersistedEntityTypes)

    Dim folder As String = origFolder.Substring(0, origFolder.LastIndexOf(rootPath))

    If createSubDirs Then
      For Each kv As KeyValuePair(Of String, PremisContainer) In conList
        Directory.CreateDirectory(Path.Combine(folder, kv.Key))
        For Each obj In kv.Value.Objects.Where(Function(o) o.ObjectCategory = PremisObjectCategory.File)
          Dim newFPath As String = Path.Combine(folder, kv.Key, Path.GetFileName(obj.GetFilenameIdentifier.IdentifierValue))
          Dim origFPath As String = Path.Combine(folder, rootPath, obj.GetFilenameIdentifier.IdentifierValue)
          'if these are the same value which can happen for metadata files, then we dpon't need to do anything
          If origFPath.ToLower.Trim <> newFPath.ToLower.Trim Then
            If File.Exists(newFPath) Then
              'because we are just rearranging the file structure this should happen regardless of the OverwriteObjects AppSetting
              File.Delete(newFPath)
              Trace.TraceWarning("File '{0}' already exists.  It will be deleted and replaced with the new file of same name.", newFPath)
            End If
            File.Move(origFPath, newFPath)
          End If
        Next

        'there should only be one premis object per container after the partitioning
        'unless the object contains metadata file
        'If kv.Value.Objects.Count <> 1 Then
        '  Trace.TraceError("Unexpected number of objects in PREMIS container")
        '  Trace.Flush()
        '  Throw New PackagerException("Unexpected number of objects in PREMIS container")
        'End If

        'after the partition, if creating subdirs, the FOLDERNAME identifier is not needed
        kv.Value.Objects.Item(0).ObjectIdentifiers.RemoveAll(Function(i) i.IdentifierType = "FOLDERNAME")

        'after the partition, if creating subdirs, the FILENAME identifier should have any path component removed
        For i As Integer = 0 To kv.Value.Objects.Count - 1
          If kv.Value.Objects.Item(i).ObjectCategory = PremisObjectCategory.File Then
            kv.Value.Objects.Item(i).GetFilenameIdentifier.IdentifierValue = Path.GetFileName(kv.Value.Objects.Item(i).GetFilenameIdentifier.IdentifierValue)
          End If
        Next

        If MedusaAppSettings.Settings.SaveFilesAs = SaveFileAsType.MEDUSA Then
          kv.Value.SaveXML(Path.Combine(folder, kv.Key, kv.Value.Objects.First.GetDefaultFileName("premis_", "xml")))
        ElseIf MedusaAppSettings.Settings.SaveFilesAs = SaveFileAsType.MEDUSA_MULTIPLE Then
          kv.Value.SaveEachXML(Path.Combine(folder, kv.Key))
        Else
          Throw New MedusaException(String.Format("Unexpected SaveFileAs Setting '{0}'.", MedusaAppSettings.Settings.SaveFilesAs))
        End If

      Next
    Else
      For Each kv As KeyValuePair(Of String, PremisContainer) In conList

        If MedusaAppSettings.Settings.SaveFilesAs = SaveFileAsType.MEDUSA Then
          kv.Value.SaveXML(Path.Combine(folder, kv.Value.Objects.First.GetDefaultFileName("premis_", "xml")))
        ElseIf MedusaAppSettings.Settings.SaveFilesAs = SaveFileAsType.MEDUSA_MULTIPLE Then
          kv.Value.SaveEachXML(folder)
        Else
          Throw New MedusaException(String.Format("Unexpected SaveFileAs Setting '{0}'.", MedusaAppSettings.Settings.SaveFilesAs))
        End If

      Next
    End If

  End Sub

  ''' <summary>
  ''' Partition this container into multiple other containers matching our Medusa content model
  ''' </summary>
  ''' <param name="rootObj">The root starting premis object</param>
  ''' <param name="rootDir">The relative directory path for the root object</param>
  ''' <param name="partList">A list of key-value pairs with the key being the relative directory path and the value being
  ''' the corresponding premis container</param>
  ''' <remarks>This routine will need to be modified as the Medusa Fedora content models are changed or as new relationship types and subtypes are added.</remarks>
  Private Shared Sub PartitionContainer(rootObj As PremisObject, rootDir As String, partList As List(Of KeyValuePair(Of String, PremisContainer)), persist As PremisEntityTypes)

    Dim newCont As New PremisContainer()
    newCont.PersistedEntityTypes = persist

    MedusaHelpers.AddObjectsAndChildren(rootObj, newCont)
    Dim kv As New KeyValuePair(Of String, PremisContainer)(rootDir, newCont)
    partList.Add(kv)

    For Each r As PremisRelationship In rootObj.Relationships
      Select Case r.RelationshipType
        Case "COLLECTION"
          'collection is external to this package so do nothing

          Select Case r.RelationshipSubType
            Case "IS_MEMBER_OF"

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New MedusaException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "METADATA"
          'metadata is kept with its related objects so do nothing

          Select Case r.RelationshipSubType
            Case "MODS", "MARC", "DC_RDF", "SPREADSHEET", "CHECKSUMS", "IMAGE_TECH"

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New MedusaException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "DERIVATION"
          'derivations are kept with their related object and currently only apply to metadata so do nothing

          Select Case r.RelationshipSubType
            Case "HAS_SOURCE", "IS_SOURCE_OF"

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New MedusaException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "BASIC_IMAGE_ASSET"
          'basic image assets are partitioned into separate fedora objects so create new subdir and corresponding premis container

          Select Case r.RelationshipSubType
            Case "PRODUCTION_MASTER", "ARCHIVAL_MASTER", "SCREEN_SIZE", "THUMBNAIL"
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(rootDir, o.GetDefaultFileNameIndex), partList, persist)
              Next

            Case "PARENT"
              'parents should have already been created

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New MedusaException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "BASIC_FILE_ASSET"
          'basic file assets are partitioned into separate fedora objects so create new subdir and corresponding premis container

          Select Case r.RelationshipSubType
            Case "UNSPECIFIED"
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(rootDir, o.GetDefaultFileNameIndex), partList, persist)
              Next

            Case "PARENT"
              'parents should have already been created

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New MedusaException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "BASIC_COMPOUND_ASSET"
          'compound assets are partitioned into separate containers, subdirectories are created depending on the relationship

          Select Case r.RelationshipSubType
            Case "FIRST_CHILD", "CHILD"
              'create subdirectory for this container
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(rootDir, o.GetDefaultFileNameIndex), partList, persist)
              Next

            Case "NEXT_SIBLING"
              'use same subdirectory for this container
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(Path.GetDirectoryName(rootDir), o.GetDefaultFileNameIndex), partList, persist)
              Next

            Case "PREVIOUS_SIBLING"
              'previous siblings should have already been created

            Case "PARENT"
              'parents should have already been created

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New MedusaException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "PAGED_TEXT_ASSET"
          Select Case r.RelationshipSubType
            Case "PAGES"
              'create subdirectory for this container
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(rootDir, o.GetDefaultFileNameIndex), partList, persist)
              Next

            Case "TEI", "OCR", "HIRES_PDF", "OPTIMIZED_PDF"
              'create subdirectory for this container
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(rootDir, o.GetDefaultFileNameIndex), partList, persist)
              Next

            Case "PARENT"
              'parents should have already been created

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New MedusaException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case Else
          Trace.TraceError(String.Format("Unexpected PREMIS Relationship Type: {0}", r.RelationshipType))
          Trace.Flush()
          Throw New MedusaException(String.Format("Unexpected PREMIS Relationship Type: {0}", r.RelationshipType))
      End Select
    Next

  End Sub

  Private Shared Sub AddEventAndChildren(evt As PremisEvent, cont As PremisContainer)
    If Not cont.Events.Contains(evt) Then
      cont.Events.Add(evt)

      For Each agt As PremisAgent In evt.LinkedAgents.Keys
        MedusaHelpers.AddAgentAndChildren(agt, cont)
      Next

      For Each obj As PremisObject In evt.LinkedObjects.Keys.Where(Function(o) o.ObjectCategory <> PremisObjectCategory.Representation)
        If Not obj.PreservationLevels.Exists(Function(p) p.PreservationLevelValue = "ORIGINAL_CONTENT_FILE" Or p.PreservationLevelValue = "DERIVATIVE_CONTENT_FILE") Then
          MedusaHelpers.AddObjectsAndChildren(obj, cont)
        End If
      Next

    End If
  End Sub

  Private Shared Sub AddAgentAndChildren(agt As PremisAgent, cont As PremisContainer)
    If Not cont.Agents.Contains(agt) Then
      cont.Agents.Add(agt)

      'NOTE: linked events and rights should end up in the contained via other means, so we do not enumerate them here

    End If
  End Sub

  Private Shared Sub AddRightsStatementAndChildren(rgtS As PremisRightsStatement, cont As PremisContainer)
    'make sure not already added
    For Each r As PremisRights In cont.Rights
      If r.RightsStatements.Contains(rgtS) Then Exit Sub
    Next

    Dim rgt As New PremisRights(rgtS)
    If Not cont.Rights.Contains(rgt) Then
      cont.Rights.Add(rgt)

      For Each agt As PremisAgent In rgtS.LinkedAgents.Keys
        MedusaHelpers.AddAgentAndChildren(agt, cont)
      Next

    End If
  End Sub

  Private Shared Sub AddObjectsAndChildren(obj As PremisObject, cont As PremisContainer)
    If Not cont.Objects.Contains(obj) Then
      cont.Objects.Add(obj)

      For Each pEvt As PremisEvent In obj.LinkedEvents
        MedusaHelpers.AddEventAndChildren(pEvt, cont)
      Next

      For Each pRgtS As PremisRightsStatement In obj.LinkedRightsStatements
        MedusaHelpers.AddRightsStatementAndChildren(pRgtS, cont)
      Next

      For Each relat As PremisRelationship In obj.Relationships
        'only objects which are not in their own container will be added here
        Select Case relat.RelationshipType
          Case "METADATA", "COLLECTION", "DERIVATION"
            Dim i As Integer = 0
            For Each obj2 As PremisObject In relat.RelatedObjects.Where(Function(o) o.ObjectCategory <> PremisObjectCategory.Representation)
              MedusaHelpers.AddObjectsAndChildren(obj2, cont)
              i = i + 1
            Next
            If i > 0 Then
              For Each evt As PremisEvent In relat.RelatedEvents
                MedusaHelpers.AddEventAndChildren(evt, cont)
                i = i + 1
              Next
            End If
        End Select
      Next
    End If
  End Sub

  ''' <summary>
  ''' Given a list of premis formats return all the distinct mime types
  ''' This is a union of the FormatNames and the FormatNotes with content like "Alternate Format '{0}'"
  ''' </summary>
  ''' <param name="fmts"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetDistinctMimeTypes(fmts As List(Of PremisFormat)) As List(Of String)
    Dim ret As New List(Of String)
    Dim rex As New Regex("[a-zA-Z0-9!#$&.+-^_]+/[a-zA-Z0-9!#$&.+-^_]+", RegexOptions.IgnoreCase)
    For Each fmt As PremisFormat In fmts
      ret.Add(fmt.FormatName)
      For Each fmtNote As String In fmt.FormatNotes
        Dim ms = rex.Matches(fmtNote)
        For Each m As Match In ms
          ret.Add(m.Value)
        Next
      Next
    Next

    Return ret.Distinct.ToList
  End Function

  ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
  'Here is async copy routine from http://www.informit.com/guides/content.aspx?g=dotnet&seqNum=827
  'See also http://stackoverflow.com/questions/1540658/net-asynchronous-stream-read-write
  ''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

  ''' <summary>
  '''  Copies a stream.
  ''' </summary>
  ''' <param name="source">The stream containing the source data.</param>
  ''' <param name="target">The stream that will receive the source data.</param>
  ''' <remarks>
  ''' This function copies until no more can be read from the stream
  '''  and does not close the stream when done.<br/>
  ''' Read and write are performed simultaneously to improve throughput.<br/>
  ''' If no data can be read for 60 seconds, the copy will time-out.
  ''' 
  ''' NOTE: Based on a bit of testing this performs no better than Stream.CopyTO
  ''' </remarks>
  Public Shared Sub CopyStreamToStream(source As Stream, target As Stream, bufSize As Integer)
    ' This stream copy supports a source-read happening at the same time
    ' as target-write.  A simpler implementation would be to use just
    ' Write() instead of BeginWrite(), at the cost of speed.

    Dim readbuffer(bufSize) As Byte
    Dim writebuffer(bufSize) As Byte
    Dim asyncResult As IAsyncResult = Nothing

    While True
      ' Read data into the readbuffer.  The previous call to BeginWrite, if any,
      '  is executing in the background.
      Dim read As Integer = source.Read(readbuffer, 0, readbuffer.Length)

      ' Ok, we have read some data and we're ready to write it, so wait here
      '  to make sure that the previous write is done before we write again.
      If asyncResult IsNot Nothing Then
        ' This should work down to ~0.01kb/sec
        asyncResult.AsyncWaitHandle.WaitOne(60000)
        target.EndWrite(asyncResult) ' Last step to the 'write'.
        If (Not asyncResult.IsCompleted) Then ' Make sure the write really completed.
          Throw New IOException("Stream write failed.")
        End If
      End If

      If read <= 0 Then
        Return ' source stream says we're done - nothing else to read.
      End If

      ' Swap the read and write buffers so we can write what we read, and we can
      '  use the then use the other buffer for our next read.
      Dim tbuf() As Byte = writebuffer
      writebuffer = readbuffer
      readbuffer = tbuf

      ' Asynchronously write the data, asyncResult.AsyncWaitHandle will
      ' be set when done.
      asyncResult = target.BeginWrite(writebuffer, 0, read, Nothing, Nothing)
    End While
  End Sub

  ''' <summary>
  ''' Given the filename, return the relationship type between the parent object and this file
  ''' If the specific type cannot be determined the default value is returned
  ''' </summary>
  ''' <param name="filename"></param>
  ''' <param name="dflt"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetMedusaRelationshipType(filename As String, dflt As String) As String
    Dim ret As String = dflt

    If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.MetadataMarcRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataMarcRegex, RegexOptions.IgnoreCase) Then
      ret = "METADATA"
    ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.MetadataDcRdfRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataDcRdfRegex, RegexOptions.IgnoreCase) Then
      ret = "METADATA"
    ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.MetadataSpreadsheetRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataSpreadsheetRegex, RegexOptions.IgnoreCase) Then
      ret = "METADATA"
    ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.ImageTechnicalMetadataRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.ImageTechnicalMetadataRegex, RegexOptions.IgnoreCase) Then
      ret = "METADATA"
    ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.Md5ManifestRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.Md5ManifestRegex, RegexOptions.IgnoreCase) Then
      ret = "METADATA"

    ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.OcrTextRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.OcrTextRegex, RegexOptions.IgnoreCase) Then
      ret = "PAGED_TEXT_ASSET"
    ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.TeiXmlRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.TeiXmlRegex, RegexOptions.IgnoreCase) Then
      ret = "PAGED_TEXT_ASSET"
    ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.HighQualityPdfRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.HighQualityPdfRegex, RegexOptions.IgnoreCase) Then
      ret = "PAGED_TEXT_ASSET"
    ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.OptimizedPdfRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.OptimizedPdfRegex, RegexOptions.IgnoreCase) Then
      ret = "PAGED_TEXT_ASSET"

    End If

    Return ret
  End Function

  ''' <summary>
  ''' Given the filename and base type, return the relationship subtype between the parent object and this file
  ''' If the specific subtype cannot be determined the default value is returned
  ''' </summary>
  ''' <param name="filename"></param>
  ''' <param name="baseType"></param>
  ''' <param name="dflt"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetMedusaRelationshipSubtype(filename As String, baseType As String, dflt As String) As String
    Dim ret As String = dflt

    If baseType = "METADATA" Then
      If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.MetadataMarcRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataMarcRegex, RegexOptions.IgnoreCase) Then
        ret = "MARC"
      ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.MetadataDcRdfRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataDcRdfRegex, RegexOptions.IgnoreCase) Then
        ret = "DC_RDF"
      ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.MetadataSpreadsheetRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataSpreadsheetRegex, RegexOptions.IgnoreCase) Then
        ret = "SPREADSHEET"
      ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.ImageTechnicalMetadataRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.ImageTechnicalMetadataRegex, RegexOptions.IgnoreCase) Then
        ret = "IMAGE_TECH"
      ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.Md5ManifestRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.Md5ManifestRegex, RegexOptions.IgnoreCase) Then
        ret = "CHECKSUMS"
      End If

    ElseIf baseType = "PAGED_TEXT_ASSET" Then
      If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.OcrTextRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.OcrTextRegex, RegexOptions.IgnoreCase) Then
        ret = "OCR"
      ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.TeiXmlRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.TeiXmlRegex, RegexOptions.IgnoreCase) Then
        ret = "TEI"
      ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.HighQualityPdfRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.HighQualityPdfRegex, RegexOptions.IgnoreCase) Then
        ret = "HIRES_PDF"
      ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.OptimizedPdfRegex) AndAlso Regex.IsMatch(filename, MedusaAppSettings.Settings.OptimizedPdfRegex, RegexOptions.IgnoreCase) Then
        ret = "OPTIMIZED_PDF"
      End If

    End If

    Return ret
  End Function

  Public Shared Sub EmailException(ex As Exception)
    Dim msgStr As String = String.Format("{0}{2}{2}{1}", ex.Message, ex.StackTrace, vbCrLf)
    MedusaHelpers.EmailErrorMessage(msgStr)
  End Sub


  Public Shared Sub EmailErrorMessage(msgStr As String)
    Try
      Dim usr = UIUCLDAPUser.Create(Principal.WindowsIdentity.GetCurrent.Name)
      Dim msg As New System.Net.Mail.MailMessage("medusa-admin@library.illinois.edu", usr.EMail)
      msg.Subject = String.Format("Medusa Error Report: {0}", My.Application.Info.Title)
      msg.Body = msgStr

      Dim smtp As New SmtpClient()
      smtp.Send(msg)
    Catch ex2 As Exception
      Console.Error.WriteLine(String.Format("Email Error: {0}", ex2.Message))
      Trace.TraceError("{0}", ex2.Message)
    End Try

  End Sub

  Public Shared Function GetFileNamePrefix(relat As String, subtype As String) As String
    Dim ret As String = "file_"

    Select Case relat
      Case "METADATA"
        Select Case subtype
          Case "MARC"
            ret = "marc_"
          Case "DC_RDF"
            ret = "dc_rdf_"
          Case Else
            ret = subtype.ToLower & "_"
        End Select
    End Select

    Return ret
  End Function

  <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
  Public Shared Function CreateHardLink(ByVal lpFileName As String, ByVal lpExistingFileName As String, ByVal lpSecurityAttributes As IntPtr) As Boolean
  End Function

  ''' <summary>
  ''' Return a PREMIS Agent object for the given NetID
  ''' </summary>
  ''' <param name="netid"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetPremisAgent(ByVal netid As String) As PremisAgent
    Dim ldap As New UIUCLDAPUser(netid)
    Dim agent As PremisAgent = New PremisAgent("UIUC_NETID", netid)
    agent.AgentIdentifiers.Add(New PremisIdentifier("EMAIL", ldap.EMail))
    agent.AgentNames.Add(ldap.DisplayName)
    agent.AgentType = "PERSON"
    agent.AgentNotes.Add(ldap.Title & ", " & ldap.HomeDepartment)

    Return agent
  End Function

  ''' <summary>
  ''' Return a PREMIS Agent object for the currently logged in user
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetPremisAgent() As PremisAgent
    Return MedusaHelpers.GetPremisAgent(Principal.WindowsIdentity.GetCurrent.Name)
  End Function

  Public Shared Function CharacterizeFile(ByVal filepath As String, ByVal proposedMime As String) As PremisObjectCharacteristics
    'TODO: Make the FITS utility an option for this

    Dim pObjChar As New PremisObjectCharacteristics()

    Dim alg As String = MedusaAppSettings.Settings.ChecksumAlgorithm
    Dim sha1 As HashAlgorithm = HashAlgorithm.Create(alg)

    If sha1 IsNot Nothing Then
      Using strm As Stream = File.OpenRead(filepath)
        Dim hash() As Byte = sha1.ComputeHash(strm)
        strm.Close()
        Dim pFix As New PremisFixity(alg, MetadataFunctions.BytesToHexStr(hash))
        pObjChar.Fixities.Add(pFix)
      End Using
    End If

    Dim fInfo As New FileInfo(filepath)

    pObjChar.Size = fInfo.Length

    Dim mime As String = MetadataFunctions.GetMimeFromFile(filepath, proposedMime)
    If Not String.IsNullOrWhiteSpace(mime) Then
      Dim pForm As New PremisFormat(mime)
      pObjChar.Formats.Add(pForm)
    End If

    Return pObjChar

  End Function


End Class
