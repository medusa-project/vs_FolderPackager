Public Class FedoraDatastream

  Public Sub New(obj As FedoraObject, id As String)
    _obj = obj
    _id = id
  End Sub

  Private _obj As FedoraObject
  Public ReadOnly Property [Object] As FedoraObject
    Get
      Return _obj
    End Get
  End Property

  Private _id As String
  Public ReadOnly Property Id As String
    Get
      Return _id
    End Get
  End Property

  Private _foxml As FoxmlDatastream
  Public ReadOnly Property Foxml As FoxmlDatastream
    Get

      If _foxml Is Nothing Then
        _foxml = Me.Object.Foxml.DataStreams.SingleOrDefault(Function(d) d.Id = Me.Id)
      End If

      Return _foxml
    End Get
  End Property


End Class
