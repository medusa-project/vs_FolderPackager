Imports System.Xml
Imports System.Configuration
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.IO
Imports System.Security.Cryptography
Imports Uiuc.Library.Premis
Imports Uiuc.Library.MetadataUtilities

Public Class CollectionRecord
  Const MAX_RETRY_COUNT = 5

  Private _xml As XmlDocument
  Private _xmlns As XmlNamespaceManager
  Private _id As String
  Private _url As String
  Private _evt As PremisEvent
  Private _objChar As PremisObjectCharacteristics
  Private _outfile As String

  Public ReadOnly Property Url As String
    Get
      Return _url
    End Get
  End Property

  Public ReadOnly Property Id As String
    Get
      Return _id
    End Get
  End Property

  Public ReadOnly Property ModsXml As XmlDocument
    Get
      Return _xml
    End Get
  End Property

  Public ReadOnly Property Uuid As String
    Get
      Dim ret As String = ""
      If _xml IsNot Nothing Then
        Dim nd = _xml.SelectSingleNode("/m:mods/m:identifier[@type='uuid']", _xmlns)
        If nd IsNot Nothing Then
          ret = nd.InnerText
        End If
      End If
      Return ret
    End Get
  End Property

  Public ReadOnly Property CollectionTitle As String
    Get
      Dim ret As String = ""
      If _xml IsNot Nothing Then
        Dim nd = _xml.SelectSingleNode("/m:mods/m:titleInfo/m:title", _xmlns)
        If nd IsNot Nothing Then
          ret = nd.InnerText
        End If
      End If
      Return ret
    End Get
  End Property

  Public ReadOnly Property Filename As String
    Get
      Return _outfile
    End Get
  End Property

  Public Sub New(id As String, folder As String)
    _id = id
    _url = String.Format(MedusaAppSettings.Settings.GetCollectionModsUrl, id)

    _evt = New PremisEvent("TEMP", "TEMP", "CAPTURE")
    _objChar = New PremisObjectCharacteristics

    _outfile = FetchURL(_url, _evt, _objChar, folder, Path.Combine(folder, "mods.xml"))

  End Sub

  Public Sub New(id As String)
    _id = id
    _url = String.Format(MedusaAppSettings.Settings.GetCollectionModsUrl, id)

    _evt = New PremisEvent("TEMP", "TEMP", "CAPTURE")
    _objChar = New PremisObjectCharacteristics

    _outfile = FetchURL(_url, _evt, _objChar, Path.GetTempPath, Path.GetTempFileName)
  End Sub

  Protected Sub New()
    'don't want a public sub new w/o params
  End Sub

  Public ReadOnly Property PremisEvent As PremisEvent
    Get
      Return _evt
    End Get
  End Property

  Public ReadOnly Property PremisObjectCharacteristics As PremisObjectCharacteristics
    Get
      Return _objChar
    End Get
  End Property

  Private Function FetchURL(ByVal url As String, ByVal pEvt As PremisEvent, ByVal pObjChar As PremisObjectCharacteristics, recPath As String, Optional saveFile As String = "") As String
    Dim uri As New Uri(url)
    Dim size As Long = 0

    Dim httpReq As WebRequest = WebRequest.Create(uri)
    Dim httpRsp As WebResponse = Nothing

    Dim tries As Integer = 0
    Do Until tries > MAX_RETRY_COUNT
      Try
        httpRsp = httpReq.GetResponse
        Exit Do
      Catch ex As WebException
        If tries >= MAX_RETRY_COUNT Or CType(ex.Response, HttpWebResponse).StatusCode = HttpStatusCode.NotFound Then
          Trace.TraceError("Error in Folder: {0}", recPath)
          Trace.TraceError("Fetching URL: {1} -- {0}", ex.Message, url)

          Dim evtInfo As New PremisEventOutcomeInformation([Enum].GetName(GetType(WebExceptionStatus), ex.Status))
          Dim evtDet As New PremisEventOutcomeDetail(ex.Message)
          evtInfo.EventOutcomeDetails.Add(evtDet)
          Dim evtDet2 As New PremisEventOutcomeDetail("A default MODS collection record will be created instead.")
          evtInfo.EventOutcomeDetails.Add(evtDet2)
          pEvt.EventOutcomeInformation.Add(evtInfo)
          Return ""
          Exit Function
        Else
          Trace.TraceInformation(String.Format("{2} Retrying FetchURL: {0}.  Try: {1}", uri, tries, recPath))
        End If
      Catch ex As Exception
        Trace.TraceError("Error in Folder: {0}", recPath)
        Trace.TraceError("Fetching URL: {1} -- {0}", ex.Message, url)

        Dim evtInfo As New PremisEventOutcomeInformation(ex.GetType.Name)
        Dim evtDet As New PremisEventOutcomeDetail(ex.Message)
        evtInfo.EventOutcomeDetails.Add(evtDet)
        pEvt.EventOutcomeInformation.Add(evtInfo)
        Return ""
        Exit Function
      End Try
      Threading.Thread.Sleep(((3 ^ tries) - 1) * 1000)
      tries = tries + 1
      httpReq = WebRequest.Create(uri)
    Loop

    Dim http_mime As String = httpRsp.ContentType

    Dim outFileName As String
    If String.IsNullOrWhiteSpace(saveFile) Then
      Dim fn As String = Path.GetFileName(uri.LocalPath)
      'replace invalid chars in name
      fn = Regex.Replace(fn, String.Format("[{0}]", New String(Path.GetInvalidFileNameChars)), "_") 'TODO: This could result in filename collisions
      outFileName = Path.Combine(recPath, fn)
    Else
      outFileName = saveFile
    End If

    Dim alg As String = MedusaAppSettings.Settings.ChecksumAlgorithm

    Dim strm As Stream = httpRsp.GetResponseStream

    Dim outStrm As Stream = File.Open(outFileName, FileMode.Create, FileAccess.Write)

    Dim sha1 As HashAlgorithm = HashAlgorithm.Create(alg)

    Dim byt(8 * 1024) As Byte
    Dim len As Integer = strm.Read(byt, 0, byt.Length)
    Dim len2 As Integer = 0
    While len > 0
      size = size + len
      outStrm.Write(byt, 0, len)
      If sha1 IsNot Nothing Then len2 = sha1.TransformBlock(byt, 0, len, byt, 0)
      len = strm.Read(byt, 0, byt.Length)
    End While

    If sha1 IsNot Nothing Then
      sha1.TransformFinalBlock(byt, 0, 0)

      Dim pFix As New PremisFixity(alg, MetadataFunctions.BytesToHexStr(sha1.Hash))
      pObjChar.Fixities.Add(pFix)
    End If

    pObjChar.Size = size
    outStrm.Close()
    strm.Close()

    If TypeOf httpRsp Is HttpWebResponse Then
      Dim evtInfoOK As New PremisEventOutcomeInformation([Enum].GetName(GetType(HttpStatusCode), CType(httpRsp, HttpWebResponse).StatusCode))
      pEvt.EventOutcomeInformation.Add(evtInfoOK)
    Else
      Dim evtInfoOK As New PremisEventOutcomeInformation(If(httpRsp.ContentLength > 0, "OK", "InternalServerError"))
      pEvt.EventOutcomeInformation.Add(evtInfoOK)
    End If

    Dim mime As String = MetadataFunctions.GetMimeFromFile(outFileName, http_mime)

    If pObjChar.Formats.Count = 0 OrElse mime <> pObjChar.Formats.Item(0).FormatName Then
      Dim pForm2 As New PremisFormat(mime)
      pForm2.FormatNotes.Add("This is the MIME type as determined by URL Moniker Library.")
      pObjChar.Formats.Add(pForm2)

      If pObjChar.Formats.Count <> 1 Then
        Dim pEvtDet As New PremisEventOutcomeDetail("The MIME type returned by the HTTP Request or as determined by the URL Moniker Library does not match the expected MIME type.")
        pEvt.EventOutcomeInformation.Item(0).EventOutcomeDetails.Add(pEvtDet)
      End If
    End If

    _xml = New XmlDocument
    _xmlns = New XmlNamespaceManager(_xml.NameTable)
    _xmlns.AddNamespace("m", "http://www.loc.gov/mods/v3")
    _xml.Load(outFileName)

    Return Path.GetFileName(outFileName)

  End Function

End Class
