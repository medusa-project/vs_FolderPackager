Imports System.Xml
Imports System.Configuration
Imports System.IO
Imports Uiuc.Library.Premis
Imports Uiuc.Library.Ldap
Imports Uiuc.Library.Medusa
Imports System.Security

''' <summary>
''' Represent the results of running a file through the FITS file characterization program
''' </summary>
''' <remarks></remarks>
Public Class FitsResult
  Private _xml As XmlDocument
  Private _xmlns As XmlNamespaceManager
  Private _err As String

  Public ReadOnly Property FitsXml As XmlDocument
    Get
      Return _xml
    End Get
  End Property

  Public ReadOnly Property ErrorOut As String
    Get
      Return _err
    End Get
  End Property

  Public ReadOnly Property Size As Long
    Get
      Dim nd As XmlElement = _xml.SelectSingleNode("//fits:fileinfo/fits:size", _xmlns)
      Return Long.Parse(nd.InnerText)
    End Get
  End Property

  Public ReadOnly Property Md5 As String
    Get
      Dim nd As XmlElement = _xml.SelectSingleNode("//fits:fileinfo/fits:md5checksum", _xmlns)
      Return nd.InnerText
    End Get
  End Property

  Public ReadOnly Property Created As DateTime?
    Get
      Dim nd As XmlElement = _xml.SelectSingleNode("//fits:fileinfo/fits:created", _xmlns)
      If nd IsNot Nothing Then
        Return FixDateTime(nd.InnerText)
      Else
        Return Nothing
      End If
    End Get
  End Property

  Public ReadOnly Property LastModified As DateTime
    Get
      Dim nd As XmlElement = _xml.SelectSingleNode("//fits:fileinfo/fits:lastmodified", _xmlns)
      Return FixDateTime(nd.InnerText)
    End Get
  End Property

  Public ReadOnly Property FitsVersion As String
    Get
      Dim nd As XmlAttribute = _xml.SelectSingleNode("/fits:fits/@version", _xmlns)
      Return nd.Value
    End Get
  End Property

  Public ReadOnly Property FilePath As String
    Get
      Dim nd As XmlElement = _xml.SelectSingleNode("//fits:fileinfo/fits:filepath", _xmlns)
      Return nd.InnerText
    End Get
  End Property


  ''' <summary>
  ''' Fix datetime strings; for some strange reason datetimes are formatted with only colons as separators
  ''' </summary>
  ''' <param name="dts"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Private Function FixDateTime(dts As String) As DateTime
    Dim delim() As Char = {" "}
    Dim parts = dts.Split(delim, 2)
    Dim d As String = parts(0).Replace(":", "-")
    Dim t As String = parts(1)
    Return DateTime.Parse(String.Format("{0}T{1}", d, t))
  End Function

  Public Function GetPremisEvent(id As PremisIdentifier) As PremisEvent
    Dim pEvt As New PremisEvent(id.IdentifierType, id.IdentifierValue, "FILE_CHARACTERIZATION")

    pEvt.EventDetail = String.Format("File '{0}' characterized by FITS {1}.  For details see objectCharacteristicsExtension.", FilePath, FitsVersion)

    Dim nds = _xml.SelectNodes("//*[@status='CONFLICT']", _xmlns) 'there were conflicts during the characterization

    Dim nds2 = _xml.SelectNodes("//fits:identification [@status='UNKNOWN']", _xmlns) 'unable to identify format

    If nds2.Count > 0 Then
      Dim evtInf As New PremisEventOutcomeInformation("UNKNOWN")
      pEvt.EventOutcomeInformation.Add(evtInf)
    ElseIf nds.Count > 0 Then
      Dim evtInf As New PremisEventOutcomeInformation("CONFLICTS")
      pEvt.EventOutcomeInformation.Add(evtInf)
    Else
      Dim evtInf As New PremisEventOutcomeInformation("OK")
      pEvt.EventOutcomeInformation.Add(evtInf)
    End If

    Return pEvt


  End Function

  Public Function GetPremisSignificantProperties() As List(Of PremisSignificantProperties)
    Dim ret As New List(Of PremisSignificantProperties)

    Dim nds As XmlNodeList = _xml.SelectNodes("//fits:metadata/*/*", _xmlns)

    For Each nd As XmlElement In nds
      If nd.GetAttribute("status") <> "CONFLICT" Then
        Dim sp As New PremisSignificantProperties(nd.LocalName, nd.InnerText)
        ret.Add(sp)
      Else
        'TODO: Warning message or PREMIS Event?
      End If
    Next

    Return ret
  End Function

  Public Function GetPremisAgent() As PremisAgent

    Dim agent As PremisAgent = New PremisAgent("SOFTWARE_VERSION", String.Format("FITS {0}", Me.FitsVersion))
    agent.AgentNames.Add(String.Format("FITS {0}", Me.FitsVersion))
    agent.AgentType = "SOFTWARE"

    Return agent

  End Function


  Public ReadOnly Property PremisCreatingApplications As List(Of PremisCreatingApplication)
    Get
      Dim ret As New List(Of PremisCreatingApplication)

      Dim nds As XmlNodeList = _xml.SelectNodes("//fits:fileinfo/fits:creatingApplicationName", _xmlns)

      For Each nd As XmlElement In nds
        Dim cappName As String = nd.InnerText
        If cappName.Length > 1 Then 'some tools seem to generate a value of '/' for application, so ignore short names
          Dim capp As New PremisCreatingApplication(cappName)
          If Me.Created.HasValue Then
            capp.DateCreatedByApplication = Me.Created
          End If
          'capp.CreatingApplicationExtensions.Add(nd)
          ret.Add(capp)
        End If
      Next

      Return ret
    End Get
  End Property

  Public ReadOnly Property PremisFixities As List(Of PremisFixity)
    Get
      Dim ret As New List(Of PremisFixity)

      Dim nds As XmlNodeList = _xml.SelectNodes("//fits:fileinfo/fits:md5checksum", _xmlns)

      For Each nd As XmlElement In nds
        Dim fxy As New PremisFixity("MD5", nd.InnerText)
        fxy.MessageDigestOriginator = String.Format("{2} [{0} {1}]", nd.GetAttribute("toolname"), nd.GetAttribute("toolversion"),
                                                    UIUCLDAPUser.GetDomainFromQualifiedID(Principal.WindowsIdentity.GetCurrent.Name))
        ret.Add(fxy)
      Next

      Return ret
    End Get
  End Property

  Public ReadOnly Property PremisFormats As List(Of PremisFormat)
    Get
      Dim ret As New List(Of PremisFormat)

      Dim nds As XmlNodeList = _xml.SelectNodes("//fits:identification/fits:identity", _xmlns)

      If nds.Count > 0 Then

        Dim nd As XmlElement = nds.Item(0)

        Dim pf As New PremisFormat(nd.GetAttribute("mimetype"))

        Dim ndvs As XmlNodeList = nd.SelectNodes("fits:version", _xmlns)
        If ndvs.Count > 1 Then
          Dim vs As String = ""
          For Each ndv As XmlElement In ndvs
            vs = String.Format("{0}{1}{2}", vs, IIf(String.IsNullOrWhiteSpace(vs), "", ", "), ndv.InnerText)
          Next
          pf.FormatNotes.Add("Conflicting Format Versions: " & vs)
        ElseIf ndvs.Count = 1 Then
          pf.FormatVersion = ndvs.Item(0).InnerText
        End If

        Dim ndexts As XmlNodeList = nd.SelectNodes("fits:externalIdentifier", _xmlns)
        If ndexts.Count > 1 Then
          Dim ex As String = ""
          For Each ndext As XmlElement In ndexts
            ex = String.Format("{0}{1}{2} {3} {4}:{5}", ex, IIf(String.IsNullOrWhiteSpace(ex), "", ", "), ndext.GetAttribute("toolname"), ndext.GetAttribute("toolversion"), ndext.GetAttribute("type"), ndext.InnerText)
          Next
          pf.FormatNotes.Add("Conflicting Format Registry Keys: " & ex)
        ElseIf ndexts.Count = 1 Then
          Dim ndext As XmlElement = ndexts.Item(0)
          pf.FormatRegistryKey = ndext.InnerText
          If ndext.GetAttribute("type").ToLower = "puid" Then
            pf.FormatRegistryName = "PRONOM"
          Else
            pf.FormatRegistryName = ndext.GetAttribute("toolname") & ": " & ndext.GetAttribute("type")
          End If
        End If


        pf.FormatNotes.Add(nd.GetAttribute("format"))

        Dim fitsTstr As String = nd.GetAttribute("toolname") & " " & nd.GetAttribute("toolversion")
        Dim tstr As String = ""
        Dim tools As XmlNodeList = nd.SelectNodes("fits:tool", _xmlns)
        For Each tool As XmlElement In tools
          tstr = String.Format("{2}{0} {1}", tool.GetAttribute("toolname"), tool.GetAttribute("toolversion"), IIf(String.IsNullOrWhiteSpace(tstr), "", tstr & ", "))
        Next

        If Not String.IsNullOrWhiteSpace(tstr) Then
          pf.FormatNotes.Add(String.Format("Format Identified By {0}: {1}", fitsTstr, tstr))
        End If

        If nds.Count > 1 Then
          For i As Integer = 1 To nds.Count - 1
            Dim nd2 As XmlElement = nds.Item(i)

            Dim fitsTstr2 As String = nd2.GetAttribute("toolname") & " " & nd2.GetAttribute("toolversion")
            Dim tstr2 As String = ""
            Dim tools2 As XmlNodeList = nd2.SelectNodes("fits:tool", _xmlns)
            For Each tool As XmlElement In tools2
              tstr2 = String.Format("{2}{0} {1}", tool.GetAttribute("toolname"), tool.GetAttribute("toolversion"), IIf(String.IsNullOrWhiteSpace(tstr2), "", tstr2 & ", "))
            Next

            If Not String.IsNullOrWhiteSpace(tstr2) Then
              pf.FormatNotes.Add(String.Format("Alternate Format '{2}' ({1}); Identified By {3}: {0}", tstr2, nd2.GetAttribute("format"), nd2.GetAttribute("mimetype"), fitsTstr2))
            End If

          Next
        End If
        ret.Add(pf)

      End If


      Return ret
    End Get
  End Property


  Public Sub New(filename As String)
    RunFITS(filename)
    _xmlns = New XmlNamespaceManager(_xml.NameTable)
    _xmlns.AddNamespace("fits", "http://hul.harvard.edu/ois/xml/ns/fits/fits_output")
  End Sub

  Private Sub RunFITS(filename As String)

    Dim output As String = ""
    Try
      'Start the child process.
      Dim p As Process = New Process()
      'Redirect the output stream of the child process.
      p.StartInfo.UseShellExecute = False
      p.StartInfo.RedirectStandardOutput = True
      p.StartInfo.RedirectStandardError = True
      'p.StartInfo.FileName = "C:\Windows\System32\cmd.exe"
      p.StartInfo.FileName = MedusaAppSettings.Settings.FitsScriptPath
      p.StartInfo.WorkingDirectory = MedusaAppSettings.Settings.FitsHome
      p.StartInfo.Arguments = String.Format("-i ""{0}""", filename)
      p.Start()
      'Read the output stream first and then wait.
      output = p.StandardOutput.ReadToEnd()
      _err = p.StandardError.ReadToEnd
      p.WaitForExit()
      If Not String.IsNullOrWhiteSpace(_err) Then
        Throw New Exception(_err)
      End If
    Catch ex As Exception
      Throw
    End Try

    _xml = New XmlDocument

    _xml.LoadXml(output)

  End Sub

End Class
