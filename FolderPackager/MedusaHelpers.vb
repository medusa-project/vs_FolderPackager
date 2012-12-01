Imports System.IO
Imports Uiuc.Library.Premis
Imports Uiuc.Library.MetadataUtilities
Imports Uiuc.Library.Ldap
Imports System.Text.RegularExpressions
Imports System.Configuration
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Net.Mail

''' <summary>
''' In order to keep the core Premis classes as generic as possible, any code that is unique to the Medusa Repository project should go here.
''' </summary>
''' <remarks></remarks>
Public Class MedusaHelpers


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

    MedusaHelpers.PartitionContainer(rootObj, rootPath, conList)

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
          Throw New PackagerException(String.Format("Unexpected SaveFileAs Setting '{0}'.", MedusaAppSettings.Settings.SaveFilesAs))
        End If

      Next
    Else
      For Each kv As KeyValuePair(Of String, PremisContainer) In conList

        If MedusaAppSettings.Settings.SaveFilesAs = SaveFileAsType.MEDUSA Then
          kv.Value.SaveXML(Path.Combine(folder, kv.Value.Objects.First.GetDefaultFileName("premis_", "xml")))
        ElseIf MedusaAppSettings.Settings.SaveFilesAs = SaveFileAsType.MEDUSA_MULTIPLE Then
          kv.Value.SaveEachXML(folder)
        Else
          Throw New PackagerException(String.Format("Unexpected SaveFileAs Setting '{0}'.", MedusaAppSettings.Settings.SaveFilesAs))
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
  Private Shared Sub PartitionContainer(rootObj As PremisObject, rootDir As String, partList As List(Of KeyValuePair(Of String, PremisContainer)))

    Dim newCont As New PremisContainer()
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
              Throw New PackagerException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "METADATA"
          'metadata is kept with its related objects so do nothing

          Select Case r.RelationshipSubType
            Case "HAS_ROOT", "MARC", "DC_RDF", "SPREADSHEET"

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New PackagerException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "DERIVATION"
          'derivations are kept with their related object and currently only apply to metadata so do nothing

          Select Case r.RelationshipSubType
            Case "HAS_SOURCE", "IS_SOURCE_OF"

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New PackagerException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "BASIC_IMAGE_ASSET"
          'basic image assets are partitioned into separate fedora objects so create new subdir and corresponding premis container

          Select Case r.RelationshipSubType
            Case "PRODUCTION_MASTER", "ARCHIVAL_MASTER", "SCREEN_SIZE", "THUMBNAIL"
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(rootDir, o.GetDefaultFileNameIndex), partList)
              Next

            Case "PARENT"
              'parents should have already been created

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New PackagerException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "BASIC_FILE_ASSET"
          'basic file assets are partitioned into separate fedora objects so create new subdir and corresponding premis container

          Select Case r.RelationshipSubType
            Case "UNSPECIFIED"
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(rootDir, o.GetDefaultFileNameIndex), partList)
              Next

            Case "PARENT"
              'parents should have already been created

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New PackagerException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case "BASIC_COMPOUND_ASSET"
          'compound assets are partitioned into separate containers, subdirectories are created depending on the relationship

          Select Case r.RelationshipSubType
            Case "FIRST_CHILD", "CHILD"
              'create subdirectory for this container
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(rootDir, o.GetDefaultFileNameIndex), partList)
              Next

            Case "NEXT_SIBLING"
              'use same subdirectory for this container
              For Each o As PremisObject In r.RelatedObjects
                MedusaHelpers.PartitionContainer(o, Path.Combine(Path.GetDirectoryName(rootDir), o.GetDefaultFileNameIndex), partList)
              Next

            Case "PREVIOUS_SIBLING"
              'previous siblings should have already been created

            Case "PARENT"
              'parents should have already been created

            Case Else
              Trace.TraceError(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))
              Trace.Flush()
              Throw New PackagerException(String.Format("Unexpected PREMIS Relationship Sub-type: {0}/{1}", r.RelationshipType, r.RelationshipSubType))

          End Select

        Case Else
          Trace.TraceError(String.Format("Unexpected PREMIS Relationship Type: {0}", r.RelationshipType))
          Trace.Flush()
          Throw New PackagerException(String.Format("Unexpected PREMIS Relationship Type: {0}", r.RelationshipType))
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

    If Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataMarcRegex, RegexOptions.IgnoreCase) Then
      ret = "METADATA"
    ElseIf Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataDcRdfRegex, RegexOptions.IgnoreCase) Then
      ret = "METADATA"
    ElseIf Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataSpreadsheetRegex, RegexOptions.IgnoreCase) Then
      ret = "METADATA"
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
      If Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataMarcRegex, RegexOptions.IgnoreCase) Then
        ret = "MARC"
      ElseIf Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataDcRdfRegex, RegexOptions.IgnoreCase) Then
        ret = "DC_RDF"
      ElseIf Regex.IsMatch(filename, MedusaAppSettings.Settings.MetadataSpreadsheetRegex, RegexOptions.IgnoreCase) Then
        ret = "SPREADSHEET"
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

End Class
