﻿Imports Uiuc.Library.Fits
Imports Uiuc.Library.Premis
Imports Uiuc.Library.Medusa
Imports System.Net
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports System.IO
Imports System.Xml
Imports System.Text.RegularExpressions

Public Class FitsController
  Inherits System.Web.Mvc.Controller

  Protected Overloads Overrides Sub Initialize(ByVal rc As RequestContext)
    MyBase.Initialize(rc)

    ServicePointManager.ServerCertificateValidationCallback = AddressOf ValidateCert
    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
  End Sub

  Public Function ValidateCert(sender As Object, certificate As X509Certificate, chain As X509Chain, sslPolicyErrors As SslPolicyErrors) As Boolean
    'ignore all certificate errors
    Return True
  End Function

  Private Function HandleWebException(wex As WebException) As ActionResult
    If TypeOf wex.Response Is HttpWebResponse AndAlso CType(wex.Response, HttpWebResponse).StatusCode = HttpStatusCode.Unauthorized Then
      Dim resp As HttpWebResponse = CType(wex.Response, HttpWebResponse)
      Dim wwwAuth As String = resp.GetResponseHeader("WWW-Authenticate")
      Dim rlm As String = GetRealm(wwwAuth)
      Response.AddHeader("WWW-Authenticate", "Basic " & rlm)
      Return New HttpUnauthorizedResult()
    Else
      Return Content(wex.Message, "text/plain")
    End If
  End Function

  Private Function GetRealm(auth As String) As String
    Dim ret As String = ""
    Dim re As New Regex("realm\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase)

    Dim ms = re.Matches(auth)

    Dim m = ms.Item(1).Groups.Item(0)
    ret = m.Value

    Return ret
  End Function

  Function GetFits(url As String) As ActionResult
    Dim fResult As FitsResult = Nothing
    Try
      fResult = Me.GetFitsResult(url)
    Catch wex As WebException
      Return HandleWebException(wex)
    Catch ex As Exception
      Return Content(ex.Message, "text/plain")
    End Try

    Return Content(fResult.FitsXml.OuterXml, "text/xml")
  End Function

  Function GetPremis(id As String, url As String, validate As Boolean?) As ActionResult

    If Not validate.HasValue Then
      validate = False
    End If

    Dim fResult As FitsResult = Nothing
    Try
      fResult = Me.GetFitsResult(url)
    Catch wex As WebException
      Return HandleWebException(wex)
    Catch ex As Exception
      Return Content(ex.Message, "text/plain")
    End Try


    'create a premis object containing fits result

    Dim pObj As New PremisObject("URL", url, PremisObjectCategory.File)

    If Not String.IsNullOrWhiteSpace(id) Then
      Dim pLocalId As New PremisIdentifier("LOCAL", id)
      pObj.ObjectIdentifiers.Insert(0, pLocalId)
    End If

    Dim pObjChar As New PremisObjectCharacteristics()
    pObjChar.Fixities.AddRange(fResult.PremisFixities)
    pObjChar.Formats.AddRange(fResult.PremisFormats)
    pObjChar.Size = fResult.Size
    pObjChar.CreatingApplications.AddRange(fResult.PremisCreatingApplications)
    pObj.SignificantProperties.AddRange(fResult.GetPremisSignificantProperties)

    pObj.ObjectCharacteristics.Add(pObjChar)

    Dim pId As New PremisIdentifier("LOCAL", "FITS_EVENT")
    Dim pEvt = fResult.GetPremisEvent(pId)

    Dim pAgt = fResult.GetPremisAgent()
    pEvt.LinkToAgent(pAgt)

    pObj.LinkToEvent(pEvt)


    Dim pCont = New PremisContainer(pObj)
    'pCont.Agents.Add(pAgt)
    pCont.Events.Add(pEvt)
    pCont.ValidateXML = validate

    Dim pStr = pCont.GetXML


    Return Content(pStr.ToString, "text/xml")
  End Function

  Private Sub AddTrustedDomansToCache(cache As CredentialCache, cred As NetworkCredential)
    Dim trustedAll As String = ConfigurationManager.AppSettings.Item("TrustedDomains")
    Dim delims() As Char = {",", ";", " ", "|"}
    Dim trustedArr() As String = trustedAll.Split(delims, StringSplitOptions.RemoveEmptyEntries)
    For Each trusted In trustedArr
      Dim u As New Uri(String.Format("http://{0}/", trusted))
      cache.Add(u, "Basic", cred)
      cache.Add(u, "Digest", cred)
    Next

  End Sub

  Private Function GetFitsResult(url As String) As FitsResult
    If String.IsNullOrWhiteSpace(url) Then
      Response.StatusCode = HttpStatusCode.BadRequest
      Throw New Exception("Url is a required paramater.")
    End If

    'retrieve the file specified by the url and save to temp file

    'Need to be able to handle authentication 
    Dim auth As String = Request.Headers.Item("Authorization")
    Dim uid As String = ""
    Dim pwd As String = ""
    Dim cred As NetworkCredential = Nothing
    Dim credCache As CredentialCache = Nothing
    If Not String.IsNullOrWhiteSpace(auth) Then
      If auth.Trim.StartsWith("basic ", StringComparison.InvariantCultureIgnoreCase) Then
        auth = auth.Substring(6)
        auth = ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(auth))
        Dim parts() As String = Split(auth, ":", 2, CompareMethod.Binary)
        uid = parts(0)
        pwd = parts(1)
        cred = New NetworkCredential(uid, pwd)
        credCache = New CredentialCache()
        Dim u As New Uri(url)
        credCache.Add(u, "Basic", cred)
        credCache.Add(u, "Digest", cred)
        AddTrustedDomansToCache(credCache, cred)
      End If
    End If

    Dim httpReq As WebRequest = Nothing
    Dim httpRsp As WebResponse = Nothing
    Try
      httpReq = WebRequest.Create(url)
      If credCache IsNot Nothing Then
        httpReq.Credentials = credCache
      End If
      httpRsp = httpReq.GetResponse
    Catch wex As WebException
      Response.StatusCode = HttpStatusCode.BadRequest
      Throw
    Catch ex As Exception
      Response.StatusCode = HttpStatusCode.BadRequest
      Throw
    End Try

    'get some metadata from the http response
    Dim httpLen As Long = httpRsp.ContentLength
    Dim httpTyp As String = httpRsp.ContentType
    Dim httpMd5 As String = httpRsp.Headers.Item(HttpResponseHeader.ContentMd5)

    Dim filename As String = ""
    Try
      filename = Path.GetTempFileName()

      Dim filestrm As New FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None)

      httpRsp.GetResponseStream.CopyTo(filestrm)

      filestrm.Close()

    Catch ex As Exception
      Response.StatusCode = HttpStatusCode.BadRequest
      Throw
    End Try

    httpRsp.GetResponseStream.Close()

    'run fits
    Dim fResult As FitsResult
    Try
      fResult = New FitsResult(filename)
    Catch ex As Exception
      Response.StatusCode = HttpStatusCode.BadRequest
      Throw
    End Try

    'cleanup the temp file
    If Not String.IsNullOrWhiteSpace(filename) Then
      System.IO.File.Delete(filename)
    End If

    If httpLen <> fResult.Size Then
      Response.StatusCode = HttpStatusCode.BadRequest
      Throw New Exception("HTTP Content-Length does not match the FITS file size.")
    End If

    If Not String.IsNullOrWhiteSpace(httpMd5) AndAlso Not httpMd5.Equals(fResult.Md5Base64, StringComparison.InvariantCultureIgnoreCase) Then
      Response.StatusCode = HttpStatusCode.BadRequest
      Throw New Exception("HTTP Content-MD5 does not match the FITS MD5 checksum.")
    End If

    Return fResult
  End Function
End Class
