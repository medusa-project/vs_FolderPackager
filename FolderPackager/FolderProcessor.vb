Imports System.Xml
Imports System.Xml.Xsl
Imports System.Xml.Schema
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Configuration
Imports System.Net
Imports System.Text
Imports System.Security
Imports System.Security.Cryptography
Imports System.Reflection
Imports Uiuc.Library.Premis
Imports Uiuc.Library.Ldap
Imports Uiuc.Library.MetadataUtilities
Imports Uiuc.Library.HandleClient
Imports Uiuc.Library.Fits

''' <summary>
''' Process a single ContentDM record to create a Medusa Submission Package
''' </summary>
''' <remarks></remarks>
Public Class FolderProcessor

  'TOOD: Verify the SHA1 checksums

  Const MAX_RETRY_COUNT = 5

  Private pContainer As PremisContainer
  Private pCurrentAgent As PremisAgent
  Private pRepresentation As PremisObject
  Private collHandle As String

  Private folderStack As Stack(Of String)
  Private objectFolderLevel As Integer
  Private baseDestFolder As String
  Private baseSrcFolder As String
  Private fileNum As Integer = 0

  Private IdMap As Dictionary(Of String, String)
  Private HandleMap As Dictionary(Of Uri, KeyValuePair(Of Integer, String))

  Protected Sub New()
    'no public empty constructor is allowed
  End Sub

  Sub New(ByVal collectionHandle As String, im As Dictionary(Of String, String), hm As Dictionary(Of Uri, KeyValuePair(Of Integer, String)))
    collHandle = collectionHandle
    IdMap = im
    HandleMap = hm
    folderStack = New Stack(Of String)
    baseDestFolder = ConfigurationManager.AppSettings.Item("WorkingFolder")
    baseSrcFolder = ConfigurationManager.AppSettings.Item("SourceFolder")
    objectFolderLevel = Integer.Parse(ConfigurationManager.AppSettings.Item("ObjectFolderLevel"))
  End Sub



  ''' <summary>
  ''' Process the folder
  ''' </summary>
  ''' <param name="sourceFolder">the absolute path to the folder to process</param>
  ''' <remarks></remarks>
  Public Sub ProcessFolder(sourceFolder As String, Optional destFolder As String = "", Optional parentObject As PremisObject = Nothing)

    fileNum = fileNum + 1

    If String.IsNullOrWhiteSpace(destFolder) Then destFolder = Path.Combine(baseDestFolder, "data")

    Dim destPath As String = Path.Combine(destFolder, Path.GetFileName(sourceFolder))

    folderStack.Push(destPath)
    Directory.CreateDirectory(destPath)


    If folderStack.Count = objectFolderLevel Then
      'this folder represent a top-level or root object

      Console.Out.WriteLine(String.Format("Root Folder: {0}", sourceFolder))

      Dim handle As String
      If IdMap.ContainsKey(sourceFolder) Then
        'use the already minted handle 
        handle = IdMap.Item(sourceFolder)
        If Not MetadataFunctions.ValidateHandle(handle) Then
          Trace.TraceError("Invalid Handle: " & handle)
          Trace.Flush()
          Throw New PackagerException("Invalid Handle: " & handle)
        End If
      Else
        'Create a PREMIS metadata for the record
        handle = MetadataFunctions.GenerateLocalIdentifier
        IdMap.Add(sourceFolder, handle)
      End If

      Dim idType As String = "LOCAL"
      Dim localId As String = handle
      If handle.StartsWith(ConfigurationManager.AppSettings.Item("Handle.Prefix") & "/") Then
        idType = "HANDLE"
        localId = MetadataFunctions.GetLocalIdentifier(handle)
      End If

      pRepresentation = New PremisObject(idType, handle, PremisObjectCategory.Representation)
      pRepresentation.XmlId = String.Format("folder_{0}", fileNum)

      If objectFolderLevel = 1 Then 'this is just the data folder, need to create another subfolder for this object
        destPath = Path.Combine(destPath, pRepresentation.GetDefaultFileName("", ""))
        Directory.CreateDirectory(destPath)
        folderStack.Push(destPath)
      Else
        'rename the folder based on the local identifier of the premis rep object
        Try
          My.Computer.FileSystem.RenameDirectory(destPath, pRepresentation.GetDefaultFileName("", ""))
        Catch ex As IOException
          If ConfigurationManager.AppSettings.Item("OverwriteObjects").ToUpper = "TRUE" Then
            'Dest folder already exists, so just delete the orig folder
            My.Computer.FileSystem.DeleteDirectory(destPath, FileIO.DeleteDirectoryOption.ThrowIfDirectoryNonEmpty)
            Trace.TraceWarning("Folder '{1}' already exists.  Original folder '{0}' will be deleted.", destPath, pRepresentation.GetDefaultFileName("", ""))
          Else
            Trace.TraceError("Folder '{1}' already exists.", destPath, pRepresentation.GetDefaultFileName("", ""))
            Trace.Flush()
            Throw New PackagerException(String.Format("Folder '{1}' already exists.", destPath, pRepresentation.GetDefaultFileName("", "")), ex)
          End If
        End Try
        destPath = Path.Combine(Path.GetDirectoryName(destPath), pRepresentation.GetDefaultFileName("", ""))
        folderStack.Pop()
        folderStack.Push(destPath)
      End If


      pContainer = New PremisContainer(pRepresentation)
      pContainer.IDPrefix = handle & ConfigurationManager.AppSettings.Item("Handle.LocalIdSeparator")
      Dim presLvl As New PremisPreservationLevel("BIT_LEVEL", Now)
      presLvl.PreservationLevelRationale.Add("Uncategorized file system capture")
      pRepresentation.PreservationLevels.Add(presLvl)
      pRepresentation.OriginalName = sourceFolder

      pCurrentAgent = UIUCLDAPUser.GetPremisAgent

      'register a handle for the root object 
      If ConfigurationManager.AppSettings.Item("Handle.Generation").ToUpper = "ROOT_OBJECT_AND_FILES" Or ConfigurationManager.AppSettings.Item("Handle.Generation").ToUpper = "ROOT_OBJECT_ONLY" Then
        Dim regHandle As String = RegisterFileHandle(destPath, pRepresentation)
      End If

      Dim files = Directory.EnumerateFiles(sourceFolder)
      For Each fl In files
        Dim fat = File.GetAttributes(fl)
        If Not (fat.HasFlag(FileAttributes.System) Or fat.HasFlag(FileAttributes.Hidden)) Then
          CaptureFile(fl, destPath, pRepresentation, "BASIC_FILE_ASSET", "UNSPECIFIED")
        End If
      Next

      Dim folders = Directory.EnumerateDirectories(sourceFolder)
      For Each fld In folders
        ProcessFolder(fld, destPath, pRepresentation)
      Next

      If Not String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("PremisDisseminationRights")) Then
        Dim pRgtStmt As New PremisRightsStatement("LOCAL", pContainer.NextID, ConfigurationManager.AppSettings.Item("PremisDisseminationRightsBasis"))
        pRgtStmt.RightsGranted.Add(New PremisRightsGranted(ConfigurationManager.AppSettings.Item("PremisDisseminationRights")))
        pRgtStmt.LinkToObject(pRepresentation)
        If Not String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("PremisDisseminationRightsRestrictions")) Then
          pRgtStmt.RightsGranted.FirstOrDefault.Restrictions.Add(ConfigurationManager.AppSettings.Item("PremisDisseminationRightsRestrictions"))
        End If
        If ConfigurationManager.AppSettings.Item("PremisDisseminationRightsBasis") = "COPYRIGHT" And
          Not String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("PremisDisseminationCopyrightStatus")) Then
          Dim cpyRt As New PremisCopyrightInformation(ConfigurationManager.AppSettings.Item("PremisDisseminationCopyrightStatus"), "United States")
          pRgtStmt.CopyrightInformation = cpyRt
        End If
        Dim pRt As New PremisRights(pRgtStmt)
        pContainer.Rights.Add(pRt)
      End If

      idType = "LOCAL"
      If collHandle.StartsWith(ConfigurationManager.AppSettings.Item("Handle.Prefix") & "/") Then
        idType = "HANDLE"
      End If

      pRepresentation.RelateToObject("COLLECTION", "IS_MEMBER_OF", idType, collHandle)

      pCurrentAgent.AgentIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
      pContainer.Agents.Add(pCurrentAgent)

      Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CREATION")
      'include details about packager program here
      pEvt.EventDetail = String.Format("SIP created from a file system folder: '{7}'.  Program: {0} {1} [{2}], Computer: {3}, {4} V{5}, {6}",
                                       My.Application.Info.Title, My.Application.Info.Version, My.Application.Info.CompanyName, My.Computer.Name, My.Computer.Info.OSFullName.Trim,
                                       My.Computer.Info.OSVersion, My.Application.UICulture.EnglishName, sourceFolder)
      pEvt.LinkToAgent(pCurrentAgent)
      pEvt.LinkToObject(pRepresentation)
      pContainer.Events.Add(pEvt)


      Select Case ConfigurationManager.AppSettings.Item("SaveFilesAs").ToLower
        Case "one"
          'save one big premis file for the whole object
          pContainer.SaveXML(Path.Combine(destPath, pContainer.Objects.First.GetDefaultFileName("premis_", "xml")))
        Case "multiple"
          'save a separate file for each premis entity
          pContainer.SaveEachXML(destPath)
        Case "representations"
          'create directory structure and save a separate premis file for each premis representation object and associated entities, 
          'also save a premis file for each file object (not metadata)
          'TODO:  This function has not been tested for the folder packager, so should notbe used yet
          pContainer.SavePartitionedXML(destPath, True)
        Case "medusa"
          'create directory structure and save a separate premis file for each fedora object as defined for our medusa content model
          'pContainer.SaveXML(Path.Combine(recPath, pContainer.Objects.First.GetDefaultFileName("test_premis_", "xml")))
          MedusaHelpers.SavePartitionedXML(pContainer, destPath, True)
        Case Else
          'save one big premis file for the whole object
          pContainer.SaveXML(Path.Combine(destPath, pContainer.Objects.First.GetDefaultFileName("premis_", "xml")))
      End Select

    ElseIf folderStack.Count < objectFolderLevel Then
      'this folder represents an intervening directory, files in this directory are ignored, sub folders are processed

      Console.Out.WriteLine(String.Format("Ignoring Folder: {0}", sourceFolder))

      Dim folders = Directory.EnumerateDirectories(sourceFolder)
      For Each fld In folders
        ProcessFolder(fld, destPath)
      Next

    ElseIf folderStack.Count > objectFolderLevel Then
      'this folder represents the components of an object, all its files and subfolders are processed

      Console.Out.WriteLine(String.Format("Subfolder: {0}", sourceFolder))

      'create representation object for folder
      Dim pFolderObj As New PremisObject("LOCAL", pContainer.NextID, PremisObjectCategory.Representation)
      pFolderObj.XmlId = String.Format("folder_{0}", fileNum)

      'Rename the folder at this point to use the local suffix index value
      Try
        My.Computer.FileSystem.RenameDirectory(destPath, pFolderObj.GetDefaultFileNameIndex)
      Catch ex As IOException
        If ConfigurationManager.AppSettings.Item("OverwriteObjects").ToUpper = "TRUE" Then
          'Dest folder already exists, so just delete the orig folder
          My.Computer.FileSystem.DeleteDirectory(destPath, FileIO.DeleteDirectoryOption.ThrowIfDirectoryNonEmpty)
          Trace.TraceWarning("Folder '{1}' already exists.  Original folder '{0}' will be deleted.", destPath, pFolderObj.GetDefaultFileName("", ""))
        Else
          Trace.TraceError("Folder '{1}' already exists.", destPath, pFolderObj.GetDefaultFileName("", ""))
          Trace.Flush()
          Throw New PackagerException(String.Format("Folder '{1}' already exists.", destPath, pFolderObj.GetDefaultFileName("", "")), ex)
        End If
      End Try
      destPath = Path.Combine(Path.GetDirectoryName(destPath), pFolderObj.GetDefaultFileNameIndex)
      folderStack.Pop()
      folderStack.Push(destPath)

      'add a folder name identifier which is the path relative to the root folder for this object
      Dim pId As New PremisIdentifier("FOLDERNAME", GetRelativePathTo(destPath))
      pFolderObj.ObjectIdentifiers.Add(pId)
      pFolderObj.OriginalName = sourceFolder


      pFolderObj.RelateToObject("BASIC_COMPOUND_ASSET", "PARENT", parentObject)
      parentObject.RelateToObject("BASIC_COMPOUND_ASSET", "CHILD", pFolderObj)

      pContainer.Objects.Add(pFolderObj)

      Dim files = Directory.EnumerateFiles(sourceFolder)
      For Each fl In files
        Dim fat = File.GetAttributes(fl)
        If Not (fat.HasFlag(FileAttributes.System) Or fat.HasFlag(FileAttributes.Hidden)) Then
          CaptureFile(fl, destPath, pFolderObj, "BASIC_FILE_ASSET", "UNSPECIFIED")
        End If
      Next

      Dim folders = Directory.EnumerateDirectories(sourceFolder)
      For Each fld In folders
        ProcessFolder(fld, destPath, pFolderObj)
      Next
    End If

    folderStack.Pop()
  End Sub

  ''' <summary>
  ''' Capture the file, copy it to the destination folder
  ''' </summary>
  ''' <param name="filename">the path to the file to capture</param>
  ''' <remarks></remarks>
  Private Sub CaptureFile(filename As String, recpath As String, parent As PremisObject, relat As String, subRelat As String)
    fileNum = fileNum + 1

    Console.Out.WriteLine(String.Format("File: {0}", filename))

    Dim i As Integer = 0

    Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CAPTURE")
    pEvt.LinkToAgent(pCurrentAgent)
    pEvt.EventDetail = String.Format("Fetching File: {0}", filename)

    pContainer.Events.Add(pEvt)

    Dim pObjChars As New PremisObjectCharacteristics()

    Dim pEvtFits As PremisEvent = Nothing
    If String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("DoFits")) OrElse ConfigurationManager.AppSettings.Item("DoFits").ToUpper = "TRUE" Then
      Dim fts As New FitsResult(filename)
      pObjChars.ObjectCharacteristicsExtensions.Add(fts.FitsXml)
      pObjChars.Formats.AddRange(fts.PremisFormats)
      pObjChars.Fixities.AddRange(fts.PremisFixities)
      pObjChars.CreatingApplications.AddRange(fts.PremisCreatingApplications)

      pEvtFits = fts.GetPremisEvent(New PremisIdentifier("LOCAL", pContainer.NextID))
      pContainer.Events.Add(pEvtFits)

      pEvtFits.LinkToAgent(pCurrentAgent)
    End If

    Dim capFile As String = FetchFile(filename, recpath, pEvt, pObjChars)

    If (pObjChars.Formats.Count = 1 AndAlso pObjChars.Formats.Item(0).FormatName = "application/octet-stream") Then
      'add event outcome for unknown format
      Dim pevtOut As New PremisEventOutcomeInformation("UNKNOWN")
      Dim pEvtDet As New PremisEventOutcomeDetail("The MIME type of the file cannot be determined; 'application/octet-stream' is used.")
      pevtOut.EventOutcomeDetails.Add(pEvtDet)
      If pEvtFits IsNot Nothing Then pEvtFits.EventOutcomeInformation.Add(pevtOut)
    End If

    If MedusaHelpers.GetDistinctMimeTypes(pObjChars.Formats).Count > 1 Then
      'add event outcome for conflicting format
      Dim pevtOut As New PremisEventOutcomeInformation("MIME_CONFLICTS")
      Dim pEvtDet As New PremisEventOutcomeDetail("Multiple alternate MIME types were detected by different characterization tools.")
      pevtOut.EventOutcomeDetails.Add(pEvtDet)
      If pEvtFits IsNot Nothing Then pEvtFits.EventOutcomeInformation.Add(pevtOut)
    End If

    If Not String.IsNullOrWhiteSpace(capFile) Then

      Dim pObj As New PremisObject("FILENAME", capFile, PremisObjectCategory.File, pObjChars)
      pObj.PreservationLevels.Add(New PremisPreservationLevel("ORIGINAL_CONTENT_FILE", Now))
      pObj.OriginalName = filename
      pObj.ObjectIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
      pObj.XmlId = String.Format("file_{0}", fileNum)

      'rename the file to use the uuid and the relative path
      Dim newFName As String = pObj.GetDefaultFileName("file_", Path.GetExtension(filename))
      Dim newFPath As String = Path.Combine(recpath, newFName)
      'If file already exists -- delete it and log a warning
      If File.Exists(newFPath) Then
        If ConfigurationManager.AppSettings.Item("OverwriteObjects").ToUpper = "TRUE" Then
          File.Delete(newFPath)
          Trace.TraceWarning("File '{0}' already exists.  It will be deleted and replaced with the new file of same name.", newFPath)
        Else
          Trace.TraceError("File '{0}' already exists.", newFPath)
          Trace.Flush()
          Throw New PackagerException(String.Format("File '{0}' already exists.", newFPath))
        End If
      End If

      Rename(Path.Combine(recpath, capFile), newFPath)
      pObj.GetFilenameIdentifier.IdentifierValue = GetRelativePathTo(newFPath)

      pObj.LinkToEvent(pEvt)
      If pEvtFits IsNot Nothing Then pObj.LinkToEvent(pEvtFits)

      pContainer.Objects.Add(pObj)
      parent.RelateToObject(relat, subRelat, pObj)
      pObj.RelateToObject(relat, "PARENT", parent)

      'register a handle for the file
      If ConfigurationManager.AppSettings.Item("Handle.Generation").ToUpper = "ROOT_OBJECT_AND_FILES" Or ConfigurationManager.AppSettings.Item("Handle.Generation").ToUpper = "FILES_ONLY" Then
        Dim handle As String = RegisterFileHandle(newFPath, pObj)
      End If


      End If
      i = i + 1

  End Sub

  ''' <summary>
  ''' Register a handle for the file or folder object
  ''' </summary>
  ''' <param name="newFPath"></param>
  ''' <param name="pObj"></param>
  ''' <returns></returns>
  ''' <remarks>This function requires the pContainer and pCurrentAgent be instantiated</remarks>
  Private Function RegisterFileHandle(newFPath As String, pObj As PremisObject) As String
    Dim urib As New UriBuilder
    urib.Scheme = "file"
    urib.Path = newFPath

    'register a handle for the URI if not already done
    Dim handle As String = ""
    Dim local_id As String
    If HandleMap.ContainsKey(urib.Uri) Then
      'use the already registered handle 
      handle = HandleMap.Item(urib.Uri).Value
      local_id = MetadataFunctions.GetLocalIdentifier(handle)
    Else
      local_id = pObj.LocalIdentifierValue
    End If

    'Register a new handle for the file, if needed
    Try
      Dim hc As HandleClient = HandleClient.CreateUpdateHandle(local_id, urib.Uri.ToString, pCurrentAgent.EmailIdentifierValue, _
                                                                                      String.Format("Collection: {0}", ConfigurationManager.AppSettings.Item("CollectionName")))
      handle = hc.handle_value
      If Not MetadataFunctions.ValidateHandle(handle) Then
        Trace.TraceError("Invalid Handle: " & handle)
        Trace.Flush()
        Throw New PackagerException("Invalid Handle: " & handle)
      End If

      If Not HandleMap.ContainsKey(urib.Uri) Then HandleMap.Add(urib.Uri, New KeyValuePair(Of Integer, String)(fileNum, handle))
      pObj.ObjectIdentifiers.Add(New PremisIdentifier("HANDLE", handle))

      Dim hEvt As PremisEvent = hc.GetPremisEvent(New PremisIdentifier("LOCAL", pContainer.NextID))
      hEvt.LinkToObject(pObj)
      hEvt.LinkToAgent(pCurrentAgent)

      pContainer.Events.Add(hEvt)

    Catch ex As Exception
      Trace.TraceError("Handle Error: {0}", ex.Message)

      Dim hEvt As New PremisEvent("LOCAL", pContainer.NextID, "HANDLE_CREATION")
      Dim hEvtOut As New PremisEventOutcomeInformation("FAILED")
      hEvtOut.EventOutcomeDetails.Add(New PremisEventOutcomeDetail(ex.Message))
      hEvt.EventOutcomeInformation.Add(hEvtOut)
      hEvt.LinkToObject(pObj)
      hEvt.LinkToAgent(pCurrentAgent)

      pContainer.Events.Add(hEvt)

    End Try

    Return handle

  End Function

  'TODO: Move this to a common library
  Private Shared Function BytesToStr(ByVal bytes() As Byte) As String
    Dim str As StringBuilder = New StringBuilder
    Dim i As Integer = 0
    Do While (i < bytes.Length)
      str.AppendFormat("{0:X2}", bytes(i))
      i = (i + 1)
    Loop
    Return str.ToString
  End Function

  'TODO: may want to explore asynchronous calls here instead of synchronous

  Private Function FetchFile(ByVal filename As String, destPath As String, ByVal pEvt As PremisEvent, ByVal pObjChar As PremisObjectCharacteristics, Optional ByVal saveFile As String = "") As String

    Dim finfo As New FileInfo(filename)

    Dim urib As New UriBuilder(filename)
    Dim size As Long = 0

    Dim httpReq As WebRequest = WebRequest.Create(urib.Uri)
    Dim httpRsp As WebResponse = Nothing

    Dim tries As Integer = 0
    Do Until tries > MAX_RETRY_COUNT
      Try
        httpRsp = httpReq.GetResponse
        Exit Do
      Catch ex As WebException
        If tries >= MAX_RETRY_COUNT Then
          Trace.TraceError("Error in Folder: {0}", destPath)
          Trace.TraceError("Fetching File: {1} -- {0}", ex.Message, filename)

          Dim evtInfo As New PremisEventOutcomeInformation([Enum].GetName(GetType(WebExceptionStatus), ex.Status))
          Dim evtDet As New PremisEventOutcomeDetail(ex.Message)
          evtInfo.EventOutcomeDetails.Add(evtDet)
          pEvt.EventOutcomeInformation.Add(evtInfo)
          Return ""
          Exit Function
        Else
          Trace.TraceInformation(String.Format("{2} Retrying FetchFile: {0}.  Try: {1}", urib.Uri, tries, destPath))
        End If
      Catch ex As Exception
        Trace.TraceError("Error in Folder: {0}", destPath)
        Trace.TraceError("Fetching File: {1} -- {0}", ex.Message, filename)

        Dim evtInfo As New PremisEventOutcomeInformation(ex.GetType.Name)
        Dim evtDet As New PremisEventOutcomeDetail(ex.Message)
        evtInfo.EventOutcomeDetails.Add(evtDet)
        pEvt.EventOutcomeInformation.Add(evtInfo)
        Return ""
        Exit Function
      End Try
      Threading.Thread.Sleep(((3 ^ tries) - 1) * 1000)
      tries = tries + 1
      httpReq = WebRequest.Create(urib.Uri)
    Loop

    Dim http_mime As String = httpRsp.ContentType 'for filewebresponse this is always application/octet-stream so just ignore it
    size = httpRsp.ContentLength

    Dim outFileName As String
    If String.IsNullOrWhiteSpace(saveFile) Then
      Dim fn As String = Path.GetFileName(urib.Uri.LocalPath)
      'replace invalid chars in name
      fn = Regex.Replace(fn, String.Format("[{0}]", New String(Path.GetInvalidFileNameChars)), "_") 'TODO: This could result in filename collisions
      outFileName = Path.Combine(destPath, fn)
    Else
      outFileName = saveFile
    End If

    Dim alg As String = ConfigurationManager.AppSettings.Item("ChecksumAlgorithm")

    If alg <> "NONE" Then
      Dim hashAlg As HashAlgorithm = HashAlgorithm.Create(alg)

      Using strm As Stream = httpRsp.GetResponseStream

        Using outStrm As Stream = File.Open(outFileName, FileMode.Create, FileAccess.Write)

          Using cstrm As New CryptoStream(outStrm, hashAlg, CryptoStreamMode.Write)

            strm.CopyTo(cstrm, 60 * 1024)
            cstrm.Close()

            Dim pFix As New PremisFixity(alg, BytesToStr(hashAlg.Hash))

            pFix.MessageDigestOriginator = String.Format("{1} [{0}]", hashAlg.GetType.AssemblyQualifiedName,
                                                         UIUCLDAPUser.GetDomainFromQualifiedID(Principal.WindowsIdentity.GetCurrent.Name))
            pObjChar.Fixities.Add(pFix)
            pObjChar.Size = size

          End Using


          outStrm.Close()
        End Using

        strm.Close()
      End Using

    Else
      Using strm As Stream = httpRsp.GetResponseStream

        Using outStrm As Stream = File.Open(outFileName, FileMode.Create, FileAccess.Write)
          strm.CopyTo(outStrm, 60 * 1024)
          outStrm.Close()
        End Using

        strm.Close()

      End Using

    End If

    'copy the datetime settings to new file and set it to readonly
    Dim finfoNew As New FileInfo(outFileName)
    finfoNew.CreationTime = finfo.CreationTime
    finfoNew.LastAccessTime = finfo.LastAccessTime
    finfoNew.LastWriteTime = finfo.LastWriteTime
    'finfoNew.IsReadOnly = True

    If TypeOf httpRsp Is HttpWebResponse Then
      Dim evtInfoOK As New PremisEventOutcomeInformation([Enum].GetName(GetType(HttpStatusCode), CType(httpRsp, HttpWebResponse).StatusCode))
      pEvt.EventOutcomeInformation.Add(evtInfoOK)
    Else
      Dim evtInfoOK As New PremisEventOutcomeInformation(If(httpRsp.ContentLength > 0, "OK", "InternalServerError"))
      pEvt.EventOutcomeInformation.Add(evtInfoOK)
    End If

    Dim mime As String = MetadataFunctions.GetMimeFromFile(outFileName, http_mime)
    If pObjChar.Formats.Count = 0 Then
      'use the urlmon as a last resort mime type determination

      Dim pForm2 As New PremisFormat(mime)
      pForm2.FormatNotes.Add("Format Identified By URL Moniker Library.")
      pObjChar.Formats.Add(pForm2)

    ElseIf mime <> "application/octet-stream" Then
      'see if there might be discrepancies between urlmon and the existing fits-based format
      If pObjChar.Formats.Item(0).FormatName = "text/plain" And mime Like "*xml" Then
        'Fits will identify malformed XML as text/plain, so lets add a note about that if possible
        pObjChar.Formats.Item(0).FormatNotes.Add(String.Format("Alternate Format '{0}'; Identified By URL Moniker Library.  This is probably malformed XML.", mime))
      ElseIf pObjChar.Formats.Item(0).FormatName = "text/plain" And (mime <> "text/plain" And mime Like "text/*") Then
        'Fits may identify other text formats as text/plain, so give URLMon a crack at it.
        pObjChar.Formats.Item(0).FormatNotes.Add(String.Format("Alternate Format '{0}'; Identified By URL Moniker Library.  This may be malformed {1}.", mime, MetadataFunctions.MimeSubType(mime)))
      ElseIf Not pObjChar.Formats.Any(Function(f) f.FormatName = mime) Then
        'URLMon may just find a different formnat
        pObjChar.Formats.Item(0).FormatNotes.Add(String.Format("Alternate Format '{0}'; Identified By URL Moniker Library.", mime))
      End If

    End If

    Return Path.GetFileName(outFileName)

  End Function

  Private Function GetRelativePathTo(path As String) As String
    Dim ret As String = path.Substring(folderStack.Last.Length + pRepresentation.GetDefaultFileName("", "").Length + 1).Trim("\")
    Return ret
  End Function

End Class
