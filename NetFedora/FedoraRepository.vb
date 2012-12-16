Imports Uiuc.Library.NetFedora.ApiA
Imports Uiuc.Library.NetFedora.ApiM
Imports Uiuc.Library.Medusa

Public Class FedoraRepository

  Private _repoA As FedoraAPIAClient
  Private _repoM As FedoraAPIMClient

  Public ReadOnly Property RepositoryName As String
    Get
      Dim rInfo = _repoA.describeRepository
      Return rInfo.repositoryName
    End Get
  End Property

  Public Sub New()
    Me.New("Fedora-API-A-Service-HTTP-Port", "Fedora-API-M-Service-HTTP-Port", MedusaAppSettings.Settings.FedoraAccount, MedusaAppSettings.Settings.FedoraPassword)
  End Sub

  Public Sub New(aPort As String, mPort As String, userId As String, password As String)
    _repoA = New FedoraAPIAClient(aPort)

    _repoA.ClientCredentials.UserName.UserName = userId
    _repoA.ClientCredentials.UserName.Password = password


    _repoM = New FedoraAPIMClient(mPort)

    _repoM.ClientCredentials.UserName.UserName = userId
    _repoM.ClientCredentials.UserName.Password = password

  End Sub

  Public Sub New(userId As String, password As String)
    Me.New("Fedora-API-A-Service-HTTP-Port", "Fedora-API-M-Service-HTTP-Port", userId, password)
  End Sub

  Public Function GetObject(pid As String) As FedoraObject
    Dim ret As New FedoraObject(Me, pid)

    Return ret
  End Function

End Class
