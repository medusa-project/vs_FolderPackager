Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports Uiuc.Library.Medusa
Imports System.IO


'''<summary>
'''This is a test class for MedusaAppSettingsTest and is intended
'''to contain all MedusaAppSettingsTest Unit Tests
'''</summary>
<TestClass()> _
Public Class MedusaAppSettingsTest


  Private testContextInstance As TestContext

  '''<summary>
  '''Gets or sets the test context which provides
  '''information about and functionality for the current test run.
  '''</summary>
  Public Property TestContext() As TestContext
    Get
      Return testContextInstance
    End Get
    Set(value As TestContext)
      testContextInstance = Value
    End Set
  End Property

#Region "Additional test attributes"
  '
  'You can use the following additional attributes as you write your tests:
  '
  'Use ClassInitialize to run code before running the first test in the class
  '<ClassInitialize()>  _
  'Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
  'End Sub
  '
  'Use ClassCleanup to run code after all tests in a class have run
  '<ClassCleanup()>  _
  'Public Shared Sub MyClassCleanup()
  'End Sub
  '
  'Use TestInitialize to run code before running each test
  '<TestInitialize()>  _
  'Public Sub MyTestInitialize()
  'End Sub
  '
  'Use TestCleanup to run code after each test has run
  '<TestCleanup()>  _
  'Public Sub MyTestCleanup()
  'End Sub
  '
#End Region


  '''<summary>
  '''A test for Settings
  '''</summary>
  <TestMethod()> _
  Public Sub SettingsTest()
    Dim actual As MedusaAppSettings
    actual = MedusaAppSettings.Settings
    Assert.AreEqual("CollectionId", actual.CollectionId)
  End Sub

  '''<summary>
  '''A test for AgentsFolder
  '''</summary>
  <TestMethod()> _
  Public Sub AgentsFolderTest()
    Dim expected As String = "AgentsFolder"
    Dim actual As String = MedusaAppSettings.Settings.AgentsFolder
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.AgentsFolder = expected
    actual = MedusaAppSettings.Settings.AgentsFolder
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for ChecksumAlgorithm
  '''</summary>
  <TestMethod()> _
  Public Sub ChecksumAlgorithmTest()
    Dim expected As String = "ChecksumAlgorithm"
    Dim actual As String = MedusaAppSettings.Settings.ChecksumAlgorithm
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.ChecksumAlgorithm = expected
    actual = MedusaAppSettings.Settings.ChecksumAlgorithm
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for FedoraChecksumAlgorithm
  '''</summary>
  <TestMethod()> _
  Public Sub FedoraChecksumAlgorithm()
    Dim expected As String = "FedoraChecksumAlgorithm"
    Dim actual As String = MedusaAppSettings.Settings.FedoraChecksumAlgorithm
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.FedoraChecksumAlgorithm = expected
    actual = MedusaAppSettings.Settings.FedoraChecksumAlgorithm
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for CollectionDescriptionPath
  '''</summary>
  <TestMethod()> _
  Public Sub CollectionDescriptionPathTest()
    Dim expected As String = "CollectionDescriptionPath"
    Dim actual As String = MedusaAppSettings.Settings.CollectionDescriptionPath
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.CollectionDescriptionPath = expected
    actual = MedusaAppSettings.Settings.CollectionDescriptionPath
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for CollectionHandle
  '''</summary>
  <TestMethod()> _
  Public Sub CollectionHandleTest()
    Dim expected As String = "CollectionHandle"
    Dim actual As String = MedusaAppSettings.Settings.CollectionHandle
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.CollectionHandle = expected
    actual = MedusaAppSettings.Settings.CollectionHandle
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for CollectionId
  '''</summary>
  <TestMethod()> _
  Public Sub CollectionIdTest()
    Dim expected As String = "CollectionId"
    Dim actual As String = MedusaAppSettings.Settings.CollectionId
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.CollectionId = expected
    actual = MedusaAppSettings.Settings.CollectionId
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for CollectionName
  '''</summary>
  <TestMethod()> _
  Public Sub CollectionNameTest()
    Dim expected As String = "CollectionName"
    Dim actual As String = MedusaAppSettings.Settings.CollectionName
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.CollectionName = expected
    actual = MedusaAppSettings.Settings.CollectionName
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for CollectionURL
  '''</summary>
  <TestMethod()> _
  Public Sub CollectionURLTest()
    Dim expected As String = "CollectionURL"
    Dim actual As String = MedusaAppSettings.Settings.CollectionURL
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.CollectionURL = expected
    actual = MedusaAppSettings.Settings.CollectionURL
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for CollectionsFolder
  '''</summary>
  <TestMethod()> _
  Public Sub CollectionsFolderTest()
    Dim expected As String = "CollectionsFolder"
    Dim actual As String = MedusaAppSettings.Settings.CollectionsFolder
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.CollectionsFolder = expected
    actual = MedusaAppSettings.Settings.CollectionsFolder
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for MedusaConnectionString
  '''</summary>
  <TestMethod()> _
  Public Sub MedusaConnectionStringTest()
    Dim expected As String = "MedusaConnectionString"
    Dim actual As String = MedusaAppSettings.Settings.MedusaConnectionString
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.MedusaConnectionString = expected
    actual = MedusaAppSettings.Settings.MedusaConnectionString
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for HandleManagerConnectionString
  '''</summary>
  <TestMethod()> _
  Public Sub HandleManagerConnectionStringTest()
    Dim expected As String = "HandleManagerConnectionString"
    Dim actual As String = MedusaAppSettings.Settings.HandleManagerConnectionString
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.HandleManagerConnectionString = expected
    actual = MedusaAppSettings.Settings.HandleManagerConnectionString
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for DcRdfToModsXslt
  '''</summary>
  <TestMethod()> _
  Public Sub DcRdfToModsXsltTest()
    Dim expected As String = "DcRdfToModsXslt"
    Dim actual As String = MedusaAppSettings.Settings.DcRdfToModsXslt
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.DcRdfToModsXslt = expected
    actual = MedusaAppSettings.Settings.DcRdfToModsXslt
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for DerivativeContentFileRegex
  '''</summary>
  <TestMethod()> _
  Public Sub DerivativeContentFileRegexTest()
    Dim expected As String = "DerivativeContentFileRegex"
    Dim actual As String = MedusaAppSettings.Settings.DerivativeContentFileRegex
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.DerivativeContentFileRegex = expected
    actual = MedusaAppSettings.Settings.DerivativeContentFileRegex
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for DoFits
  '''</summary>
  <TestMethod()> _
  Public Sub DoFitsTest()
    Dim expected As Boolean = False
    Dim actual As Boolean = MedusaAppSettings.Settings.DoFits
    Assert.AreEqual(expected, actual)

    expected = Not expected
    MedusaAppSettings.Settings.DoFits = expected
    actual = MedusaAppSettings.Settings.DoFits
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for FedoraAccount
  '''</summary>
  <TestMethod()> _
  Public Sub FedoraAccountTest()
    Dim expected As String = "FedoraAccount"
    Dim actual As String = MedusaAppSettings.Settings.FedoraAccount
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.FedoraAccount = expected
    actual = MedusaAppSettings.Settings.FedoraAccount
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for FedoraPassword
  '''</summary>
  <TestMethod()> _
  Public Sub FedoraPasswordTest()
    Dim expected As String = "FedoraPassword"
    Dim actual As String = MedusaAppSettings.Settings.FedoraPassword
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.FedoraPassword = expected
    actual = MedusaAppSettings.Settings.FedoraPassword
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for FitsHome
  '''</summary>
  <TestMethod()> _
  Public Sub FitsHomeTest()
    Dim expected As String = "FitsHome"
    Dim actual As String = MedusaAppSettings.Settings.FitsHome
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.FitsHome = expected
    actual = MedusaAppSettings.Settings.FitsHome
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for FitsScript
  '''</summary>
  <TestMethod()> _
  Public Sub FitsScriptTest()
    Dim expected As String = "FitsScript"
    Dim actual As String = MedusaAppSettings.Settings.FitsScript
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.FitsScript = expected
    actual = MedusaAppSettings.Settings.FitsScript
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for FitsScriptPath
  '''</summary>
  <TestMethod()> _
  Public Sub FitsScriptPathTest()
    MedusaAppSettings.Settings.Reset()
    Dim expected As String = Path.Combine("FitsHome", "FitsScript")
    Dim actual As String = MedusaAppSettings.Settings.FitsScriptPath
    Assert.AreEqual(expected, actual)

    MedusaAppSettings.Settings.FitsHome = expected
    MedusaAppSettings.Settings.FitsScript = expected
    expected = Path.Combine(expected, expected)
    actual = MedusaAppSettings.Settings.FitsScriptPath
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for GetCollectionJsonUrl
  '''</summary>
  <TestMethod()> _
  Public Sub GetCollectionJsonUrlTest()
    Dim expected As String = "GetCollectionJsonUrl"
    Dim actual As String = MedusaAppSettings.Settings.GetCollectionJsonUrl
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.GetCollectionJsonUrl = expected
    actual = MedusaAppSettings.Settings.GetCollectionJsonUrl
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for GetCollectionModsUrl
  '''</summary>
  <TestMethod()> _
  Public Sub GetCollectionModsUrlTest()
    Dim expected As String = "GetCollectionModsUrl"
    Dim actual As String = MedusaAppSettings.Settings.GetCollectionModsUrl
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.GetCollectionModsUrl = expected
    actual = MedusaAppSettings.Settings.GetCollectionModsUrl
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for GetMarcUrl
  '''</summary>
  <TestMethod()> _
  Public Sub GetMarcUrlTest()
    Dim expected As String = "GetMarcUrl"
    Dim actual As String = MedusaAppSettings.Settings.GetMarcUrl
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.GetMarcUrl = expected
    actual = MedusaAppSettings.Settings.GetMarcUrl
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for HandleGeneration
  '''</summary>
  <TestMethod()> _
  Public Sub HandleGenerationTest()
    Dim expected As HandleGenerationType = HandleGenerationType.ROOT_OBJECT_ONLY
    Dim actual As HandleGenerationType = MedusaAppSettings.Settings.HandleGeneration
    Assert.AreEqual(expected, actual)

    expected = HandleGenerationType.NONE
    MedusaAppSettings.Settings.HandleGeneration = expected
    actual = MedusaAppSettings.Settings.HandleGeneration
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for HandleLocalIdSeparator
  '''</summary>
  <TestMethod()> _
  Public Sub HandleLocalIdSeparatorTest()
    Dim expected As String = "HandleLocalIdSeparator"
    Dim actual As String = MedusaAppSettings.Settings.HandleLocalIdSeparator
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.HandleLocalIdSeparator = expected
    actual = MedusaAppSettings.Settings.HandleLocalIdSeparator
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for HandlePassword
  '''</summary>
  <TestMethod()> _
  Public Sub HandlePasswordTest()
    Dim expected As String = "HandlePassword"
    Dim actual As String = MedusaAppSettings.Settings.HandlePassword
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.HandlePassword = expected
    actual = MedusaAppSettings.Settings.HandlePassword
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for HandlePrefix
  '''</summary>
  <TestMethod()> _
  Public Sub HandlePrefixTest()
    Dim expected As String = "HandlePrefix"
    Dim actual As String = MedusaAppSettings.Settings.HandlePrefix
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.HandlePrefix = expected
    actual = MedusaAppSettings.Settings.HandlePrefix
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for HandleProject
  '''</summary>
  <TestMethod()> _
  Public Sub HandleProjectTest()
    Dim expected As String = "HandleProject"
    Dim actual As String = MedusaAppSettings.Settings.HandleProject
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.HandleProject = expected
    actual = MedusaAppSettings.Settings.HandleProject
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for HandleResolverBaseURL
  '''</summary>
  <TestMethod()> _
  Public Sub HandleResolverBaseURLTest()
    Dim expected As String = "HandleResolverBaseURL"
    Dim actual As String = MedusaAppSettings.Settings.HandleResolverBaseURL
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.HandleResolverBaseURL = expected
    actual = MedusaAppSettings.Settings.HandleResolverBaseURL
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for HandleResourceType
  '''</summary>
  <TestMethod()> _
  Public Sub HandleResourceTypeTest()
    Dim expected As String = "HandleResourceType"
    Dim actual As String = MedusaAppSettings.Settings.HandleResourceType
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.HandleResourceType = expected
    actual = MedusaAppSettings.Settings.HandleResourceType
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for HandleServiceURL
  '''</summary>
  <TestMethod()> _
  Public Sub HandleServiceURLTest()
    Dim expected As String = "HandleServiceURL"
    Dim actual As String = MedusaAppSettings.Settings.HandleServiceURL
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.HandleServiceURL = expected
    actual = MedusaAppSettings.Settings.HandleServiceURL
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for IgnoreBadCert
  '''</summary>
  <TestMethod()> _
  Public Sub IgnoreBadCertTest()
    Dim expected As Boolean = True
    Dim actual As Boolean = MedusaAppSettings.Settings.IgnoreBadCert
    Assert.AreEqual(expected, actual)

    expected = Not expected
    MedusaAppSettings.Settings.IgnoreBadCert = expected
    actual = MedusaAppSettings.Settings.IgnoreBadCert
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for LogFile
  '''</summary>
  <TestMethod()> _
  Public Sub LogFileTest()
    Dim expected As String = "FolderPackager.log"
    Dim actual As String = MedusaAppSettings.Settings.LogFile
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.LogFile = expected
    actual = MedusaAppSettings.Settings.LogFile
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for LogFilePath
  '''</summary>
  <TestMethod()> _
  Public Sub LogFilePathTest()
    MedusaAppSettings.Settings.Reset()
    Dim expected As String = Path.Combine("WorkingFolder", "FolderPackager.log")
    Dim actual As String = MedusaAppSettings.Settings.LogFilePath
    Assert.AreEqual(expected, actual)

    MedusaAppSettings.Settings.WorkingFolder = expected
    MedusaAppSettings.Settings.LogFile = expected
    expected = Path.Combine(expected, expected)
    actual = MedusaAppSettings.Settings.LogFilePath
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for MarcToModsXslt
  '''</summary>
  <TestMethod()> _
  Public Sub MarcToModsXsltTest()
    Dim expected As String = "MarcToModsXslt"
    Dim actual As String = MedusaAppSettings.Settings.MarcToModsXslt
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.MarcToModsXslt = expected
    actual = MedusaAppSettings.Settings.MarcToModsXslt
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for ModsToDcXslt
  '''</summary>
  <TestMethod()> _
  Public Sub ModsToDcXsltTest()
    Dim expected As String = "ModsToDcXslt"
    Dim actual As String = MedusaAppSettings.Settings.ModsToDcXslt
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.ModsToDcXslt = expected
    actual = MedusaAppSettings.Settings.ModsToDcXslt
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for MetadataDcRdfRegex
  '''</summary>
  <TestMethod()> _
  Public Sub MetadataDcRdfRegexTest()
    Dim expected As String = "MetadataDcRdfRegex"
    Dim actual As String = MedusaAppSettings.Settings.MetadataDcRdfRegex
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.MetadataDcRdfRegex = expected
    actual = MedusaAppSettings.Settings.MetadataDcRdfRegex
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for MetadataMarcRegex
  '''</summary>
  <TestMethod()> _
  Public Sub MetadataMarcRegexTest()
    Dim expected As String = "MetadataMarcRegex"
    Dim actual As String = MedusaAppSettings.Settings.MetadataMarcRegex
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.MetadataMarcRegex = expected
    actual = MedusaAppSettings.Settings.MetadataMarcRegex
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for MetadataSpreadsheetRegex
  '''</summary>
  <TestMethod()> _
  Public Sub MetadataSpreadsheetRegexTest()
    Dim expected As String = "MetadataSpreadsheetRegex"
    Dim actual As String = MedusaAppSettings.Settings.MetadataSpreadsheetRegex
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.MetadataSpreadsheetRegex = expected
    actual = MedusaAppSettings.Settings.MetadataSpreadsheetRegex
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for ObjectAlreadyExists
  '''</summary>
  <TestMethod()> _
  Public Sub ObjectAlreadyExistsTest()
    Dim expected As ObjectAlreadyExistsType = ObjectAlreadyExistsType.OVERWRITE
    Dim actual As ObjectAlreadyExistsType = MedusaAppSettings.Settings.ObjectAlreadyExists
    Assert.AreEqual(expected, actual)

    expected = ObjectAlreadyExistsType.THROW_ERROR
    MedusaAppSettings.Settings.ObjectAlreadyExists = expected
    actual = MedusaAppSettings.Settings.ObjectAlreadyExists
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for ObjectFolderLevel
  '''</summary>
  <TestMethod()> _
  Public Sub ObjectFolderLevelTest()
    Dim expected As Integer = 1
    Dim actual As Integer = MedusaAppSettings.Settings.ObjectFolderLevel
    Assert.AreEqual(expected, actual)

    expected = expected + 1
    MedusaAppSettings.Settings.ObjectFolderLevel = expected
    actual = MedusaAppSettings.Settings.ObjectFolderLevel
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for OmitFilesRegex
  '''</summary>
  <TestMethod()> _
  Public Sub OmitFilesRegexTest()
    Dim expected As String = "OmitFilesRegex"
    Dim actual As String = MedusaAppSettings.Settings.OmitFilesRegex
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.OmitFilesRegex = expected
    actual = MedusaAppSettings.Settings.OmitFilesRegex
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for OmitFoldersRegex
  '''</summary>
  <TestMethod()> _
  Public Sub OmitFoldersRegexTest()
    Dim expected As String = "OmitFoldersRegex"
    Dim actual As String = MedusaAppSettings.Settings.OmitFoldersRegex
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.OmitFoldersRegex = expected
    actual = MedusaAppSettings.Settings.OmitFoldersRegex
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for OriginalContentFileRegex
  '''</summary>
  <TestMethod()> _
  Public Sub OriginalContentFileRegexTest()
    Dim expected As String = "OriginalContentFileRegex"
    Dim actual As String = MedusaAppSettings.Settings.OriginalContentFileRegex
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.OriginalContentFileRegex = expected
    actual = MedusaAppSettings.Settings.OriginalContentFileRegex
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PackageMode
  '''</summary>
  <TestMethod()> _
  Public Sub PackageModeTest()
    Dim expected As PackageModeType = PackageModeType.HARDLINK
    Dim actual As PackageModeType = MedusaAppSettings.Settings.PackageMode
    Assert.AreEqual(expected, actual)

    expected = PackageModeType.COPY
    MedusaAppSettings.Settings.PackageMode = expected
    actual = MedusaAppSettings.Settings.PackageMode
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PreservationLevel
  '''</summary>
  <TestMethod()> _
  Public Sub PreservationLevelTest()
    Dim expected As String = "PreservationLevel"
    Dim actual As String = MedusaAppSettings.Settings.PreservationLevel
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PreservationLevel = expected
    actual = MedusaAppSettings.Settings.PreservationLevel
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PreservationLevelRationale
  '''</summary>
  <TestMethod()> _
  Public Sub PreservationLevelRationaleTest()
    Dim expected As String = "PreservationLevelRationale"
    Dim actual As String = MedusaAppSettings.Settings.PreservationLevelRationale
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PreservationLevelRationale = expected
    actual = MedusaAppSettings.Settings.PreservationLevelRationale
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PremisDisseminationCopyrightJurisdiction
  '''</summary>
  <TestMethod()> _
  Public Sub PremisDisseminationCopyrightJurisdictionTest()
    Dim expected As String = "United States"
    Dim actual As String = MedusaAppSettings.Settings.PremisDisseminationCopyrightJurisdiction
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PremisDisseminationCopyrightJurisdiction = expected
    actual = MedusaAppSettings.Settings.PremisDisseminationCopyrightJurisdiction
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PremisDisseminationCopyrightStatus
  '''</summary>
  <TestMethod()> _
  Public Sub PremisDisseminationCopyrightStatusTest()
    Dim expected As String = "PremisDisseminationCopyrightStatus"
    Dim actual As String = MedusaAppSettings.Settings.PremisDisseminationCopyrightStatus
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PremisDisseminationCopyrightStatus = expected
    actual = MedusaAppSettings.Settings.PremisDisseminationCopyrightStatus
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PremisDisseminationCopyrightNote
  '''</summary>
  <TestMethod()> _
  Public Sub PremisDisseminationCopyrightNoteTest()
    Dim expected As String = "PremisDisseminationCopyrightNote"
    Dim actual As String = MedusaAppSettings.Settings.PremisDisseminationCopyrightNote
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PremisDisseminationCopyrightNote = expected
    actual = MedusaAppSettings.Settings.PremisDisseminationCopyrightNote
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PremisDisseminationOtherRightsBasis
  '''</summary>
  <TestMethod()> _
  Public Sub PremisDisseminationOtherRightsBasisTest()
    Dim expected As String = "PremisDisseminationOtherRightsBasis"
    Dim actual As String = MedusaAppSettings.Settings.PremisDisseminationOtherRightsBasis
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PremisDisseminationOtherRightsBasis = expected
    actual = MedusaAppSettings.Settings.PremisDisseminationOtherRightsBasis
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PremisDisseminationRights
  '''</summary>
  <TestMethod()> _
  Public Sub PremisDisseminationRightsTest()
    Dim expected As String = "PremisDisseminationRights"
    Dim actual As String = MedusaAppSettings.Settings.PremisDisseminationRights
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PremisDisseminationRights = expected
    actual = MedusaAppSettings.Settings.PremisDisseminationRights
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PremisDisseminationRightsBasis
  '''</summary>
  <TestMethod()> _
  Public Sub PremisDisseminationRightsBasisTest()
    Dim expected As String = MedusaAppSettings.OTHER
    Dim actual As String = MedusaAppSettings.Settings.PremisDisseminationRightsBasis
    Assert.AreEqual(expected, actual)

    expected = MedusaAppSettings.COPYRIGHT
    MedusaAppSettings.Settings.PremisDisseminationRightsBasis = expected
    actual = MedusaAppSettings.Settings.PremisDisseminationRightsBasis
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PremisDisseminationRightsBasis
  '''</summary>
  <TestMethod()> _
  <ExpectedException(GetType(MedusaException))> _
  Public Sub PremisDisseminationRightsBasisExceptionTest()
    MedusaAppSettings.Settings.Reset()
    Dim expected As String = MedusaAppSettings.OTHER
    Dim actual As String = MedusaAppSettings.Settings.PremisDisseminationRightsBasis
    Assert.AreEqual(expected, actual)

    expected = "JUNK"
    MedusaAppSettings.Settings.PremisDisseminationRightsBasis = expected
    actual = MedusaAppSettings.Settings.PremisDisseminationRightsBasis
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PremisDisseminationRightsRestrictions
  '''</summary>
  <TestMethod()> _
  Public Sub PremisDisseminationRightsRestrictionsTest()
    Dim expected As String = "PremisDisseminationRightsRestrictions"
    Dim actual As String = MedusaAppSettings.Settings.PremisDisseminationRightsRestrictions
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PremisDisseminationRightsRestrictions = expected
    actual = MedusaAppSettings.Settings.PremisDisseminationRightsRestrictions
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PremisDisseminationStatuteCitation
  '''</summary>
  <TestMethod()> _
  Public Sub PremisDisseminationStatuteCitationTest()
    Dim expected As String = "PremisDisseminationStatuteCitation"
    Dim actual As String = MedusaAppSettings.Settings.PremisDisseminationStatuteCitation
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PremisDisseminationStatuteCitation = expected
    actual = MedusaAppSettings.Settings.PremisDisseminationStatuteCitation
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for PremisDisseminationStatuteJurisdiction
  '''</summary>
  <TestMethod()> _
  Public Sub PremisDisseminationStatuteJurisdictionTest()
    Dim expected As String = "Illinois"
    Dim actual As String = MedusaAppSettings.Settings.PremisDisseminationStatuteJurisdiction
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PremisDisseminationStatuteJurisdiction = expected
    actual = MedusaAppSettings.Settings.PremisDisseminationStatuteJurisdiction
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for RestartAtPath
  '''</summary>
  <TestMethod()> _
  Public Sub RestartAtPathTest()
    Dim expected As String = "RestartAtPath"
    Dim actual As String = MedusaAppSettings.Settings.RestartAtPath
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.RestartAtPath = expected
    actual = MedusaAppSettings.Settings.RestartAtPath
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for SaveFilesAs
  '''</summary>
  <TestMethod()> _
  Public Sub SaveFilesAsTest()
    Dim expected As SaveFileAsType = SaveFileAsType.MEDUSA_MULTIPLE
    Dim actual As SaveFileAsType = MedusaAppSettings.Settings.SaveFilesAs
    Assert.AreEqual(expected, actual)

    expected = SaveFileAsType.ONE
    MedusaAppSettings.Settings.SaveFilesAs = expected
    actual = MedusaAppSettings.Settings.SaveFilesAs
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for SignificantFileIdentiferRegex
  '''</summary>
  <TestMethod()> _
  Public Sub SignificantFileIdentiferRegexTest()
    Dim expected As String = "SignificantFileIdentiferRegex"
    Dim actual As String = MedusaAppSettings.Settings.SignificantFileIdentiferRegex
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.SignificantFileIdentiferRegex = expected
    actual = MedusaAppSettings.Settings.SignificantFileIdentiferRegex
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for SourceFolder
  '''</summary>
  <TestMethod()> _
  Public Sub PageFoldersRegexTest()
    Dim expected As String = "PageFoldersRegex"
    Dim actual As String = MedusaAppSettings.Settings.PageFoldersRegex
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.PageFoldersRegex = expected
    actual = MedusaAppSettings.Settings.PageFoldersRegex
    Assert.AreEqual(expected, actual)
  End Sub

  '''<summary>
  '''A test for SourceFolder
  '''</summary>
  <TestMethod()> _
  Public Sub SourceFolderTest()
    Dim expected As String = "SourceFolder"
    Dim actual As String = MedusaAppSettings.Settings.SourceFolder
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.SourceFolder = expected
    actual = MedusaAppSettings.Settings.SourceFolder
    Assert.AreEqual(expected, actual)
  End Sub


  '''<summary>
  '''A test for WorkingFolder
  '''</summary>
  <TestMethod()> _
  Public Sub WorkingFolderTest()
    Dim expected As String = "WorkingFolder"
    Dim actual As String = MedusaAppSettings.Settings.WorkingFolder
    Assert.AreEqual(expected, actual)

    expected = String.Format("{0}-{0}", expected)
    MedusaAppSettings.Settings.WorkingFolder = expected
    actual = MedusaAppSettings.Settings.WorkingFolder
    Assert.AreEqual(expected, actual)
  End Sub
End Class
