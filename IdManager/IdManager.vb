Imports System.Security
Imports System.Text.RegularExpressions
Imports Uiuc.Library.Medusa

''' <summary>
''' A collection of shared functions for dealing with identifiers in the Medusa repository
''' </summary>
''' <remarks></remarks>
Public Class IdManager

  ''' <summary>
  ''' Given the human-readable identifier return the  medusa id
  ''' </summary>
  ''' <param name="humanId"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetMedusaIdentifier(humanId As String) As String
    Dim id As String = ""
    Using db As New MedusaDataContext

      id = (From i In db.IdentifierLookups Where i.HumanIdentifier = humanId Select i.MedusaIdentifier).SingleOrDefault

    End Using

    Return id
  End Function

  ''' <summary>
  ''' Given the medusa identifier return the  human-readable id
  ''' </summary>
  ''' <param name="medusaId"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetHumanIdentifier(medusaId As String) As String
    Dim id As String = ""
    Using db As New MedusaDataContext

      id = (From i In db.IdentifierLookups Where i.MedusaIdentifier = medusaId Select i.HumanIdentifier).SingleOrDefault

    End Using

    Return id
  End Function

  ''' <summary>
  ''' Add a new identifier entry; if the human-readable id already exists return its already associated medusa id instead; otherwise add the new id and return it
  ''' </summary>
  ''' <param name="humanId"></param>
  ''' <param name="medusaId"></param>
  ''' <returns>The medusa id of identifier; if this is different than the submitted local id then the human id already existsed in the databasde</returns>
  ''' <remarks></remarks>
  Public Shared Function AddIdentifier(humanId As String, medusaId As String) As String
    Dim id As String = ""
    Using db As New MedusaDataContext

      id = (From i In db.IdentifierLookups Where i.HumanIdentifier = humanId Select i.MedusaIdentifier).SingleOrDefault

      If String.IsNullOrWhiteSpace(id) Then

        Dim newId As New IdentifierLookup
        newId.HumanIdentifier = humanId
        newId.MedusaIdentifier = medusaId
        newId.RegisteredDate = Now
        newId.RegisteredBy = Principal.WindowsIdentity.GetCurrent.Name

        db.IdentifierLookups.InsertOnSubmit(newId)

        db.SubmitChanges()

        id = medusaId

      End If

    End Using

    Return id
  End Function

  ''' <summary>
  ''' Generate a new Medusa Local Identifier which follows this syntax:  HandleProject:Guid-CheckDigit
  ''' </summary>
  ''' <returns>Handle</returns>
  ''' <remarks>HandlePrefix is optional.  A check digit is added to the end of the GUID using the Verhoeff algorithm</remarks>
  Public Shared Function GenerateLocalIdentifier() As String
    Dim project As String = MedusaAppSettings.Settings.HandleProject
    Dim uuid As Guid = Guid.NewGuid


    Dim uuidStr As String = uuid.ToString.Replace("-", "")
    Dim checkD As Char = CheckDigit.GenerateCheckCharacter(uuidStr)
    Dim handle As String = String.Format("{0}:{1}-{2}", project, uuid.ToString, checkD)

    Return handle
  End Function

  ''' <summary>
  ''' Given a complete handle, return just the local part (minus the registered handle prefix)
  ''' </summary>
  ''' <param name="handle"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetLocalIdentifier(handle As String) As String
    Dim re As New Regex("^\s*(\d+)/([^:]+:[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}-[A-F0-9](?:\.\d+)?)\s*$", RegexOptions.IgnoreCase)
    Dim ret As String

    Dim m As Match = re.Match(handle)

    If m.Success Then
      Dim prefix As String = m.Groups.Item(1).Value
      Dim project As String = m.Groups.Item(2).Value
      ret = project
    Else
      Throw New Exception("Invalid Handle: " & handle)
    End If

    Return ret
  End Function

  ''' <summary>
  ''' If the given handle is valid return True; else return False.  This checks for valid syntax, prefix, project, and Guid Check Digit.
  ''' </summary>
  ''' <param name="handle"></param>
  ''' <returns></returns>
  ''' <remarks>HandlePrefix is optional.</remarks>
  Public Shared Function ValidateHandle(ByVal handle As String) As Boolean
    Dim re As New Regex("^\s*(?:(\d+)/)?([^:]+):([A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}-[A-F0-9])(?:\.(\d+))?\s*$", RegexOptions.IgnoreCase)

    Dim m As Match = re.Match(handle)

    If m.Success Then
      Dim prefix As String = m.Groups.Item(1).Value
      Dim project As String = m.Groups.Item(2).Value
      Dim uuidPlusCheck As String = m.Groups.Item(3).Value
      Dim localId As String = m.Groups.Item(4).Value

      If (Not String.IsNullOrWhiteSpace(prefix)) AndAlso prefix <> MedusaAppSettings.Settings.HandlePrefix Then
        Return False
      ElseIf project <> MedusaAppSettings.Settings.HandleProject Then
        Return False
      ElseIf CheckDigit.ValidateCheckCharacter(uuidPlusCheck.Replace("-", "")) = False Then
        Return False
      ElseIf (Not String.IsNullOrWhiteSpace(localId)) AndAlso (Not IsNumeric(localId)) Then
        Return False
      End If
    Else
      Return False
    End If

    Return True
  End Function

  Public Shared Function GetIdType(id As String) As String
    Dim idType As String = "LOCAL"
    If id.StartsWith(MedusaAppSettings.Settings.HandlePrefix & "/") Then
      idType = "HANDLE"
    End If
    Return idType
  End Function

End Class

