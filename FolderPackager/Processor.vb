Imports System.Xml
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Configuration
Imports System.Text
Imports Uiuc.Library.Premis
Imports Uiuc.Library.MetadataUtilities
Imports Uiuc.Library.CollectionsRegistry
Imports Uiuc.Library.Ldap

Public Class Processor
  Private _idMap As New Dictionary(Of String, String)
  Private _handleMap As New Dictionary(Of Uri, KeyValuePair(Of Integer, String))

  ''' <summary>
  ''' Process the source folder with all files belonging to the given collection
  ''' </summary>
  ''' <param name="collHandle"></param>
  ''' <remarks></remarks>
  ''' 
  Public Sub StartProcess(ByVal collHandle As String)

    Dim folderName As String = MedusaAppSettings.Settings.SourceFolder

    If File.Exists(MedusaAppSettings.Settings.IdMapFilePath) Then
      LoadIdMap(MedusaAppSettings.Settings.IdMapFilePath)
    End If

    If File.Exists(MedusaAppSettings.Settings.HandleMapFilePath) Then
      LoadHandleMap(MedusaAppSettings.Settings.HandleMapFilePath)
    End If

    Dim processor As New FolderProcessor(collHandle, _idMap, _handleMap)
    processor.ProcessFolder(folderName)

    SaveIdMap(MedusaAppSettings.Settings.IdMapFilePath)
    SaveHandleMap(MedusaAppSettings.Settings.HandleMapFilePath)

  End Sub

  Private Sub SaveIdMap(fileName As String)
    'If Not File.Exists(fileName) Then
    Dim fs As New StreamWriter(fileName)
    For Each k In _idMap
      fs.WriteLine(String.Format("{0},{1}", k.Key, k.Value))
    Next
    fs.Close()
    'End If
  End Sub

  Private Sub SaveHandleMap(fileName As String)
    'If Not File.Exists(fileName) Then
    Dim fs As New StreamWriter(fileName)
    For Each k In _handleMap
      fs.WriteLine(String.Format("{0},{1},{2}", k.Value.Key, k.Key, k.Value.Value))
    Next
    fs.Close()
    'End If
  End Sub

  Private Sub LoadIdMap(filename As String)
    Dim fs As New StreamReader(filename)
    Do Until fs.EndOfStream
      Dim ln As String = fs.ReadLine
      If Not ln.StartsWith("#") Then
        Dim parts() As String = ln.Split(",", 2, StringSplitOptions.RemoveEmptyEntries)
        _idMap.Add(parts(0), parts(1))
      End If
    Loop
    fs.Close()
  End Sub

  Private Sub LoadHandleMap(filename As String)
    Dim fs As New StreamReader(filename)
    Do Until fs.EndOfStream
      Dim ln As String = fs.ReadLine
      If Not ln.StartsWith("#") Then
        Dim parts() As String = ln.Split(",", 3, StringSplitOptions.RemoveEmptyEntries)
        _handleMap.Add(New Uri(parts(1)), New KeyValuePair(Of Integer, String)(parts(0), parts(2)))
      End If
    Loop
    fs.Close()
  End Sub

  ''' <summary>
  ''' Create a collection record package for the exported ContentDM collection.
  ''' If there is a CollectionId value it will be used to fetch the collection record from the Collection Registry.  Otherwise,
  ''' if there is a value for the CollectionHandle AppSetting that CollectionHandle AppSetting will be used for all the records.  
  ''' If the CollectionHandle AppSetting
  ''' does not have a value, a new CollectionHandle will be created and used for the new collection record.  The 
  ''' CollectionHandle AppSetting should be set to this new Handle value for any subsequent runs of this script.
  ''' </summary>
  ''' <returns></returns>
  ''' <remarks>This will just be a stub record and should be manually enhanced, if applicable.</remarks>
  Public Function ProcessCollection() As String
    'TODO:  Use the collection registry ID to capture the collection mods record

    Directory.CreateDirectory("collection")

    Dim collId As String = MedusaAppSettings.Settings.CollectionId

    Dim collrec As CollectionRecord = Nothing
    If Not String.IsNullOrWhiteSpace(collId) Then
      collrec = New CollectionRecord(collId, "collection")
    End If

    Dim collHandle As String = ""
    If collrec IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(collrec.Uuid) Then
      collHandle = String.Format("{0}:{1}", MedusaAppSettings.Settings.HandleProject, collrec.Uuid)
    Else
      collHandle = MedusaAppSettings.Settings.CollectionHandle
    End If

    If collrec Is Nothing Or String.IsNullOrWhiteSpace(collrec.Uuid) Then
      'need to create collection record
      Dim pth As String = MedusaAppSettings.Settings.CollectionDescriptionPath
      Dim collDescr As String = File.ReadAllText(pth)

      Dim newFPath As String = Path.Combine("collection", "mods.xml")
      'create mods record for the collection
      Dim xmlwr As XmlWriter = XmlWriter.Create(newFPath, New XmlWriterSettings With {.Indent = True, .Encoding = Encoding.UTF8, .OmitXmlDeclaration = True})
      xmlwr.WriteStartElement("mods", "http://www.loc.gov/mods/v3")
      xmlwr.WriteAttributeString("version", "3.4")
      xmlwr.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.loc.gov/mods/v3 http://www.loc.gov/standards/mods/mods.xsd")
      xmlwr.WriteComment("This collection mods record should be edited by hand to most accurately reflect the collection")
      xmlwr.WriteStartElement("typeOfResource")
      xmlwr.WriteAttributeString("collection", "yes")
      xmlwr.WriteString("mixed material")
      xmlwr.WriteEndElement()
      xmlwr.WriteStartElement("titleInfo")
      xmlwr.WriteElementString("title", MedusaAppSettings.Settings.CollectionName)
      xmlwr.WriteEndElement()
      xmlwr.WriteElementString("abstract", collDescr)
      xmlwr.WriteStartElement("location")
      xmlwr.WriteStartElement("url")
      xmlwr.WriteAttributeString("access", "object in context")
      xmlwr.WriteAttributeString("usage", "primary")
      xmlwr.WriteString(MedusaAppSettings.Settings.CollectionURL)
      xmlwr.WriteEndElement()
      xmlwr.WriteEndElement()
      xmlwr.Close()
    End If

    'Create a PREMIS metadata for the collection
    If String.IsNullOrWhiteSpace(collHandle) Then
      collHandle = MetadataFunctions.GenerateLocalIdentifier
    End If
    Dim idType As String = MetadataFunctions.GetIdType(collHandle)

    If MetadataFunctions.ValidateHandle(collHandle) = False Then
      Trace.TraceError("Invalid Handle: " & collHandle)
      Trace.Flush()
      Throw New PackagerException("Invalid Handle: " & collHandle)
    End If

    Dim pObj As New PremisObject(idType, collHandle, PremisObjectCategory.Representation)
    pObj.ObjectIdentifiers.Add(New PremisIdentifier("URL", MedusaAppSettings.Settings.CollectionURL))
    If collrec IsNot Nothing Then
      pObj.ObjectIdentifiers.Add(New PremisIdentifier("URL", collrec.Url))
    End If
    Dim pContainer As PremisContainer = New PremisContainer(pObj)
    pContainer.IDPrefix = collHandle & MedusaAppSettings.Settings.HandleLocalIdSeparator

    Dim pUserAgent As PremisAgent = UIUCLDAPUser.GetPremisAgent
    Dim pSoftAgent As PremisAgent = PremisAgent.GetCurrentSoftwareAgent("LOCAL", pContainer.NextID)
    pUserAgent.AgentIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
    pContainer.Agents.Add(pUserAgent)

    Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CREATION")
    pEvt.LinkToAgent(pUserAgent)
    pEvt.LinkToAgent(pSoftAgent)
    pEvt.LinkToObject(pObj)
    pContainer.Events.Add(pEvt)

    Dim pObj2 As New PremisObject("FILENAME", "mods.xml", PremisObjectCategory.File, "text/xml")
    pObj2.ObjectIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
    If collrec IsNot Nothing And collrec.ModsXml IsNot Nothing Then
      pObj2.ObjectCharacteristics.Add(collrec.PremisObjectCharacteristics)
    End If
    pContainer.Objects.Add(pObj2)

    'rename mods file to use uuid
    Dim newFName As String = pObj2.GetDefaultFileName("mods_", ".xml")
    If File.Exists(Path.Combine("collection", newFName)) Then
      File.Delete(Path.Combine("collection", newFName))
    End If
    Rename(Path.Combine("collection", "mods.xml"), Path.Combine("collection", newFName))
    pObj2.GetFilenameIdentifier.IdentifierValue = newFName

    Dim pEvt2 As PremisEvent
    If collrec Is Nothing Then
      pEvt2 = New PremisEvent("LOCAL", pContainer.NextID, "CREATION")
      pEvt2.EventDetail = String.Format("The {0} file was derived from preconfigured collection data.  It is expected to be manually edited to add data.", newFName)
    Else
      pEvt2 = collrec.PremisEvent
      pEvt2.EventIdentifier.IdentifierType = "LOCAL"
      pEvt2.EventIdentifier.IdentifierValue = pContainer.NextID
    End If
    pEvt2.LinkToAgent(pUserAgent)
    pEvt2.LinkToAgent(pSoftAgent)
    pEvt2.LinkToObject(pObj2)
    pContainer.Events.Add(pEvt2)


    pObj.RelateToObject("METADATA", "HAS_ROOT", pObj2)

    If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationRights) Then
      Dim pRgtStmt As New PremisRightsStatement("LOCAL", pContainer.NextID, MedusaAppSettings.Settings.PremisDisseminationRightsBasis)
      pRgtStmt.RightsGranted.Add(New PremisRightsGranted(MedusaAppSettings.Settings.PremisDisseminationRights))
      pRgtStmt.LinkToObject(pObj)
      If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationRightsRestrictions) Then
        pRgtStmt.RightsGranted.FirstOrDefault.Restrictions.Add(MedusaAppSettings.Settings.PremisDisseminationRightsRestrictions)
      End If
      If MedusaAppSettings.Settings.PremisDisseminationRightsBasis = MedusaAppSettings.COPYRIGHT And
        Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.PremisDisseminationCopyrightStatus) Then
        Dim cpyRt As New PremisCopyrightInformation(MedusaAppSettings.Settings.PremisDisseminationCopyrightStatus, "United States")
        pRgtStmt.CopyrightInformation = cpyRt
      End If
      Dim pRt As New PremisRights(pRgtStmt)
      pContainer.Rights.Add(pRt)
    End If

    pContainer.SaveXML(Path.Combine("collection", pObj.GetDefaultFileName("premis_object_", "xml")))

    'pContainer.SaveEachXML(Path.Combine("collection", "_").TrimEnd("_"))


    Return collHandle
  End Function


End Class
