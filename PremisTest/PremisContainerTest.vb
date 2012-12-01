Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports Uiuc.Library.Premis



'''<summary>
'''This is a test class for PremisContainerTest and is intended
'''to contain all PremisContainerTest Unit Tests
'''</summary>
<TestClass()> _
Public Class PremisContainerTest


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
  '''A test for ContentDM PremisContainer Constructor
  '''</summary>
  <TestMethod()> _
  Public Sub ContentDMPremisContainerFileConstructorTest()

    'collection object
    Dim filename As String = "\\libgrsurya\Medusa_Staging\ContentDM\AfricanMaps\collection\premis_object_MEDUSA_f7236f68-5c00-4837-a7a0-af387823de45-4.xml"
    Dim target As PremisContainer = New PremisContainer(filename)
    Assert.IsTrue(target.Objects.Count = 2)
    Assert.IsTrue(target.Events.Count = 2)
    Assert.IsTrue(target.Agents.Count = 1)
    Assert.IsTrue(target.Rights.Count = 1)

    'object representation
    filename = "\\libgrsurya\Medusa_Staging\ContentDM\AfricanMaps\0\1\MEDUSA_ab85d66d-601b-4d9d-89fd-2555511360ed-5\premis_MEDUSA_ab85d66d-601b-4d9d-89fd-2555511360ed-5.xml"
    target = New PremisContainer(filename)
    Assert.IsTrue(target.Objects.Count = 4)
    Assert.IsTrue(target.Events.Count = 4)
    Assert.IsTrue(target.Agents.Count = 1)
    Assert.IsTrue(target.Rights.Count = 1)

    'object file
    filename = "\\libgrsurya\Medusa_Staging\ContentDM\AfricanMaps\0\1\MEDUSA_ab85d66d-601b-4d9d-89fd-2555511360ed-5\00011\premis_MEDUSA_ab85d66d-601b-4d9d-89fd-2555511360ed-5.00011.xml"
    target = New PremisContainer(filename)
    Assert.IsTrue(target.Objects.Count = 1)
    Assert.IsTrue(target.Events.Count = 2)
    Assert.IsTrue(target.Agents.Count = 1)
    Assert.IsTrue(target.Rights.Count = 0)
  End Sub

  '''<summary>
  '''A test for FolderPackager PremisContainer Constructor
  '''</summary>
  <TestMethod()> _
  Public Sub FolderPremisContainerFileConstructorTest()

    'collection object
    Dim filename As String = "\\libgrsurya\Medusa_Staging\UnicaPackageTest\data\MEDUSA_b4740450-53e3-4946-beef-b75e8a54cce3-8\premis_MEDUSA_b4740450-53e3-4946-beef-b75e8a54cce3-8.xml"
    Dim target As PremisContainer = New PremisContainer(filename)
    Assert.IsTrue(target.Objects.Count = 1)
    Assert.IsTrue(target.Events.Count = 2)
    Assert.IsTrue(target.Agents.Count = 2)
    Assert.IsTrue(target.Rights.Count = 1)

  End Sub

End Class
