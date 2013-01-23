Public Class MedusaException
  Inherits Exception

  Public Sub New(ByVal Message As String)
    MyBase.New(Message)
  End Sub

  Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
    MyBase.New(Message, InnerException)
  End Sub

End Class
