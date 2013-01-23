Public Class FedoraSearchResults
  Implements ICollection(Of FedoraObject)

  Private fields() As String = {"pid"}

  Private qry As ApiA.FieldSearchQuery
  Private fRepo As FedoraRepository
  Private cnt As Integer = -1

  Public Sub New(repo As FedoraRepository, query As ApiA.FieldSearchQuery)
    fRepo = repo
    qry = query
  End Sub

  Public Function Contains(item As FedoraObject) As Boolean Implements ICollection(Of FedoraObject).Contains
    'brute force linear search
    For Each obj In Me
      If obj.Equals1(item) Then
        Return True
      End If
    Next
    Return False
  End Function

  Public Sub CopyTo(array() As FedoraObject, arrayIndex As Integer) Implements ICollection(Of FedoraObject).CopyTo
    If array Is Nothing Then
      Throw New ArgumentNullException("The array is nothing.")
    End If
    If arrayIndex < 0 Then
      Throw New ArgumentOutOfRangeException("The arrayIndex is less than zero.")
    End If

    Dim i As Integer = arrayIndex
    For Each obj In Me
      If i >= array.Count Then
        Throw New ArgumentException("Not enough space in the array.")
      End If
      array(i) = obj
      i = i + 1
    Next

  End Sub

  ''' <summary>
  ''' Return a count of all objects returned by search
  ''' </summary>
  ''' <value></value>
  ''' <returns></returns>
  ''' <remarks>Note that the ApiA listSession.completeListSize is always empty, so must retrieve all records in order to count them 
  ''' Also note that if the results are less than maxResults then the listSession itself is empty, so must get count of resultList array.</remarks>
  Public ReadOnly Property Count As Integer Implements ICollection(Of FedoraObject).Count
    Get
      If cnt >= 0 Then
        Return cnt
      End If

      'do it the slow way
      Dim re As ApiA.FieldSearchResult = fRepo.AccessClient.findObjects(fields, FedoraRepository.MaxSearchResults, qry)
      Dim i As Integer = re.resultList.Count
      Do Until re.listSession Is Nothing
        re = fRepo.AccessClient.resumeFindObjects(re.listSession.token)
        i = i + re.resultList.Count
      Loop

      cnt = i
      Return i
    End Get
  End Property

  Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of FedoraObject).IsReadOnly
    Get
      Return True
    End Get
  End Property

  Public Iterator Function GetEnumerator() As IEnumerator(Of FedoraObject) Implements IEnumerable(Of FedoraObject).GetEnumerator

    Dim re As ApiA.FieldSearchResult = fRepo.AccessClient.findObjects(fields, FedoraRepository.MaxSearchResults, qry)
    For Each r In re.resultList
      Yield New FedoraObject(Me.fRepo, r.pid)
    Next
    Do Until re.listSession Is Nothing
      re = fRepo.AccessClient.resumeFindObjects(re.listSession.token)
      For Each r In re.resultList
        Yield New FedoraObject(Me.fRepo, r.pid)
      Next
    Loop

  End Function

  Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
    Yield Me.GetEnumerator
  End Function

  Public Sub Add(item As FedoraObject) Implements ICollection(Of FedoraObject).Add
    Throw New NotSupportedException()
  End Sub

  Public Sub Clear() Implements ICollection(Of FedoraObject).Clear
    Throw New NotSupportedException()
  End Sub

  Public Function Remove(item As FedoraObject) As Boolean Implements ICollection(Of FedoraObject).Remove
    Throw New NotSupportedException()
  End Function

 
End Class


