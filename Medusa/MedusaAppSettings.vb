Imports System.Configuration
Imports System.IO
Imports System.Collections.Specialized
Imports System.Reflection

''' <summary>
''' A singleton class provides access to the Medusa properties from the various configuration files, such as app.config
''' </summary>
''' <remarks></remarks>
Public Class MedusaAppSettings

  'define different rights
  Public Const COPYRIGHT As String = "COPYRIGHT"
  Public Const LICENSE As String = "LICENSE"
  Public Const STATUTE As String = "STATUTE"
  Public Const OTHER As String = "OTHER"

  Private Shared _thisInst As MedusaAppSettings

  'TODO: Would probably be good to rewrite this class to use a dictionary of values instead of a bunch of private variables
  Private _dict As Dictionary(Of String, String)

  ' *** Here is the template for getting or setting appsettings string properties; change to public and chaneg the name to match a appSettings key
  Private Property SomeSettingKey As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Private Function GetValue(key As String, defaultValue As String) As String
    If Not _dict.ContainsKey(key) Then
      If ConfigurationManager.AppSettings.AllKeys.Contains(key) Then
        _dict.Add(key, ConfigurationManager.AppSettings.Item(key))
      Else
        Dim confSet As NameValueCollection = ConfigurationManager.GetSection("secretAppSettings")
        If confSet IsNot Nothing AndAlso confSet.AllKeys.Contains(key) Then
          _dict.Add(key, confSet.Item(key))
        Else
          _dict.Add(key, defaultValue)
        End If
      End If
    End If
    Return _dict.Item(key)
  End Function

  Private Function GetValue(key As String) As String
    Return GetValue(key, Nothing)
  End Function

  Private Sub SetValue(key As String, value As String)
    _dict.Item(key) = value
  End Sub

  Protected Sub New()
    'this is a singleton
    _dict = New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase)
  End Sub

  ''' <summary>
  ''' Reset all values to their orignal app.config settings
  ''' </summary>
  ''' <remarks></remarks>
  Public Sub Reset()
    _dict.Clear()
  End Sub

  Public Shared Function Settings() As MedusaAppSettings
    If _thisInst Is Nothing Then
      _thisInst = New MedusaAppSettings
    End If
    Return _thisInst
  End Function


  Private _handleManagerConnectionString As String = Nothing
  Public Property HandleManagerConnectionString As String
    Get
      If _handleManagerConnectionString Is Nothing Then
        _handleManagerConnectionString = ConfigurationManager.ConnectionStrings.Item("HandleManagerConnectionString").ConnectionString
      End If
      Return _handleManagerConnectionString
    End Get
    Set(value As String)
      _handleManagerConnectionString = value
    End Set
  End Property

  Private _medusaConnectionString As String = Nothing
  Public Property MedusaConnectionString As String
    Get
      If _medusaConnectionString Is Nothing Then
        _medusaConnectionString = ConfigurationManager.ConnectionStrings.Item("MedusaConnectionString").ConnectionString
      End If
      Return _medusaConnectionString
    End Get
    Set(value As String)
      _medusaConnectionString = value
    End Set
  End Property

  Public Property PageFoldersRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property


  Public Property ImageTechnicalMetadataRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property Md5ManifestRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property OcrTextRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property HighQualityPdfRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property OptimizedPdfRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property TeiXmlRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property AgentsFolder As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property CollectionsFolder As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property HandlePrefix As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property HandleProject As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property HandleLocalIdSeparator As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property HandleServiceURL As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property HandleResourceType As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property HandleResolverBaseURL As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Private _handleGeneration As HandleGenerationType? = Nothing
  Public Property HandleGeneration As HandleGenerationType
    Get
      If _handleGeneration Is Nothing Then
        _handleGeneration = [Enum].Parse(GetType(HandleGenerationType), ConfigurationManager.AppSettings.Item("HandleGeneration"), True)
      End If
      Return _handleGeneration
    End Get
    Set(value As HandleGenerationType)
      _handleGeneration = value
    End Set
  End Property

  Public Property GetCollectionModsUrl As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property GetCollectionJsonUrl As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property GetMarcUrl As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property ChecksumAlgorithm As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Private _ignoreBadCert As Boolean? = Nothing
  Public Property IgnoreBadCert As Boolean
    Get
      If _ignoreBadCert Is Nothing Then
        If String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("IgnoreBadCert")) Then
          _ignoreBadCert = False
        Else
          _ignoreBadCert = Boolean.Parse(ConfigurationManager.AppSettings.Item("IgnoreBadCert"))
        End If
      End If
      Return _ignoreBadCert
    End Get
    Set(value As Boolean)
      _ignoreBadCert = value
    End Set
  End Property

  Private _saveFilesAs As SaveFileAsType? = Nothing
  Public Property SaveFilesAs As SaveFileAsType
    Get
      If _saveFilesAs Is Nothing Then
        _saveFilesAs = [Enum].Parse(GetType(SaveFileAsType), ConfigurationManager.AppSettings.Item("SaveFilesAs"), True)
      End If
      Return _saveFilesAs
    End Get
    Set(value As SaveFileAsType)
      _saveFilesAs = value
    End Set
  End Property

  Private _doFits As Boolean? = Nothing
  Public Property DoFits As Boolean
    Get
      If _doFits Is Nothing Then
        If String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings.Item("DoFits")) Then
          _doFits = True
        Else
          _doFits = Boolean.Parse(ConfigurationManager.AppSettings.Item("DoFits"))
        End If
      End If
      Return _doFits
    End Get
    Set(value As Boolean)
      _doFits = value
    End Set
  End Property

  Public Property FitsHome As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property FitsScript As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public ReadOnly Property FitsScriptPath As String
    Get
      Return Path.Combine(FitsHome, FitsScript)
    End Get
  End Property

  Private _objectAlreadyExists As ObjectAlreadyExistsType? = Nothing
  Public Property ObjectAlreadyExists As ObjectAlreadyExistsType
    Get
      If _objectAlreadyExists Is Nothing Then
        _objectAlreadyExists = [Enum].Parse(GetType(ObjectAlreadyExistsType), ConfigurationManager.AppSettings.Item("ObjectAlreadyExists"), True)
      End If
      Return _objectAlreadyExists
    End Get
    Set(value As ObjectAlreadyExistsType)
      _objectAlreadyExists = value
    End Set
  End Property

  Public Property WorkingFolder As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property LogFile As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4), "FolderPackager.log")
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public ReadOnly Property LogFilePath As String
    Get
      Return Path.Combine(WorkingFolder, LogFile)
    End Get
  End Property


  Public Property SourceFolder As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property CollectionId As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property CollectionHandle As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property CollectionName As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property CollectionURL As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property CollectionDescriptionPath As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Private _objectFolderLevel As Integer? = Nothing
  Public Property ObjectFolderLevel As Integer
    Get
      If _objectFolderLevel Is Nothing Then
        _objectFolderLevel = Integer.Parse(ConfigurationManager.AppSettings.Item("ObjectFolderLevel"))
      End If
      Return _objectFolderLevel
    End Get
    Set(value As Integer)
      _objectFolderLevel = value
    End Set
  End Property

  Public Property PremisDisseminationRightsBasis As String
    Get
      Dim ret As String = GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
      If MedusaAppSettings.ValidRightsBasis.Contains(ret) Then
        Return ret
      Else
        Throw New MedusaException(String.Format("The value '{0}' is not valid for Rights Basis.", ret))
      End If
    End Get
    Set(value As String)
      If MedusaAppSettings.ValidRightsBasis.Contains(value) Then
        SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
      Else
        Throw New MedusaException(String.Format("The value '{0}' is not valid for Rights Basis.", value))
      End If
    End Set
  End Property

  Public Property PremisDisseminationStatuteJurisdiction As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property PremisDisseminationStatuteCitation As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property


  Public Property PremisDisseminationCopyrightJurisdiction As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property PremisDisseminationCopyrightStatus As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property PremisDisseminationCopyrightNote As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property PremisDisseminationOtherRightsBasis As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property PremisDisseminationRights As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property PremisDisseminationRightsRestrictions As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property MetadataMarcRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property MetadataDcRdfRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property MetadataSpreadsheetRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property OmitFoldersRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property OmitFilesRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property DerivativeContentFileRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property OriginalContentFileRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property SignificantFileIdentiferRegex As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Private _packageMode As PackageModeType? = Nothing
  Public Property PackageMode As PackageModeType
    Get
      If _packageMode Is Nothing Then
        _packageMode = [Enum].Parse(GetType(PackageModeType), ConfigurationManager.AppSettings.Item("PackageMode"), True)
      End If
      Return _packageMode
    End Get
    Set(value As PackageModeType)
      _packageMode = value
    End Set
  End Property


  Public Property HandlePassword As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property FedoraAccount As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property FedoraPassword As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property


  Public Property MarcToModsXslt As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property DcRdfToModsXslt As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property RestartAtPath As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Shared ReadOnly Property ValidRightsBasis As String()
    Get
      Return {MedusaAppSettings.COPYRIGHT, MedusaAppSettings.LICENSE, MedusaAppSettings.OTHER, MedusaAppSettings.STATUTE}
    End Get
  End Property

  Public Property PreservationLevel As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

  Public Property PreservationLevelRationale As String
    Get
      Return GetValue(MethodBase.GetCurrentMethod.Name.Substring(4))
    End Get
    Set(value As String)
      SetValue(MethodBase.GetCurrentMethod.Name.Substring(4), value)
    End Set
  End Property

End Class

Public Enum HandleGenerationType
  ROOT_OBJECT_AND_FILES
  ROOT_OBJECT_ONLY
  FILES_ONLY
  NONE
End Enum

Public Enum SaveFileAsType
  ONE
  MULTIPLE
  REPRESENTATIONS
  MEDUSA
  MEDUSA_MULTIPLE
End Enum

Public Enum PackageModeType
  MOVE
  COPY
  HARDLINK
End Enum

Public Enum ObjectAlreadyExistsType
  OVERWRITE
  SKIP
  THROW_ERROR
End Enum

