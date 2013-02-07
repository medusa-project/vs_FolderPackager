Imports Uiuc.Library.Fits
Imports Uiuc.Library.Premis
Imports Uiuc.Library.Medusa
Imports System.Net
Imports System.IO
Imports System.Xml

Public Class FitsController
  Inherits System.Web.Mvc.Controller

  Function GetPremis(id As String, url As String, validate As Boolean?) As ActionResult

    If Not validate.HasValue Then
      validate = False
    End If

    If String.IsNullOrWhiteSpace(url) Then
      Response.StatusCode = HttpStatusCode.BadRequest
      Return Content("Url is a required paramater.", "text/plain")
    End If

    'retrieve the file specified by the url and save to temp file

    Dim httpReq As WebRequest = Nothing
    Dim httpRsp As WebResponse = Nothing
    Try
      httpReq = WebRequest.Create(url)
      httpRsp = httpReq.GetResponse
    Catch wex As WebException
      Response.StatusCode = HttpStatusCode.BadRequest
      Return Content(wex.Message, "text/plain")
    Catch ex As Exception
      Response.StatusCode = HttpStatusCode.InternalServerError
      Return Content(ex.Message, "text/plain")
    End Try

    Dim filename As String = ""
    Try
      filename = Path.GetTempFileName()
      Dim filestrm As New FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None)

      httpRsp.GetResponseStream.CopyTo(filestrm)

      filestrm.Close()
    Catch ex As Exception
      Response.StatusCode = HttpStatusCode.InternalServerError
      Return Content(ex.Message, "text/plain")
    End Try

    httpRsp.GetResponseStream.Close()

    'run fits
    Dim fResult As FitsResult
    Try
      fResult = New FitsResult(filename)
    Catch ex As Exception
      Response.StatusCode = HttpStatusCode.InternalServerError
      Return Content(ex.Message, "text/plain")
    End Try

    'cleanup the temp file
    If Not String.IsNullOrWhiteSpace(filename) Then
      System.IO.File.Delete(filename)
    End If

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
    pCont.Agents.Add(pAgt)
    pCont.Events.Add(pEvt)
    pCont.ValidateXML = validate

    Dim pStr = pCont.GetXML


    Return Content(pStr.ToString, "text/xml")
  End Function

End Class
