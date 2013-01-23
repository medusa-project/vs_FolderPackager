Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Xml
Imports Uiuc.Library.NetFedora



'''<summary>
'''This is a test class for FedoraObjectTest and is intended
'''to contain all FedoraObjectTest Unit Tests
''' 
''' Some tests assume that there is an object in the test repository with a pid of "test:1234".
''' Also, that there are objest with pids test:a ... test:h with labels starting with "edible "
''' and ending with "apple", "banana", "doughnut", "eggplant", "fig", "grapefruit", "honeydew"
'''</summary>
<TestClass()> _
Public Class FedoraObjectTest


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
  '''A test for RepositoryName
  '''</summary>
  <TestMethod()> _
  Public Sub RepositoryNameTest()
    Dim repo As New FedoraRepository
    Dim expected As String = "Fedora Repository" 'this is the default for a newly installed repo
    Dim actual As String
    actual = repo.Information.repositoryName
    Assert.AreEqual(expected, actual)
  End Sub

  ''' <summary>
  ''' Test the object not found exception
  ''' </summary>
  ''' <remarks></remarks>
  <TestMethod()> _
  <ExpectedException(GetType(FedoraException))> _
  Public Sub FedoraObjectNotFoundTest()
    Dim pid As String = "notfound:1234"
    Dim repo As New FedoraRepository
    Dim obj As FedoraObject = repo.GetObject(pid)
    Dim fxml As FoxmlDigitalObject = obj.Foxml
  End Sub

  ''' <summary>
  ''' Test the creation and basic functionality of the FedoraObject class
  ''' </summary>
  ''' <remarks></remarks>
  <TestMethod()> _
  Public Sub FedoraObjectTest()
    Dim pid As String = "test:1234"
    Dim repo As New FedoraRepository
    Dim obj As FedoraObject = repo.GetObject(pid)
    Dim fxml As FoxmlDigitalObject = obj.Foxml

    Assert.AreEqual(pid, fxml.Pid)

  End Sub

  <TestMethod()> _
  Public Sub RetrieveDatastreamTest()
    Dim pid As String = "test:1234"
    Dim id As String = "DC"
    Dim repo As New FedoraRepository
    Dim obj As FedoraObject = repo.GetObject(pid)

    Dim ds = obj.GetDatastream(id)

    Assert.AreEqual(id, ds.Id)
  End Sub

  <TestMethod()> _
  Public Sub RetrieveLatestDatastreamVersionTest()
    Dim pid As String = "test:1234"
    Dim id As String = "DC"
    Dim repo As New FedoraRepository
    Dim obj As FedoraObject = repo.GetObject(pid)
    Dim ds As FedoraDatastream = obj.GetDatastream(id)
    Dim dsv As FoxmlDatastreamVersion = ds.Foxml.GetLatestVersion()

    Dim dsvDt = dsv.Created.Value.AddMilliseconds(0.1)

    Assert.IsTrue(ds.Foxml.DatastreamVersions.All(Function(d) d.Created <= dsvDt))

  End Sub

  ''' <summary>
  ''' Test the search term string version of the FindObjects methods
  ''' </summary>
  ''' <remarks>Set the FedoraRepository.MaxSearchResults variable to 3, 4, 5,  8, 9,100</remarks>
  <TestMethod()> _
  Public Sub FindObjectsTest()
    FedoraRepository.MaxSearchResults = 1000
    Dim repo As New FedoraRepository
    Dim sr As FedoraSearchResults = repo.FindObjects("edible*")

    Dim expectedCount As Integer = 8

    Assert.IsTrue(sr.Count = expectedCount)
    Dim cnt = sr.Count 'doing this since the count is cached after the first one, and want to make sure the cache is working
    Assert.IsTrue(cnt = expectedCount)

    Dim i As Integer = AscW("a")
    'assumes results are returned in pid order
    For Each obj As FedoraObject In sr
      Assert.IsTrue(obj.Pid = String.Format("test:{0}", ChrW(i)))
      i = i + 1
    Next

    Assert.IsTrue(i - AscW("a") = cnt)

  End Sub

  ''' <summary>
  ''' Test the search conditions version of the FindObjects method
  ''' </summary>
  ''' <remarks></remarks>
  <TestMethod()> _
  Public Sub FindObjects2Test()
    FedoraRepository.MaxSearchResults = 1000
    Dim pid As String = "test:1234"
    Dim repo As New FedoraRepository
    Dim cond(0) As ApiA.Condition
    cond(0) = New ApiA.Condition()
    cond(0).property = "pid"
    cond(0).operator = ApiA.ComparisonOperator.eq
    cond(0).value = pid
    Dim sr As FedoraSearchResults = repo.FindObjects(cond)

    Dim expectedCount As Integer = 1

    Assert.IsTrue(sr.Count = expectedCount)
    Dim cnt = sr.Count 'doing this since the count is cached after the first one, and want to make sure the cache is working
    Assert.IsTrue(cnt = expectedCount)

    Assert.IsTrue(sr.First.Pid = pid)

    sr = repo.FindObjects("pid", ApiA.ComparisonOperator.eq, pid)

    Assert.IsTrue(sr.Count = expectedCount)
    cnt = sr.Count 'doing this since the count is cached after the first one, and want to make sure the cache is working
    Assert.IsTrue(cnt = expectedCount)

    Assert.IsTrue(sr.First.Pid = pid)

  End Sub

  <TestMethod()> _
  Public Sub IngestTest()
    'create a simple foxml file to ingest
    Dim newPid As String = "test:9876"
    Dim lbl As String = "test ingest"
    Dim myns As String = "http://medusa.library.illinois.edu/relations-external#"
    Dim object2 As String = "test:collection_1234"
    Dim predicate2 As String = "collection_is_member_of"

    Dim foxmlObj As New FoxmlDigitalObject(newPid)
    foxmlObj.Label = lbl

    Dim foxmlDc As New FoxmlDatastream("DC", ControlGroups.X)
    Dim dc As New OaiDc
    dc.Identifiers.Add(newPid)
    dc.Titles.Add(lbl)
    Dim dcXml As XmlDocument = dc.Xml


    Dim foxmlDcV1 As New FoxmlDatastreamVersion("text/xml", dcXml)

    foxmlDcV1.ParentDatastream = foxmlDc
    foxmlDc.DatastreamVersions.Add(foxmlDcV1)

    foxmlDc.ParentDigitalObject = foxmlObj
    foxmlObj.DataStreams.Add(foxmlDc)

    Dim foxmlRelsx As New FoxmlDatastream("RELS-EXT", ControlGroups.X)
    Dim relsx As New RelsExt(newPid)
    relsx.AddRelationship("medusa", predicate2, myns, object2)
    Dim relsxXml As XmlDocument = relsx.Xml

    Dim foxmlRelsxV1 As New FoxmlDatastreamVersion("text/xml", relsxXml)

    foxmlRelsxV1.ParentDatastream = foxmlRelsx
    foxmlRelsx.DatastreamVersions.Add(foxmlRelsxV1)

    foxmlRelsx.ParentDigitalObject = foxmlObj
    foxmlObj.DataStreams.Add(foxmlRelsx)

    Dim repo As New FedoraRepository
    Dim obj As FedoraObject = repo.IngestObject(foxmlObj, "This is a test")

    Assert.IsTrue(newPid = obj.Pid)

    'clean up after the test
    Dim d As Date = repo.PurgeObject(newPid, "This is a test")

    Assert.AreEqual(d.Day, Now.Day)
    Assert.AreEqual(d.Month, Now.Month)
    Assert.AreEqual(d.Year, Now.Year)

  End Sub

  <TestMethod()> _
  Public Sub RelsExtTest()
    Dim subject As String = "test:1234"
    Dim object1 As String = "test:5678"
    Dim predicate1 As String = "metadata_root"
    Dim object2 As String = "test:collection_1234"
    Dim predicate2 As String = "collection_is_member_of"

    Dim rels As New RelsExt(subject)
    Dim myns As String = "http://medusa.library.illinois.edu/relations-external#"
    Dim rdfns As String = "http://www.w3.org/1999/02/22-rdf-syntax-ns#"

    rels.AddRelationship("medusa", predicate1, myns, object1) 'one with prefix
    rels.AddRelationship(predicate2, myns, object2) 'one w/o prefix

    Dim xml As XmlDocument = rels.Xml

    Dim xmlns As New XmlNamespaceManager(xml.NameTable)
    xmlns.AddNamespace("rdf", rdfns)
    xmlns.AddNamespace("medusa", myns)

    Assert.IsTrue(xml.DocumentElement.NamespaceURI = rdfns)
    Assert.IsTrue(xml.DocumentElement.LocalName = "RDF")

    Assert.IsTrue(xml.DocumentElement.Item("Description", rdfns).GetAttribute("about", rdfns) = FedoraObject.FedoraUriPrefix & subject)

    Dim x As XmlElement = xml.SelectSingleNode(String.Format("/rdf:RDF/rdf:Description[@rdf:about='{0}{1}']/medusa:{2}", FedoraObject.FedoraUriPrefix, subject, predicate1), xmlns)
    Assert.IsNotNull(x)
    Assert.IsTrue(x.GetAttribute("resource", rdfns) = FedoraObject.FedoraUriPrefix & object1)

    x = xml.SelectSingleNode(String.Format("/rdf:RDF/rdf:Description[@rdf:about='{0}{1}']/medusa:{2}", FedoraObject.FedoraUriPrefix, subject, predicate2), xmlns)
    Assert.IsNotNull(x)
    Assert.IsTrue(x.GetAttribute("resource", rdfns) = FedoraObject.FedoraUriPrefix & object2)

    Dim xmlStr As String = <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"
                             xmlns:medusa="http://medusa.library.illinois.edu/relations-external#">
                             <rdf:Description rdf:about="info:fedora/test:1234">
                               <medusa:metadata_root rdf:resource="info:fedora/test:5678"/>
                               <medusa:collection_is_member_of rdf:resource="info:fedora/test:collection_1234"/>
                             </rdf:Description>
                           </rdf:RDF>.ToString

    Dim xmlDoc As New XmlDocument
    xmlDoc.LoadXml(xmlStr)

    Dim newRelsExt As New RelsExt(xmlDoc.DocumentElement)

    Assert.IsTrue(newRelsExt.Pid = subject)
    Assert.IsTrue(newRelsExt.Subject = New Uri(FedoraObject.FedoraUriPrefix & subject))

    Dim d As List(Of String) = newRelsExt.GetRelatedObjectPids(predicate1, myns)
    Assert.IsTrue(d.Count = 1)
    Assert.IsTrue(d.Item(0) = object1)

    d = newRelsExt.GetRelatedObjectPids(New Uri(String.Format("{1}{0}", predicate1, myns)))
    Assert.IsTrue(d.Count = 1)
    Assert.IsTrue(d.Item(0) = object1)

    d = newRelsExt.GetRelatedObjectPids(predicate2, myns)
    Assert.IsTrue(d.Count = 1)
    Assert.IsTrue(d.Item(0) = object2)

    d = newRelsExt.GetRelatedObjectPids(New Uri(String.Format("{1}{0}", predicate2, myns)))
    Assert.IsTrue(d.Count = 1)
    Assert.IsTrue(d.Item(0) = object2)

  End Sub

End Class
