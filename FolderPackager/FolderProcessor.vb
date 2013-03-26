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
Imports Uiuc.Library.IdManagement
Imports Uiuc.Library.Fits
Imports Uiuc.Library.Medusa
Imports Uiuc.Library.MetadataUtilities
Imports System.ComponentModel
Imports System.Runtime.InteropServices

''' <summary>
''' Process a file system folder to create a Medusa Submission Package
''' </summary>
''' <remarks></remarks>
Public Class FolderProcessor

  Dim copying As Boolean = False

  'TOOD: Verify the SHA1 checksums

  Const MAX_RETRY_COUNT = 5
  Const IO_BUFFER_SIZE = &HF000

  Private pContainer As PremisContainer

  Private pUserAgent As PremisAgent
  Private pFitsAgent As PremisAgent
  Private pSoftAgent As PremisAgent

  Private pRepresentation As PremisObject 'the root representation object which is being packaged
  Private pPages As MedusaFullDsdBookPages 'only used for the Full DSD Book preservation level

  Private collHandle As String

  Private folderStack As Stack(Of String)
  Private objectFolderLevel As Integer
  Private baseDestFolder As String
  Private baseSrcFolder As String
  Private fileNum As Integer = 0

  'Private HandleMap As Dictionary(Of Uri, KeyValuePair(Of Integer, String))

  Private xslt As New Dictionary(Of String, XslCompiledTransform)

  Protected Sub New()
    'no public empty constructor is allowed
  End Sub

  Sub New(ByVal collectionHandle As String)
    collHandle = collectionHandle
    folderStack = New Stack(Of String)
    baseDestFolder = MedusaAppSettings.Settings.WorkingFolder
    baseSrcFolder = MedusaAppSettings.Settings.SourceFolder
    objectFolderLevel = MedusaAppSettings.Settings.ObjectFolderLevel
  End Sub



  ''' <summary>
  ''' Process the folder
  ''' </summary>
  ''' <param name="sourceFolder">the absolute path to the folder to process</param>
  ''' <remarks></remarks>
  Public Sub ProcessFolder(sourceFolder As String, Optional destFolder As String = "", Optional parentObject As PremisObject = Nothing)

    If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.OmitFoldersRegex) AndAlso Regex.IsMatch(sourceFolder, MedusaAppSettings.Settings.OmitFoldersRegex, RegexOptions.IgnoreCase) Then
      Trace.TraceWarning(String.Format("Skipping folder '{0}'.", sourceFolder))
      Exit Sub
    End If

    If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.RestartAtPath) AndAlso Regex.IsMatch(sourceFolder, MedusaAppSettings.Settings.RestartAtPath, RegexOptions.IgnoreCase) Then
      'a blank restart at path indicates that we can process folders as normal
      MedusaAppSettings.Settings.RestartAtPath = ""
    End If

    fileNum = fileNum + 1

    If String.IsNullOrWhiteSpace(destFolder) Then destFolder = Path.Combine(baseDestFolder, "data")

    Dim destPath As String = Path.Combine(destFolder, Path.GetFileName(sourceFolder))

    folderStack.Push(destPath)

    If String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.RestartAtPath) Then
      'only need to create directory if we have found our restart at path
      Directory.CreateDirectory(destPath)
    End If

    If folderStack.Count < objectFolderLevel Or Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.RestartAtPath) Then
      'this folder represents an intervening directory, files in this directory are ignored, sub folders are processed

      Console.Out.WriteLine(String.Format("Ignoring Folder: {0}", sourceFolder))

      If folderStack.Count >= objectFolderLevel Then
        'we are here because restart at path has not been found, and the restart at path must not be below the objectFolderLevel, so
        'no need to traverse into this folder
      Else
        Dim folders As List(Of String) = Directory.EnumerateDirectories(sourceFolder).ToList
        For Each fld In folders
          ProcessFolder(fld, destPath)
        Next
      End If

    ElseIf folderStack.Count = objectFolderLevel Then
      'this folder represent a top-level or root object
      pPages = Nothing

      Console.Out.WriteLine(String.Format("Root Folder: {0}", sourceFolder))

      Dim handle As String = IdManager.GetMedusaIdentifier(sourceFolder)

      If Not IdManager.ValidateHandle(handle) Then
        Trace.TraceError("Invalid Handle: " & handle)
        Trace.Flush()
        Throw New PackagerException("Invalid Handle: " & handle)
      End If

      Dim localId As String = handle
      Dim idType As String = IdManager.GetIdType(handle)
      If idType = "HANDLE" Then
        localId = IdManager.ParseLocalIdentifier(handle)
      End If

      pRepresentation = New PremisObject(idType, handle, PremisObjectCategory.Representation)
      pRepresentation.XmlId = String.Format("folder_{0}", fileNum)


      Dim skipFolder As Boolean = False
      If objectFolderLevel = 1 Then 'this is just the data folder, need to create another subfolder for this object
        destPath = Path.Combine(destPath, pRepresentation.GetDefaultFileName("", ""))
        Directory.CreateDirectory(destPath)
        folderStack.Push(destPath)
      Else
        'rename the folder based on the local identifier of the premis rep object
        Try
          My.Computer.FileSystem.RenameDirectory(destPath, pRepresentation.GetDefaultFileName("", ""))
        Catch ex As IOException
          If MedusaAppSettings.Settings.ObjectAlreadyExists = ObjectAlreadyExistsType.OVERWRITE Then
            'Dest folder already exists, so just delete the orig folder
            My.Computer.FileSystem.DeleteDirectory(destPath, FileIO.DeleteDirectoryOption.ThrowIfDirectoryNonEmpty)
            Trace.TraceWarning("Folder '{1}' already exists.  Contents will be overwritten.", destPath, pRepresentation.GetDefaultFileName("", ""))
          ElseIf MedusaAppSettings.Settings.ObjectAlreadyExists = ObjectAlreadyExistsType.SKIP Then
            'Dest folder already exists, so delete and skip this folder
            skipFolder = True
            My.Computer.FileSystem.DeleteDirectory(destPath, FileIO.DeleteDirectoryOption.ThrowIfDirectoryNonEmpty)
            Trace.TraceWarning("Folder '{1}' already exists.  Folder processing will be skipped.", destPath, pRepresentation.GetDefaultFileName("", ""))
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

      If skipFolder = False Then
        pContainer = New PremisContainer(pRepresentation)
        pContainer.IDPrefix = handle & MedusaAppSettings.Settings.HandleLocalIdSeparator
        Dim presLvl As New PremisPreservationLevel(MedusaAppSettings.Settings.PreservationLevel, Now)
        presLvl.PreservationLevelRationale.Add(MedusaAppSettings.Settings.PreservationLevelRationale)
        pRepresentation.PreservationLevels.Add(presLvl)
        pRepresentation.OriginalName = sourceFolder

        pUserAgent = MedusaHelpers.GetPremisAgent
        pUserAgent.AgentIdentifiers.Insert(0, IdManager.GetLocalPremisIdentifier(pUserAgent.AgentIdentifiers.First.IdentifierValue))
        If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.AgentsFolder) Then
          Directory.CreateDirectory(Path.Combine(MedusaAppSettings.Settings.AgentsFolder, pUserAgent.GetDefaultFileName("", "")))
          pUserAgent.SaveXML(Path.Combine(MedusaAppSettings.Settings.AgentsFolder, pUserAgent.GetDefaultFileName("", ""), pUserAgent.GetDefaultFileName("premis_agent_", "xml")), pContainer)
        End If

        pSoftAgent = PremisAgent.GetCurrentSoftwareAgent()
        pSoftAgent.AgentIdentifiers.Insert(0, IdManager.GetLocalPremisIdentifier(pSoftAgent.AgentIdentifiers.First.IdentifierValue))
        If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.AgentsFolder) Then
          Directory.CreateDirectory(Path.Combine(MedusaAppSettings.Settings.AgentsFolder, pSoftAgent.GetDefaultFileName("", "")))
          pSoftAgent.SaveXML(Path.Combine(MedusaAppSettings.Settings.AgentsFolder, pSoftAgent.GetDefaultFileName("", ""), pSoftAgent.GetDefaultFileName("premis_agent_", "xml")), pContainer)
        End If

        'register a handle for the root object 
        If MedusaAppSettings.Settings.HandleGeneration = HandleGenerationType.ROOT_OBJECT_AND_FILES Or MedusaAppSettings.Settings.HandleGeneration = HandleGenerationType.ROOT_OBJECT_ONLY Then
          Dim regHandle As String = RegisterFileHandle(destPath, pRepresentation)
        End If

        Dim files As List(Of String) = Directory.EnumerateFiles(sourceFolder).ToList
        For Each fl In files
          If String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.OmitFilesRegex) OrElse Not Regex.IsMatch(fl, MedusaAppSettings.Settings.OmitFilesRegex, RegexOptions.IgnoreCase) Then
            Dim fat = File.GetAttributes(fl)
            If Not (fat.HasFlag(FileAttributes.System) Or fat.HasFlag(FileAttributes.Hidden)) Then
              Dim relatType As String = MedusaHelpers.GetMedusaRelationshipType(fl, "BASIC_FILE_ASSET")
              Dim relatSubtype As String = MedusaHelpers.GetMedusaRelationshipSubtype(fl, relatType, "UNSPECIFIED")
              Dim capfile As String = CaptureFile(fl, destPath, pRepresentation, relatType, relatSubtype)
            End If
          Else
            Trace.TraceWarning(String.Format("Skipping file '{0}'.", fl))
          End If
        Next

        Dim folders As List(Of String) = Directory.EnumerateDirectories(sourceFolder).ToList
        For Each fld In folders
          ProcessFolder(fld, destPath, pRepresentation)
        Next

        'create the premis rights statement for dissemination rights
        Dim pRt As PremisRights = MedusaHelpers.GetPremisDisseminationRights("LOCAL", pContainer.NextID, pRepresentation)
        If pRt IsNot Nothing Then
          pContainer.Rights.Add(pRt)
        End If

        idType = IdManager.GetIdType(collHandle)

        pRepresentation.RelateToObject("COLLECTION", "IS_MEMBER_OF", idType, collHandle)

        pContainer.Agents.Add(pUserAgent)

        pContainer.Agents.Add(pSoftAgent)

        If pFitsAgent IsNot Nothing Then
          pFitsAgent.AgentIdentifiers.Insert(0, IdManager.GetLocalPremisIdentifier(pFitsAgent.AgentIdentifiers.First.IdentifierValue))
          If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.AgentsFolder) Then
            Directory.CreateDirectory(Path.Combine(MedusaAppSettings.Settings.AgentsFolder, pFitsAgent.GetDefaultFileName("", "")))
            pFitsAgent.SaveXML(Path.Combine(MedusaAppSettings.Settings.AgentsFolder, pFitsAgent.GetDefaultFileName("", ""), pFitsAgent.GetDefaultFileName("premis_agent_", "xml")), pContainer)
          End If
          pContainer.Agents.Add(pFitsAgent)
        End If

        Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CREATION")
        'include details about packager program here
        pEvt.EventDetail = String.Format("SIP created from a file system folder: '{7}'.  Program: {0} {1} [{2}], Computer: {3}, {4} V{5}, {6}",
                                         My.Application.Info.Title, My.Application.Info.Version, My.Application.Info.CompanyName, My.Computer.Name, My.Computer.Info.OSFullName.Trim,
                                         My.Computer.Info.OSVersion, My.Application.UICulture.EnglishName, sourceFolder)
        pEvt.LinkToAgent(pUserAgent)
        pEvt.LinkToAgent(pSoftAgent)
        pEvt.LinkToObject(pRepresentation)
        pContainer.Events.Add(pEvt)

        If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.AgentsFolder) Then
          pContainer.PersistedEntityTypes = PremisEntityTypes.AllExceptAgents
        End If

        MedusaHelpers.SavePremisContainer(pContainer, destPath)
      End If

    ElseIf folderStack.Count > objectFolderLevel Then

      'this folder represents the components of an object, all its files and subfolders are processed

      Console.Out.WriteLine(String.Format("Subfolder: {0}", sourceFolder))

      If MedusaAppSettings.Settings.PreservationLevel = "FULL_DSD_BOOK" And Regex.IsMatch(sourceFolder, MedusaAppSettings.Settings.PageFoldersRegex) Then
        ProcessFullDsdBook(destPath, sourceFolder)

      Else 'default is FILE_SYSTEM_FOLDER; bit-level preservation of folder contents
        ProcessFileSystemFolder(destPath, sourceFolder, parentObject)
      End If

    End If

    folderStack.Pop()
  End Sub

  Private Sub ProcessFullDsdBook(destPath As String, sourcefolder As String)
    If pPages Is Nothing Then
      pPages = New MedusaFullDsdBookPages(pContainer, pRepresentation)
    End If

    'process all the files in the sourceFolder
    Dim files As List(Of String) = Directory.EnumerateFiles(sourcefolder).ToList
    For Each fl In files
      If String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.OmitFilesRegex) OrElse Not Regex.IsMatch(fl, MedusaAppSettings.Settings.OmitFilesRegex, RegexOptions.IgnoreCase) Then
        Dim fat = File.GetAttributes(fl)
        If Not (fat.HasFlag(FileAttributes.System) Or fat.HasFlag(FileAttributes.Hidden)) Then
          Dim capfile As String = CaptureFile(fl, destPath, Nothing, "", "")
          Dim obj = pContainer.FindSingleObject("FILENAME", GetRelativePathTo(capfile))
          pPages.AddPage(obj)
        End If
      Else
        Trace.TraceWarning(String.Format("Skipping file '{0}'.", fl))
      End If
    Next

    'check that there are no subfolders, since they are not allowed in the Full DSD Book Model


  End Sub

  Private Sub ProcessFileSystemFolder(destPath As String, sourcefolder As String, parentObject As PremisObject)
    'create representation object for folder
    Dim pFolderObj As New PremisObject("LOCAL", pContainer.NextID, PremisObjectCategory.Representation)
    pFolderObj.XmlId = String.Format("folder_{0}", fileNum)

    Dim skipFolder As Boolean = False
    'Rename the folder at this point to use the local suffix index value
    Try
      My.Computer.FileSystem.RenameDirectory(destPath, pFolderObj.GetDefaultFileNameIndex)
    Catch ex As IOException
      If MedusaAppSettings.Settings.ObjectAlreadyExists = ObjectAlreadyExistsType.OVERWRITE Then
        'Dest folder already exists, so just delete the orig folder
        My.Computer.FileSystem.DeleteDirectory(destPath, FileIO.DeleteDirectoryOption.ThrowIfDirectoryNonEmpty)
        Trace.TraceWarning("Folder '{1}' already exists.  Contents will be overwritten.", destPath, pFolderObj.GetDefaultFileName("", ""))
      ElseIf MedusaAppSettings.Settings.ObjectAlreadyExists = ObjectAlreadyExistsType.SKIP Then
        'Dest folder already exists, so just skip the orig folder
        skipFolder = True
        Trace.TraceWarning("Folder '{1}' already exists.  Folder processing will be skipped.", destPath, pFolderObj.GetDefaultFileName("", ""))
      Else
        Trace.TraceError("Folder '{1}' already exists.", destPath, pFolderObj.GetDefaultFileName("", ""))
        Trace.Flush()
        Throw New PackagerException(String.Format("Folder '{1}' already exists.", destPath, pFolderObj.GetDefaultFileName("", "")), ex)
      End If
    End Try
    destPath = Path.Combine(Path.GetDirectoryName(destPath), pFolderObj.GetDefaultFileNameIndex)
    folderStack.Pop()
    folderStack.Push(destPath)

    If skipFolder = False Then
      'add a folder name identifier which is the path relative to the root folder for this object
      Dim pId As New PremisIdentifier("FOLDERNAME", GetRelativePathTo(destPath))
      pFolderObj.ObjectIdentifiers.Add(pId)
      pFolderObj.OriginalName = sourcefolder


      pFolderObj.RelateToObject("BASIC_COMPOUND_ASSET", "PARENT", parentObject)
      parentObject.RelateToObject("BASIC_COMPOUND_ASSET", "CHILD", pFolderObj)

      pContainer.Objects.Add(pFolderObj)

      Dim files As List(Of String) = Directory.EnumerateFiles(sourcefolder).ToList
      For Each fl In files
        If String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.OmitFilesRegex) OrElse Not Regex.IsMatch(fl, MedusaAppSettings.Settings.OmitFilesRegex, RegexOptions.IgnoreCase) Then
          Dim fat = File.GetAttributes(fl)
          If Not (fat.HasFlag(FileAttributes.System) Or fat.HasFlag(FileAttributes.Hidden)) Then
            Dim relatType As String = MedusaHelpers.GetMedusaRelationshipType(fl, "BASIC_FILE_ASSET")
            Dim relatSubtype As String = MedusaHelpers.GetMedusaRelationshipSubtype(fl, relatType, "UNSPECIFIED")
            Dim capfile As String = CaptureFile(fl, destPath, pFolderObj, relatType, relatSubtype)
          End If
        Else
          Trace.TraceWarning(String.Format("Skipping file '{0}'.", fl))
        End If
      Next

      Dim folders As List(Of String) = Directory.EnumerateDirectories(sourcefolder).ToList
      For Each fld In folders
        ProcessFolder(fld, destPath, pFolderObj)
      Next

    End If

  End Sub

  ''' <summary>
  ''' Capture the file, copy it to the destination folder
  ''' </summary>
  ''' <param name="filename">the path to the file to capture</param>
  ''' <param name="recpath">the path to store the new file</param>
  ''' <param name="parent">the parent PREMIS object; this can be set to nothing if relationships are created external to this function</param>
  ''' <param name="relat">the relationship between the parent and this object</param>
  ''' <param name="subRelat">the sub-relationship between the parent and this object</param>
  ''' <remarks></remarks>
  ''' <returns>the path to the new file</returns>
  Private Function CaptureFile(filename As String, recpath As String, parent As PremisObject, relat As String, subRelat As String) As String
    fileNum = fileNum + 1
    Dim ret As String = ""

    Console.Out.WriteLine(String.Format("File: {0}", filename))

    Dim i As Integer = 0

    Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CAPTURE")
    pEvt.LinkToAgent(pUserAgent)
    pEvt.LinkToAgent(pSoftAgent)
    pEvt.EventDetail = String.Format("Fetching File: {0}", filename)

    pContainer.Events.Add(pEvt)

    Dim pObjChars As New PremisObjectCharacteristics()
    Dim pSigProps As New List(Of PremisSignificantProperties)

    Dim pEvtFits As PremisEvent = Nothing
    If MedusaAppSettings.Settings.DoFits = True Then
      Dim fts As New FitsResult(filename)
      pObjChars.ObjectCharacteristicsExtensions.Add(fts.FitsXml)
      pObjChars.Formats.AddRange(fts.PremisFormats)
      pObjChars.Fixities.AddRange(fts.PremisFixities)
      pObjChars.CreatingApplications.AddRange(fts.PremisCreatingApplications)

      pEvtFits = fts.GetPremisEvent(New PremisIdentifier("LOCAL", pContainer.NextID))
      pContainer.Events.Add(pEvtFits)

      pSigProps = fts.GetPremisSignificantProperties

      If pFitsAgent Is Nothing Then
        pFitsAgent = fts.GetPremisAgent()
      End If

      pEvtFits.LinkToAgent(pFitsAgent)
      pEvtFits.LinkToAgent(pUserAgent)
    End If

    Dim capFile As String
    If Path.GetPathRoot(filename).ToLower = Path.GetPathRoot(recpath).ToLower And (MedusaAppSettings.Settings.PackageMode = PackageModeType.MOVE Or MedusaAppSettings.Settings.PackageMode = PackageModeType.HARDLINK) Then
      capFile = MoveFile(filename, recpath, pEvt, pObjChars)
    Else
      capFile = FetchFile(filename, recpath, pEvt, pObjChars)
    End If

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
    ret = capFile

    If Not String.IsNullOrWhiteSpace(capFile) Then

      Dim pObj As New PremisObject("FILENAME", capFile, PremisObjectCategory.File, pObjChars)
      pObj.SignificantProperties.AddRange(pSigProps)

      If relat = "METADATA" Then
        pObj.PreservationLevels.Add(New PremisPreservationLevel("ORIGINAL_METADATA_FILE", Now))
      ElseIf Regex.IsMatch(filename, MedusaAppSettings.Settings.OriginalContentFileRegex, RegexOptions.IgnoreCase) Then
        pObj.PreservationLevels.Add(New PremisPreservationLevel("ORIGINAL_CONTENT_FILE", Now))
      ElseIf Regex.IsMatch(filename, MedusaAppSettings.Settings.DerivativeContentFileRegex, RegexOptions.IgnoreCase) Then
        pObj.PreservationLevels.Add(New PremisPreservationLevel("DERIVATIVE_CONTENT_FILE", Now))
      Else
        pObj.PreservationLevels.Add(New PremisPreservationLevel("ORIGINAL_CONTENT_FILE", Now))
      End If
      pObj.OriginalName = filename
      pObj.ObjectIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
      pObj.XmlId = String.Format("file_{0}", fileNum)


      'TODO: This rename code is a mess!  The OVERWRITE and SKIP checks should be moved up so as to avoid recapturing a file that already exists
      'or this code should just be eliminated so as to always overwrite files.  In actuality if the setting is SKIP, this will never be reached
      'because the folder will have already been skipped.

      'rename the file to use the uuid and the relative path
      Dim newFName As String = pObj.GetDefaultFileName(MedusaHelpers.GetFileNamePrefix(relat, subRelat), Path.GetExtension(filename))
      Dim newFPath As String = Path.Combine(recpath, newFName)
      Dim capPath As String = Path.Combine(recpath, capFile)
      'If file already exists -- delete it and log a warning
      If File.Exists(newFPath) Then
        If MedusaAppSettings.Settings.ObjectAlreadyExists = ObjectAlreadyExistsType.OVERWRITE Then
          File.Delete(newFPath)
          Trace.TraceWarning("File '{0}' already exists.  It will be deleted and replaced with the new file of same name.", newFPath)
        ElseIf MedusaAppSettings.Settings.ObjectAlreadyExists = ObjectAlreadyExistsType.SKIP Then
          Dim newFInfo As New FileInfo(newFPath)
          Dim capFInfo As New FileInfo(capPath)

          If newFInfo.Length = capFInfo.Length And newFInfo.LastWriteTime = capFInfo.LastWriteTime Then
            'if the captured file and the existing file are the same size and date just skip this file and delete the captured file
            File.Delete(capPath)
            Trace.TraceWarning("File '{0}' already exists.  It will be skipped.", newFPath)
          Else
            'overwrite the existing file because it is different than the captured file
            File.Delete(newFPath)
            Trace.TraceWarning("File '{0}' already exists but is different than the captured file.  It will be deleted and replaced with captured file.", newFPath)
          End If
        Else
          Trace.TraceError("File '{0}' already exists.", newFPath)
          Trace.Flush()
          Throw New PackagerException(String.Format("File '{0}' already exists.", newFPath))
        End If
      End If

      My.Computer.FileSystem.MoveFile(capPath, newFPath)
      pObj.GetFilenameIdentifier.IdentifierValue = GetRelativePathTo(newFPath)

      pObj.LinkToEvent(pEvt)
      If pEvtFits IsNot Nothing Then pObj.LinkToEvent(pEvtFits)

      pContainer.Objects.Add(pObj)

      If parent IsNot Nothing Then
        parent.RelateToObject(relat, subRelat, pObj)
        pObj.RelateToObject(relat, "PARENT", parent)
      End If

      'register a handle for the file
      If MedusaAppSettings.Settings.HandleGeneration = HandleGenerationType.ROOT_OBJECT_AND_FILES Or MedusaAppSettings.Settings.HandleGeneration = HandleGenerationType.FILES_ONLY Then
        Dim handle As String = RegisterFileHandle(newFPath, pObj)
      End If

      ret = newFPath

      If (relat = "METADATA" And subRelat = "MARC" And Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.MarcToModsXslt)) Or _
        (relat = "METADATA" And subRelat = "DC_RDF" And Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.DcRdfToModsXslt)) Then

        TransformMetadata(subRelat, ret, pObj, parent)

      End If

    End If

    i = i + 1

    Return ret

  End Function

  Private Sub TransformMetadata(subRelat As String, filePath As String, pObj As PremisObject, parent As PremisObject)
    Dim xsltFile As String = ""
    If Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.MarcToModsXslt) Then
      xsltFile = MedusaAppSettings.Settings.MarcToModsXslt
    ElseIf Not String.IsNullOrWhiteSpace(MedusaAppSettings.Settings.DcRdfToModsXslt) Then
      xsltFile = MedusaAppSettings.Settings.DcRdfToModsXslt
    End If

    If xslt.ContainsKey(subRelat) = False Then
      xslt.Add(subRelat, New XslCompiledTransform())
      xslt.Item(subRelat).Load(xsltFile)
    End If


    Dim modsObj = New PremisObject("LOCAL", pContainer.NextID, PremisObjectCategory.File)
    Dim modsFile As String = Path.Combine(Path.GetDirectoryName(filePath), modsObj.GetDefaultFileName("mods_", "xml"))

    Try
      xslt.Item(subRelat).Transform(filePath, modsFile)

    Catch ex As Exception
      modsObj = Nothing
      File.Delete(modsFile)

      Trace.TraceError("Transforming the {0} file into {1} using the {2} XSLT failed: {3}", Path.GetFileName(filePath), Path.GetFileName(modsFile), xsltFile, ex.Message)
      MedusaHelpers.EmailErrorMessage(String.Format("Transforming the {0} file into {1} using the {2} XSLT failed: {3}", Path.GetFileName(filePath), Path.GetFileName(modsFile), xsltFile, ex.Message))

      'Add migration event for the failed attempt
      Dim modsEvtFail As New PremisEvent("LOCAL", pContainer.NextID, "MIGRATION")
      modsEvtFail.EventDetail = String.Format("Transforming the {0} file into {1} using the {2} XSLT.", Path.GetFileName(filePath), Path.GetFileName(modsFile), xsltFile)
      Dim modsEvtOut = New PremisEventOutcomeInformation("FAILED")
      modsEvtOut.EventOutcomeDetails.Add(New PremisEventOutcomeDetail(ex.Message))
      modsEvtFail.EventOutcomeInformation.Add(modsEvtOut)
      modsEvtFail.LinkToAgent(pSoftAgent)
      modsEvtFail.LinkToAgent(pUserAgent)
      modsEvtFail.LinkToObject(pObj)

      pContainer.Events.Add(modsEvtFail)
    End Try

    If modsObj IsNot Nothing Then

      modsObj.ObjectIdentifiers.Add(New PremisIdentifier("FILENAME", modsFile))

      Dim modsChar = MedusaHelpers.CharacterizeFile(modsFile, "text/xml")
      modsObj.ObjectCharacteristics.Add(modsChar)
      modsObj.PreservationLevels.Add(New PremisPreservationLevel("DERIVATIVE_METADATA_FILE"))

      If parent IsNot Nothing Then
        parent.RelateToObject("METADATA", "MODS", modsObj)
      End If

      'Add migration event and derivation relationship
      Dim modsEvt As New PremisEvent("LOCAL", pContainer.NextID, "MIGRATION")
      modsEvt.EventDetail = String.Format("Transforming the {0} file into {1} using the {2} XSLT.", Path.GetFileName(filePath), Path.GetFileName(modsFile), xsltFile)
      modsEvt.EventOutcomeInformation.Add(New PremisEventOutcomeInformation("OK"))
      modsEvt.LinkToAgent(pSoftAgent)
      modsEvt.LinkToAgent(pUserAgent)

      modsObj.RelateToObject("DERIVATION", "HAS_SOURCE", pObj, modsEvt)

      pContainer.Objects.Add(modsObj)
      pContainer.Events.Add(modsEvt)

    End If

  End Sub

  ''' <summary>
  ''' Register a handle for the file or folder object
  ''' </summary>
  ''' <param name="newFPath"></param>
  ''' <param name="pObj"></param>
  ''' <returns></returns>
  ''' <remarks>This function requires the pContainer and pUserAgent be instantiated</remarks>
  Private Function RegisterFileHandle(newFPath As String, pObj As PremisObject) As String
    If pContainer Is Nothing Or pUserAgent Is Nothing Or pSoftAgent Is Nothing Then
      Throw New PackagerException("Object is not in state where a handle can be registered.")
    End If

    Dim handle = IdManager.RegisterFileHandle(newFPath, pObj, pContainer, pUserAgent, pSoftAgent)

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
  'I did try some asynchronous double-buffered copy routines but they were not faster

  'TODO:  The FetchFile and MoveFile functions are all wet

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
          Trace.Flush()
          MedusaHelpers.EmailException(ex)

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
        Trace.Flush()
        MedusaHelpers.EmailException(ex)

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

    Dim alg As String = MedusaAppSettings.Settings.ChecksumAlgorithm

    If alg <> "NONE" Then
      Dim hashAlg As HashAlgorithm = HashAlgorithm.Create(alg)

      Using strm As Stream = httpRsp.GetResponseStream

        Using outStrm As Stream = File.Open(outFileName, FileMode.Create, FileAccess.Write)

          Using cstrm As New CryptoStream(outStrm, hashAlg, CryptoStreamMode.Write)

            strm.CopyTo(cstrm, IO_BUFFER_SIZE)
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

    Else 'don't generate hash, just copy files
      Using strm As Stream = httpRsp.GetResponseStream

        Using outStrm As Stream = File.Open(outFileName, FileMode.Create, FileAccess.Write)

          strm.CopyTo(outStrm, IO_BUFFER_SIZE)

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

  Private Function MoveFile(ByVal filename As String, destPath As String, ByVal pEvt As PremisEvent, ByVal pObjChar As PremisObjectCharacteristics, Optional ByVal saveFile As String = "") As String

    Dim finfo As New FileInfo(filename)

    Dim size As Long = finfo.Length

    Dim outFileName As String
    If String.IsNullOrWhiteSpace(saveFile) Then
      Dim fn As String = Path.GetFileName(filename)
      'replace invalid chars in name
      fn = Regex.Replace(fn, String.Format("[{0}]", New String(Path.GetInvalidFileNameChars)), "_") 'TODO: This could result in filename collisions
      outFileName = Path.Combine(destPath, fn)
    Else
      outFileName = saveFile
    End If

    Dim alg As String = MedusaAppSettings.Settings.ChecksumAlgorithm

    If alg <> "NONE" Then
      Dim hashAlg As HashAlgorithm = HashAlgorithm.Create(alg)

      Using strm As Stream = New FileStream(filename, FileMode.Open, FileAccess.Read)

        Dim byt(IO_BUFFER_SIZE) As Byte
        Dim len As Integer = strm.Read(byt, 0, byt.Length)
        Dim len2 As Integer = 0
        Dim size2 As Long
        While len > 0
          size2 = size2 + len
          len2 = hashAlg.TransformBlock(byt, 0, len, byt, 0)
          len = strm.Read(byt, 0, byt.Length)
        End While
        hashAlg.TransformFinalBlock(byt, 0, 0)

        Dim pFix As New PremisFixity(alg, BytesToStr(hashAlg.Hash))

        pFix.MessageDigestOriginator = String.Format("{1} [{0}]", hashAlg.GetType.AssemblyQualifiedName,
                                                     UIUCLDAPUser.GetDomainFromQualifiedID(Principal.WindowsIdentity.GetCurrent.Name))
        pObjChar.Fixities.Add(pFix)
        pObjChar.Size = size

        If size2 <> size2 Then
          Throw New PackagerException("Size mismatch between file system and actual byte count.")
        End If

        strm.Close()
      End Using
    End If

    'move files

    If MedusaAppSettings.Settings.PackageMode = PackageModeType.MOVE Then
      File.Move(filename, outFileName)
    ElseIf MedusaAppSettings.Settings.PackageMode = PackageModeType.HARDLINK Then
      Dim ret As Boolean = MedusaHelpers.CreateHardLink(outFileName, filename, IntPtr.Zero)
      If ret = False Then
        Throw New PackagerException(String.Format("CreateHardLink failed: {0}", Err.LastDllError))
      End If
    End If

    'copy the datetime settings to new file and maybe set it to readonly
    Dim finfoNew As New FileInfo(outFileName)
    finfoNew.IsReadOnly = False 'some files are originally set to readonly, must change this so they can be manipulated
    finfoNew.CreationTime = finfo.CreationTime
    finfoNew.LastAccessTime = finfo.LastAccessTime
    finfoNew.LastWriteTime = finfo.LastWriteTime
    'TODO: Maybe for security make it read-only after the move
    'finfoNew.IsReadOnly = True

    Dim evtInfoOK As New PremisEventOutcomeInformation("OK")
    pEvt.EventOutcomeInformation.Add(evtInfoOK)

    Dim mime As String = MetadataFunctions.GetMimeFromFile(outFileName, "application/octet-stream")
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
    Dim pathRoot As String = "\" & pRepresentation.GetDefaultFileName("", "") & "\"
    Dim i As Integer = path.IndexOf(pathRoot) + pathRoot.Length

    Dim ret As String = path.Substring(i).Trim("\")
    Return ret
  End Function



End Class
