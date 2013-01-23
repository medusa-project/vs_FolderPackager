Imports Uiuc.Library.NetFedora.ApiA
Imports Uiuc.Library.NetFedora.ApiM
Imports System.Xml
Imports System.IO
Imports System.ServiceModel

Public Class FedoraObject
  Implements IEquatable(Of FedoraObject)

  Public Const FoxmlNamespace As String = "info:fedora/fedora-system:def/foxml#"
  Public Const FedoraModelNamespace As String = "info:fedora/fedora-system:def/model#"
  Public Const FedoraViewNamespace As String = "info:fedora/fedora-system:def/view#"
  Public Const FedoraUriPrefix As String = "info:fedora/"

  Public Sub New(repo As FedoraRepository, pid As String)
    _repo = repo
    _pid = pid
  End Sub

  Private _repo As FedoraRepository
  Public ReadOnly Property Repository As FedoraRepository
    Get
      Return _repo
    End Get
  End Property

  Private _pid As String
  Public ReadOnly Property Pid As String
    Get
      Return _pid
    End Get
  End Property

  Private _foxml As FoxmlDigitalObject
  Public ReadOnly Property Foxml As FoxmlDigitalObject
    Get
      'TODO: Gracefully handle not found errors
      If _foxml Is Nothing Then
        Try
          Dim byts() As Byte = _repo.ManagementClient.export(_pid, FedoraRepository.ExportFormatFoxml1_1, ExportContexts.public.ToString)

          Dim x As New XmlDocument
          x.Load(New MemoryStream(byts))

          _foxml = New FoxmlDigitalObject(x.DocumentElement)
        Catch fex As FaultException
          Throw New FedoraException("Unable to export object.", fex)
        End Try
      End If

      Return _foxml
    End Get
  End Property

  Public Function GetDatastream(id As String) As FedoraDatastream
    Dim ds = New FedoraDatastream(Me, id)

    Return ds
  End Function


  Public Function GetDataStreamUrl(dsId As String, asOfDateTime As String) As String
    Return String.Format("{0}/get/{1}/{2}/{3}", _repo.Information.repositoryName, _pid, dsId, asOfDateTime)
  End Function

  Public Function GetDataStreamUrl(dsId As String) As String
    Return String.Format("{0}/get/{1}/{2}", _repo.Information.repositoryBaseURL, _pid, dsId)
  End Function

  Public Function Equals1(other As FedoraObject) As Boolean Implements IEquatable(Of FedoraObject).Equals
    If other Is Nothing Then Return False

    If Me.Pid = other.Pid Then
      Return True
    Else
      Return False
    End If
  End Function

  Public Overrides Function Equals(obj As Object) As Boolean
    Return Me.Equals1(obj)
  End Function
End Class
