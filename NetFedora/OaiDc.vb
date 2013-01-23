Imports System.Xml

Public Class OaiDc
  Public Const OaiDcNamespace As String = "http://www.openarchives.org/OAI/2.0/oai_dc/"
  Public Const DcNamespace As String = "http://purl.org/dc/elements/1.1/"
  Public Const OaiDcSchemaLocation As String = "http://www.openarchives.org/OAI/2.0/OAI-PMH.xsd"

  Public Property Contributors As List(Of String)

  Public Property Coverages As List(Of String)

  Public Property Creators As List(Of String)

  Public Property Dates As List(Of String)

  Public Property Descriptions As List(Of String)

  Public Property Formats As List(Of String)

  Public Property Identifiers As List(Of String)

  Public Property Languages As List(Of String)

  Public Property Publishers As List(Of String)

  Public Property Relations As List(Of String)

  Public Property Rights As List(Of String)

  Public Property Sources As List(Of String)

  Public Property Subjects As List(Of String)

  Public Property Titles As List(Of String)

  Public Property Types As List(Of String)

  Public Sub New()
    Contributors = New List(Of String)
    Coverages = New List(Of String)
    Creators = New List(Of String)
    Dates = New List(Of String)
    Descriptions = New List(Of String)
    Formats = New List(Of String)
    Identifiers = New List(Of String)
    Languages = New List(Of String)
    Publishers = New List(Of String)
    Relations = New List(Of String)
    Rights = New List(Of String)
    Sources = New List(Of String)
    Subjects = New List(Of String)
    Titles = New List(Of String)
    Types = New List(Of String)
  End Sub

  Public Sub New(identifier As String, title As String)
    Me.New()
    Identifiers.Add(identifier)
    Titles.Add(title)
  End Sub

  Public Sub New(elem As XmlElement)
    Me.New()

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("dc", OaiDc.OaiDcNamespace)

    Dim nds As XmlNodeList

    nds = elem.SelectNodes("*//dc:contributor", xmlns)
    For Each nd As XmlElement In nds
      Me.Contributors.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:coverage", xmlns)
    For Each nd As XmlElement In nds
      Me.Coverages.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:creator", xmlns)
    For Each nd As XmlElement In nds
      Me.Creators.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:description", xmlns)
    For Each nd As XmlElement In nds
      Me.Descriptions.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:format", xmlns)
    For Each nd As XmlElement In nds
      Me.Formats.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:identifier", xmlns)
    For Each nd As XmlElement In nds
      Me.Identifiers.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:language", xmlns)
    For Each nd As XmlElement In nds
      Me.Languages.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:publisher", xmlns)
    For Each nd As XmlElement In nds
      Me.Publishers.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:relation", xmlns)
    For Each nd As XmlElement In nds
      Me.Relations.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:rights", xmlns)
    For Each nd As XmlElement In nds
      Me.Rights.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:source", xmlns)
    For Each nd As XmlElement In nds
      Me.Sources.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:subject", xmlns)
    For Each nd As XmlElement In nds
      Me.Subjects.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:title", xmlns)
    For Each nd As XmlElement In nds
      Me.Titles.Add(nd.InnerText)
    Next

    nds = elem.SelectNodes("*//dc:type", xmlns)
    For Each nd As XmlElement In nds
      Me.Types.Add(nd.InnerText)
    Next

  End Sub

  Private Sub GetXMLRoot(xmlwr As XmlWriter)
    xmlwr.WriteStartElement("oai_dc", "dc", OaiDc.OaiDcNamespace)
    xmlwr.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", String.Format("{0} {1}", OaiDc.OaiDcNamespace, OaiDc.OaiDcSchemaLocation))
  End Sub

  Public Sub GetXml(xmlwr As XmlWriter)
    Me.GetXMLRoot(xmlwr)

    For Each v As String In Me.Contributors
      xmlwr.WriteElementString("dc", "contributor", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Coverages
      xmlwr.WriteElementString("dc", "coverage", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Creators
      xmlwr.WriteElementString("dc", "creator", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Dates
      xmlwr.WriteElementString("dc", "date", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Descriptions
      xmlwr.WriteElementString("dc", "description", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Formats
      xmlwr.WriteElementString("dc", "format", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Identifiers
      xmlwr.WriteElementString("dc", "identifier", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Languages
      xmlwr.WriteElementString("dc", "language", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Publishers
      xmlwr.WriteElementString("dc", "publisher", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Relations
      xmlwr.WriteElementString("dc", "relation", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Rights
      xmlwr.WriteElementString("dc", "rights", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Sources
      xmlwr.WriteElementString("dc", "source", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Subjects
      xmlwr.WriteElementString("dc", "subject", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Titles
      xmlwr.WriteElementString("dc", "title", OaiDc.DcNamespace, v)
    Next

    For Each v As String In Me.Types
      xmlwr.WriteElementString("dc", "type", OaiDc.DcNamespace, v)
    Next

    xmlwr.WriteEndElement() 'oai_dc:dc
  End Sub

  Public ReadOnly Property Xml As XmlDocument
    Get
      Dim xmlDoc As New XmlDocument
      Using writer As XmlWriter = xmlDoc.CreateNavigator.AppendChild
        Me.GetXml(writer)
      End Using
      Return xmlDoc
    End Get
  End Property

End Class
