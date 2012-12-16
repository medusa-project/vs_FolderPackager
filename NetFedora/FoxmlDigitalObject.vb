Imports System.Xml
Imports System.Xml.Schema
Imports System.Text

Public Class FoxmlDigitalObject
  Public Property ValidateXML As Boolean = True
  Private Shared _schemas As XmlSchemaSet

  Public Const FoxmlNamespace As String = "info:fedora/fedora-system:def/foxml#"
  Public Const FoxmlSchemaLocation As String = "http://www.fedora.info/definitions/1/0/foxml1-1.xsd"
  Public Const FedoraModelNamespace As String = "info:fedora/fedora-system:def/model#"


  Public Const Version As String = "1.1"

  Public Property Pid As String

  'object properties
  Public Property State As StateCodes = StateCodes.A
  Public Property Label As String
  Public Property OwnerId As String
  Public Property CreatedDate As Date?
  Public Property LastModifiedDate As Date?

  Public Property DataStreams As List(Of FoxmlDataStream)

  Public Sub New()
    DataStreams = New List(Of FoxmlDataStream)
  End Sub

  Public Sub New(id As String)
    Me.New()
    Pid = id
  End Sub

  Public Sub GetXMLRoot(xmlwr As XmlWriter)
    xmlwr.WriteStartElement("digitalObject", FoxmlDigitalObject.FoxmlNamespace)
    xmlwr.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", String.Format("{0} {1}", FoxmlDigitalObject.FoxmlNamespace, FoxmlDigitalObject.FoxmlSchemaLocation))
    xmlwr.WriteAttributeString("VERSION", FoxmlDigitalObject.Version)
    xmlwr.WriteAttributeString("PID", Pid)
  End Sub

  Public Function GetXML() As String
    Dim sb As New StringBuilder()
    Dim xmlwr As XmlWriter = XmlWriter.Create(sb, New XmlWriterSettings With {.Indent = True, .Encoding = Encoding.UTF8, .OmitXmlDeclaration = True})

    Me.GetXMLRoot(xmlwr)

    xmlwr.WriteStartElement("objectProperties")

    xmlwr.WriteStartElement("property")
    xmlwr.WriteAttributeString("NAME", String.Format("{0}{1}", FoxmlDigitalObject.FedoraModelNamespace, "state"))
    xmlwr.WriteAttributeString("VALUE", [Enum].GetName(GetType(StateCodes), State))
    xmlwr.WriteEndElement()

    If Not String.IsNullOrWhiteSpace(Label) Then
      xmlwr.WriteStartElement("property")
      xmlwr.WriteAttributeString("NAME", String.Format("{0}{1}", FoxmlDigitalObject.FedoraModelNamespace, "label"))
      xmlwr.WriteAttributeString("VALUE", Label)
      xmlwr.WriteEndElement()
    End If

    If Not String.IsNullOrWhiteSpace(OwnerId) Then
      xmlwr.WriteStartElement("property")
      xmlwr.WriteAttributeString("NAME", String.Format("{0}{1}", FoxmlDigitalObject.FedoraModelNamespace, "ownerId"))
      xmlwr.WriteAttributeString("VALUE", OwnerId)
      xmlwr.WriteEndElement()
    End If

    If CreatedDate.HasValue Then
      xmlwr.WriteStartElement("property")
      xmlwr.WriteAttributeString("NAME", String.Format("{0}{1}", FoxmlDigitalObject.FedoraModelNamespace, "createdDate"))
      xmlwr.WriteAttributeString("VALUE", CreatedDate.Value.ToString("yyyy-MM-ddThh:mm:ss.fffK"))
      xmlwr.WriteEndElement()
    End If

    If LastModifiedDate.HasValue Then
      xmlwr.WriteStartElement("property")
      xmlwr.WriteAttributeString("NAME", String.Format("{0}{1}", FoxmlDigitalObject.FedoraModelNamespace, "lastModifiedDate"))
      xmlwr.WriteAttributeString("VALUE", LastModifiedDate.Value.ToString("yyyy-MM-ddThh:mm:ss.fffK"))
      xmlwr.WriteEndElement()
    End If

    xmlwr.WriteEndElement() 'objectProperties

    For Each ds As FoxmlDataStream In DataStreams
      ds.getxml(xmlwr)
    Next


    xmlwr.WriteEndElement() 'digitalObject
    xmlwr.Close()

    Dim xmlStr As String = sb.ToString

    Validate(xmlStr)

    Return xmlStr

  End Function

  Private Sub Validate(ByVal xmlStr As String)

    If Not ValidateXML Then Exit Sub

    If _schemas Is Nothing Then
      _schemas = New XmlSchemaSet
      _schemas.Add(FoxmlDigitalObject.FoxmlNamespace, FoxmlDigitalObject.FoxmlSchemaLocation)
    End If

    Dim document As XmlDocument = New XmlDocument()
    document.Schemas.Add(_schemas)
    document.LoadXml(xmlStr)
    Dim validation As ValidationEventHandler = New ValidationEventHandler(AddressOf SchemaValidationHandler)
    document.Validate(validation)

  End Sub

  Private Sub SchemaValidationHandler(ByVal sender As Object, ByVal e As ValidationEventArgs)

    Select Case e.Severity
      Case XmlSeverityType.Error
        Throw New XmlSchemaException(e.Message)
        Exit Sub
      Case XmlSeverityType.Warning
        Throw New XmlSchemaException(e.Message)
        Exit Sub
    End Select

  End Sub

End Class

Public Enum StateCodes As Integer
  A = 1 'Active
  I = 2 'Inactive
  D = 3 'Deleted
End Enum