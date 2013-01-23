Imports Uiuc.Library.NetFedora.ApiA
Imports Uiuc.Library.NetFedora.ApiM
Imports Uiuc.Library.Medusa

Public Class FedoraRepository

  Friend Const ExportFormatFoxml1_1 As String = "info:fedora/fedora-system:FOXML-1.1"
  Friend Const ExportFormatFoxml1_0 As String = "info:fedora/fedora-system:FOXML-1.0"
  Friend Const ExportFormatMets1_1 As String = "info:fedora/fedora-system:METSFedoraExt-1.1"
  Friend Const ExportFormatMets1_0 As String = "info:fedora/fedora-system:METSFedoraExt-1.0"
  Friend Const ExportFormatAtom1_1 As String = "info:fedora/fedora-system:ATOM-1.1"
  Friend Const ExportFormatAtomZip1_1 As String = "info:fedora/fedora-system:ATOMZip-1.1"

  Public Shared MaxSearchResults As Integer = 100 'the maximum size of a search result returned by a single find or resume request

  Public Sub New()
    Me.New("Fedora-API-A-Service-HTTP-Port", "Fedora-API-M-Service-HTTP-Port", MedusaAppSettings.Settings.FedoraAccount, MedusaAppSettings.Settings.FedoraPassword)
  End Sub

  Public Sub New(userId As String, password As String)
    Me.New("Fedora-API-A-Service-HTTP-Port", "Fedora-API-M-Service-HTTP-Port", userId, password)
  End Sub

  Public Sub New(aPort As String, mPort As String, userId As String, password As String)
    _repoA = New FedoraAPIAClient(aPort)

    _repoA.ClientCredentials.UserName.UserName = userId
    _repoA.ClientCredentials.UserName.Password = password


    _repoM = New FedoraAPIMClient(mPort)

    _repoM.ClientCredentials.UserName.UserName = userId
    _repoM.ClientCredentials.UserName.Password = password

  End Sub

  Private _repoA As FedoraAPIAClient
  Friend ReadOnly Property AccessClient As FedoraAPIAClient
    Get
      Return _repoA
    End Get
  End Property

  Private _repoM As FedoraAPIMClient
  Friend ReadOnly Property ManagementClient As FedoraAPIMClient
    Get
      Return _repoM
    End Get
  End Property

  Private _rInfo As RepositoryInfo
  Public ReadOnly Property Information As RepositoryInfo
    Get
      If _rInfo Is Nothing Then
        _rInfo = _repoA.describeRepository
      End If
      Return _rInfo
    End Get
  End Property

  ''' <summary>
  ''' Return a single object given its pid
  ''' </summary>
  ''' <param name="pid"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function GetObject(pid As String) As FedoraObject
    Dim ret As New FedoraObject(Me, pid)

    Return ret
  End Function

  ''' <summary>
  ''' Search for objects using a freeform search terms string.
  ''' </summary>
  ''' <param name="terms"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function FindObjects(terms As String) As FedoraSearchResults
    Dim qry As New ApiA.FieldSearchQuery()
    qry.Item = terms

    Return New FedoraSearchResults(Me, qry)
  End Function

  ''' <summary>
  ''' Search for objects having a given value for the given property using the given operator.
  ''' </summary>
  ''' <param name="prop"></param>
  ''' <param name="op"></param>
  ''' <param name="value"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function FindObjects(prop As String, op As ApiA.ComparisonOperator, value As String) As FedoraSearchResults
    Dim cond(0) As ApiA.Condition
    cond(0) = New ApiA.Condition
    cond(0).property = prop
    cond(0).operator = op
    cond(0).value = value
    Return Me.FindObjects(cond)
  End Function

  ''' <summary>
  ''' Search for objects using an array of ApiA.Condition
  ''' </summary>
  ''' <param name="searchConditions"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Function FindObjects(searchConditions() As ApiA.Condition) As FedoraSearchResults
    Dim qry As New ApiA.FieldSearchQuery()
    Dim cond As New ApiA.FieldSearchQueryConditions
    cond.condition = searchConditions
    qry.Item = cond

    Return New FedoraSearchResults(Me, qry)
  End Function

  Public Function IngestObject(foxml As FoxmlDigitalObject, logMsg As String) As FedoraObject
    Dim xmlStr As String = foxml.GetXML
    Dim enc As New Text.UTF8Encoding
    Dim byt() As Byte = enc.GetBytes(xmlStr)

    Dim pid As String = Me.ManagementClient.ingest(byt, FedoraRepository.ExportFormatFoxml1_1, logMsg)

    Return New FedoraObject(Me, pid)

  End Function

  Public Function PurgeObject(pid As String, logMsg As String) As Date
    Dim dts As String = Me.ManagementClient.purgeObject(pid, logMsg, False)
    Return Date.Parse(dts)
  End Function


End Class

Friend Enum ExportContexts
  [public]
  migrate
  archive
End Enum