Imports System.IO
Imports System.Net
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json
Imports Uiuc.Library.Medusa

''' <summary>
''' Collection as instantiated from JSON returned by the registry
''' </summary>
''' <remarks></remarks>
<DataContract(Namespace:="")> _
Public Class Collection
  Const MAX_RETRY_COUNT = 5

  <DataMember(name:="id")> _
  Public Property Id As Integer

  <DataMember(name:="uuid")> _
  Public Property Uuid As String

  <DataMember(name:="title")> _
  Public Property Title As String

  <DataMember(name:="root_directory_id")> _
  Public Property RootDirectoryId As Integer

  <DataMember(name:="file_group_ids")> _
  Public Property FileGroupIds As List(Of Integer)

  Public Shared Function Create(json As Stream) As Collection
    Dim ds As New DataContractJsonSerializer(GetType(Collection))
    Dim result As Collection = DirectCast(ds.ReadObject(json), Collection)
    ds = Nothing
    Return result
  End Function

  Public Shared Function Create(json As String) As Collection
    Dim encoding As New System.Text.UTF8Encoding
    Dim bytes() As Byte = encoding.GetBytes(json)

    Using os As New MemoryStream
      os.Write(bytes, 0, bytes.Length)
      os.Position = 0
      Return Collection.Create(os)
    End Using
  End Function

  Public Shared Function Create(id As Integer) As Collection
    Dim url As String = String.Format(MedusaAppSettings.Settings.GetCollectionJsonUrl, id)
    Dim json As String = Collection.FetchURL(url)
    Return Collection.Create(json)
  End Function

  Private Shared Function FetchURL(ByVal url As String) As String
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
          Trace.TraceError("Error Fetching URL: {1} -- {0}", ex.Message, url)

          Return ""
          Exit Function
        Else
          Trace.TraceInformation(String.Format("Retrying FetchURL: {0}.  Try: {1}", uri, tries))
        End If
      Catch ex As Exception
        Trace.TraceError("Error Fetching URL: {1} -- {0}", ex.Message, url)

        Return ""
        Exit Function
      End Try
      Threading.Thread.Sleep(((3 ^ tries) - 1) * 1000)
      tries = tries + 1
      httpReq = WebRequest.Create(uri)
    Loop

    Dim http_mime As String = httpRsp.ContentType

    Dim strm As Stream = httpRsp.GetResponseStream

    Dim strr As New StreamReader(strm, Text.Encoding.UTF8, True)

    Return strr.ReadToEnd

  End Function
End Class

