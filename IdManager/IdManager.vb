Imports System.Security
Imports System.Text.RegularExpressions
Imports Uiuc.Library.Medusa
Imports Uiuc.Library.Premis
Imports Uiuc.Library.HandleClient

''' <summary>
''' A collection of shared functions for dealing with identifiers in the Medusa repository
''' </summary>
''' <remarks></remarks>
Public Class IdManager

  ''' <summary>
  ''' Given the URI return the associated handle
  ''' </summary>
  ''' <param name="uri"></param>
  ''' <returns>The handle or empty string if the uri is not found</returns>
  ''' <remarks></remarks>
  Public Shared Function GetHandle(uri As Uri) As String
    Dim hndl As String = ""
    Using db As New MedusaDataContext(MedusaAppSettings.Settings.ConnectionString)

      hndl = (From ha In db.HandleActions Where ha.target = uri.ToString Order By ha.id Descending Select ha.handle).FirstOrDefault

    End Using

    Return hndl

  End Function

  ''' <summary>
  ''' Register a handle for the file or folder object
  ''' </summary>
  ''' <param name="newFPath"></param>
  ''' <param name="pObj"></param>
  ''' <returns></returns>
  ''' <remarks>This function requires the pContainer and pUserAgent be instantiated</remarks>
  Public Shared Function RegisterFileHandle(newFPath As String, pObj As PremisObject, pContainer As PremisContainer, pUserAgent As PremisAgent, pSoftAgent As PremisAgent) As String
    If pContainer Is Nothing Or pUserAgent Is Nothing Or pSoftAgent Is Nothing Then
      Throw New IdManagementException("Object is not in state where a handle can be registered.")
    End If

    Dim urib As New UriBuilder
    urib.Scheme = "file"
    urib.Path = newFPath

    'register a handle for the URI if not already done
    Dim handle As String = IdManager.GetHandle(urib.Uri)
    Dim local_id As String

    If Not String.IsNullOrWhiteSpace(handle) Then
      'use the already registered handle 
      local_id = IdManager.ParseLocalIdentifier(handle)
    Else
      local_id = pObj.LocalIdentifierValue
    End If

    'Register a new handle for the file, if needed
    Try
      Dim hc As HandleClient.HandleClient = HandleClient.HandleClient.CreateUpdateHandle(local_id, urib.Uri.ToString, pUserAgent.EmailIdentifierValue, _
                                                                                      String.Format("Collection: {0}", MedusaAppSettings.Settings.CollectionName))
      handle = hc.handle_value
      If Not IdManager.ValidateHandle(handle) Then
        Trace.TraceError("Invalid Handle: " & handle)
        Trace.Flush()
        Throw New IdManagementException("Invalid Handle: " & handle)
      End If

      pObj.ObjectIdentifiers.Add(New PremisIdentifier("HANDLE", handle))

      Dim hEvt As PremisEvent = hc.GetPremisEvent(New PremisIdentifier("LOCAL", pContainer.NextID))
      hEvt.LinkToObject(pObj)
      hEvt.LinkToAgent(pUserAgent)
      hEvt.LinkToAgent(pSoftAgent)

      pContainer.Events.Add(hEvt)

    Catch ex As Exception
      Trace.TraceError("Handle: {0}", ex.Message)
      Trace.Flush()
      MedusaHelpers.EmailException(ex)

      Dim hEvt As New PremisEvent("LOCAL", pContainer.NextID, "HANDLE_CREATION")
      Dim hEvtOut As New PremisEventOutcomeInformation("FAILED")
      hEvtOut.EventOutcomeDetails.Add(New PremisEventOutcomeDetail(ex.Message))
      hEvt.EventOutcomeInformation.Add(hEvtOut)
      hEvt.LinkToObject(pObj)
      hEvt.LinkToAgent(pUserAgent)
      hEvt.LinkToAgent(pSoftAgent)

      pContainer.Events.Add(hEvt)

    End Try

    Return handle

  End Function

  ''' <summary>
  ''' Given the human-readable identifier return the corresponding medusa identifier, 
  ''' if the human-readable id does not exist in the database then a new medusa id is generated for it
  ''' and added to the database and it is returned
  ''' </summary>
  ''' <param name="humanId"></param>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Shared Function GetMedusaIdentifier(humanId As String) As String
    Dim id As String = ""
    Using db As New MedusaDataContext(MedusaAppSettings.Settings.ConnectionString)

      id = (From i In db.IdentifierLookups Where i.HumanIdentifier = humanId Select i.MedusaIdentifier).SingleOrDefault

    End Using

    If String.IsNullOrWhiteSpace(id) Then
      id = IdManager.AddIdentifier(humanId, IdManager.GenerateLocalIdentifier)
    End If

    Return id
  End Function

  Public Shared Function GetLocalPremisIdentifier(humanId As String) As PremisIdentifier
    Return New PremisIdentifier("LOCAL", IdManager.GetMedusaIdentifier(humanId))
  End Function


  ''' <summary>
  ''' Given the medusa identifier return the  human-readable id
  ''' </summary>
  ''' <param name="medusaId"></param>
  ''' <returns>The human-readable identifier or empty string if the medusa identifier is not found</returns>
  ''' <remarks></remarks>
  Public Shared Function GetHumanIdentifier(medusaId As String) As String
    Dim id As String = ""
    Using db As New MedusaDataContext(MedusaAppSettings.Settings.ConnectionString)

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
    Using db As New MedusaDataContext(MedusaAppSettings.Settings.ConnectionString)

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
  Public Shared Function ParseLocalIdentifier(handle As String) As String
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

