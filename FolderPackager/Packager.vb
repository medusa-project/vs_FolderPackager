Imports System.Xml
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Configuration
Imports System.Text
Imports System.Security
Imports Uiuc.Library.Premis
Imports Uiuc.Library.MetadataUtilities
Imports Uiuc.Library.Ldap
Imports Uiuc.Library.CollectionsRegistry
Imports System.Net
Imports System.Net.Mail

'TODO:  Add an email function for fatal error conditions

Module Packager
  Const DEBUG_MAX_COUNT = 1000000 'Integer.MaxValue
  Private IdMap As New Dictionary(Of String, String)
  Private HandleMap As New Dictionary(Of Uri, KeyValuePair(Of Integer, String))

  ''' <summary>
  ''' "Remember the Maine!"
  ''' </summary>
  ''' <remarks></remarks>
  Sub Main()
    Dim started As DateTime = Now

    'need to deal with SSL
    ServicePointManager.ServerCertificateValidationCallback = AddressOf ValidateCert
    ServicePointManager.DefaultConnectionLimit = 10


    'set current directory to working folder; this is the destination folder for the package
    Directory.CreateDirectory(ConfigurationManager.AppSettings.Item("WorkingFolder"))
    Directory.SetCurrentDirectory(ConfigurationManager.AppSettings.Item("WorkingFolder"))

    'init tracing/logging
    Trace.Listeners.Clear()
    Trace.Listeners.Add(New TextWriterTraceListener(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), "FolderPackager.log")))

    Trace.TraceInformation("Started: {0} By: {1} ", started.ToString("s"), Principal.WindowsIdentity.GetCurrent.Name)
    Trace.TraceInformation("Working Folder: {0}", Directory.GetCurrentDirectory)

    Dim collHandle As String = ProcessCollection()

    'Try
    StartProcess(collHandle)
    'Catch ex As Exception
    '  Console.Error.WriteLine(String.Format("Error: {0}", ex.Message))
    '  Trace.TraceError("Error: {0}", ex.Message)
    '  'email fatal errors to operator
    '  Try
    '    Dim usr = UIUCLDAPUser.Create(Principal.WindowsIdentity.GetCurrent.Name)
    '    Dim msg As New System.Net.Mail.MailMessage("medusa-admin@library.illinois.edu", usr.EMail)
    '    msg.Subject = String.Format("Medusa Error Report: {0}", My.Application.Info.Title)
    '    msg.Body = String.Format("{0}{2}{2}{1}", ex.Message, ex.StackTrace, vbCrLf)

    '    Dim smtp As New SmtpClient()
    '    smtp.Send(msg)
    '  Catch ex2 As Exception
    '    Console.Error.WriteLine(String.Format("Email Error: {0}", ex2.Message))
    '    Trace.TraceError("Error: {0}", ex2.Message)
    '  End Try

    'End Try

    Dim finished As DateTime = Now
    Trace.TraceInformation("Finished: {0}  Elapsed Time: {1} minutes", finished.ToString("s"), DateDiff(DateInterval.Minute, started, finished))
    Trace.Flush()

    'Console.Out.WriteLine("Done.  Press Enter to finish.")
    'Console.In.ReadLine()

  End Sub

  ''' <summary>
  ''' Process the source folder with all files belonging to the given collection
  ''' </summary>
  ''' <param name="collHandle"></param>
  ''' <remarks></remarks>
  Private Sub StartProcess(ByVal collHandle As String)

    Dim folderName As String = ConfigurationManager.AppSettings.Item("SourceFolder")

    If File.Exists(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), ConfigurationManager.AppSettings.Item("IdMapFile"))) Then
      LoadIdMap(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), ConfigurationManager.AppSettings.Item("IdMapFile")))
    End If

    If File.Exists(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), ConfigurationManager.AppSettings.Item("HandleMapFile"))) Then
      LoadHandleMap(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), ConfigurationManager.AppSettings.Item("HandleMapFile")))
    End If

    Dim processor As New FolderProcessor(collHandle, IdMap, HandleMap)
    processor.ProcessFolder(folderName)

    SaveIdMap(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), ConfigurationManager.AppSettings.Item("IdMapFile")))
    SaveHandleMap(Path.Combine(ConfigurationManager.AppSettings.Item("WorkingFolder"), ConfigurationManager.AppSettings.Item("HandleMapFile")))

  End Sub

  Private Sub SaveIdMap(fileName As String)
    'If Not File.Exists(fileName) Then
    Dim fs As New StreamWriter(fileName)
    For Each k In IdMap
      fs.WriteLine(String.Format("{0},{1}", k.Key, k.Value))
    Next
    fs.Close()
    'End If
  End Sub

  Private Sub SaveHandleMap(fileName As String)
    'If Not File.Exists(fileName) Then
    Dim fs As New StreamWriter(fileName)
    For Each k In HandleMap
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
        IdMap.Add(parts(0), parts(1))
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
        HandleMap.Add(New Uri(parts(1)), New KeyValuePair(Of Integer, String)(parts(0), parts(2)))
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
  Private Function ProcessCollection() As String
    'TODO:  Use the collection registry ID to capture the collection mods record

    Directory.CreateDirectory("collection")

    Dim collId As String = ConfigurationManager.AppSettings.Item("CollectionId")

    Dim collrec As CollectionRecord = Nothing
    If Not String.IsNullOrWhiteSpace(collId) Then
      collrec = New CollectionRecord(collId, "collection")
    End If

    Dim collHandle As String = ""
    If collrec IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(collrec.Uuid) Then
      collHandle = String.Format("{0}:{1}", ConfigurationManager.AppSettings.Item("Handle.Project"), collrec.Uuid)
    Else
      collHandle = ConfigurationManager.AppSettings.Item("CollectionHandle")
    End If

    If collrec Is Nothing Then
      'need to create collection record
      Dim pth As String = ConfigurationManager.AppSettings.Item("CollectionDescriptionPath")
      Dim collDescr As String = File.ReadAllText(pth)



      'create mods record for the collection
      Dim xmlwr As XmlWriter = XmlWriter.Create(Path.Combine("collection", "mods.xml"), New XmlWriterSettings With {.Indent = True, .Encoding = Encoding.UTF8, .OmitXmlDeclaration = True})
      xmlwr.WriteStartElement("mods", "http://www.loc.gov/mods/v3")
      xmlwr.WriteAttributeString("version", "3.4")
      xmlwr.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", "http://www.loc.gov/mods/v3 http://www.loc.gov/standards/mods/mods.xsd")
      xmlwr.WriteComment("This collection mods record should be edited by hand to most accurately reflect the collection")
      xmlwr.WriteStartElement("typeOfResource")
      xmlwr.WriteAttributeString("collection", "yes")
      xmlwr.WriteString("mixed material")
      xmlwr.WriteEndElement()
      xmlwr.WriteStartElement("titleInfo")
      xmlwr.WriteElementString("title", ConfigurationManager.AppSettings.Item("ContentDMCollectionName"))
      xmlwr.WriteEndElement()
      xmlwr.WriteElementString("abstract", collDescr)
      xmlwr.WriteStartElement("location")
      xmlwr.WriteStartElement("url")
      xmlwr.WriteAttributeString("access", "object in context")
      xmlwr.WriteAttributeString("usage", "primary")
      xmlwr.WriteString(ConfigurationManager.AppSettings.Item("ContentDMCollectionURL"))
      xmlwr.WriteEndElement()
      xmlwr.WriteEndElement()
      xmlwr.Close()
    End If


    'Create a PREMIS metadata for the collection
    If String.IsNullOrWhiteSpace(collHandle) Then
      collHandle = MetadataFunctions.GenerateLocalIdentifier
    End If
    Dim idType As String = "LOCAL"
    If collHandle.StartsWith(ConfigurationManager.AppSettings.Item("Handle.Prefix") & "/") Then
      idType = "HANDLE"
    End If

    If MetadataFunctions.ValidateHandle(collHandle) = False Then
      Trace.TraceError("Invalid Handle: " & collHandle)
      Trace.Flush()
      Throw New PackagerException("Invalid Handle: " & collHandle)
    End If

    Dim pObj As New PremisObject(idType, collHandle, PremisObjectCategory.Representation)
    pObj.ObjectIdentifiers.Add(New PremisIdentifier("URL", ConfigurationManager.AppSettings.Item("CollectionURL")))
    If collrec IsNot Nothing Then
      pObj.ObjectIdentifiers.Add(New PremisIdentifier("URL", collrec.Url))
    End If
    Dim pContainer As PremisContainer = New PremisContainer(pObj)
    pContainer.IDPrefix = collHandle & ConfigurationManager.AppSettings.Item("Handle.LocalIdSeparator")

    Dim currentAgent As PremisAgent = UIUCLDAPUser.GetPremisAgent
    currentAgent.AgentIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
    pContainer.Agents.Add(currentAgent)

    Dim pEvt As New PremisEvent("LOCAL", pContainer.NextID, "CREATION")
    pEvt.LinkToAgent(currentAgent)
    pEvt.LinkToObject(pObj)
    pContainer.Events.Add(pEvt)

    Dim pObj2 As New PremisObject("FILENAME", "mods.xml", PremisObjectCategory.File, "text/xml")
    pObj2.ObjectIdentifiers.Insert(0, pContainer.NextLocalIdentifier)
    If collrec IsNot Nothing Then
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
    pEvt2.LinkToAgent(currentAgent)
    pEvt2.LinkToObject(pObj2)
    pContainer.Events.Add(pEvt2)


    pObj.RelateToObject("METADATA", "HAS_ROOT", pObj2)

    If Not String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("PremisDisseminationRights")) Then
      Dim pRgtStmt As New PremisRightsStatement("LOCAL", pContainer.NextID, ConfigurationManager.AppSettings.Item("PremisDisseminationRightsBasis"))
      pRgtStmt.RightsGranted.Add(New PremisRightsGranted(ConfigurationManager.AppSettings.Item("PremisDisseminationRights")))
      pRgtStmt.LinkToObject(pObj)
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

    pContainer.SaveXML(Path.Combine("collection", pObj.GetDefaultFileName("premis_object_", "xml")))

    'pContainer.SaveEachXML(Path.Combine("collection", "_").TrimEnd("_"))


    Return collHandle
  End Function



  Function ValidateCert(ByVal sender As Object, _
    ByVal cert As System.Security.Cryptography.X509Certificates.X509Certificate, _
    ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, _
    ByVal sslPolicyErrors As Security.SslPolicyErrors) As Boolean
    If ConfigurationManager.AppSettings.Item("IgnoreBadCert").ToUpper = "TRUE" Then
      Return True
    Else
      Return sslPolicyErrors = Security.SslPolicyErrors.None
    End If
  End Function


End Module
