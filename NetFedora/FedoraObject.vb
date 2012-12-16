Imports Uiuc.Library.NetFedora.ApiA

Public Class FedoraObject

  Private _repo As FedoraRepository
  Private _pid As String

  Public Sub New(repo As FedoraRepository, pid As String)
    _repo = repo
    _pid = pid
  End Sub
End Class
