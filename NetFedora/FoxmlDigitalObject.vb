Imports System.Xml
Imports System.Xml.Schema
Imports System.Text
Imports System.IO
Imports System.ComponentModel

Public Class FoxmlDigitalObject
  Public Property ValidateXML As Boolean = True
  Private Shared _schemas As XmlSchemaSet

  Public Const FoxmlSchemaLocation As String = "http://www.fedora.info/definitions/1/0/foxml1-1.xsd"

  Public Const Version As String = "1.1"

  Private _pid As String
  Public ReadOnly Property Pid As String
    Get
      Return _pid
    End Get
  End Property

  'object properties
  Public Property State As StateCodes = StateCodes.A
  Public Property Label As String
  Public Property OwnerId As String

  Private _createdDate As Date?
  Public ReadOnly Property CreatedDate As Date?
    Get
      Return _createdDate
    End Get
  End Property

  Private _lastModifiedDate As Date?
  Public ReadOnly Property LastModifiedDate As Date?
    Get
      Return _lastModifiedDate
    End Get
  End Property

  Public Property DataStreams As BindingList(Of FoxmlDatastream)

  ''' <summary>
  ''' Update the parent of the item being added to the datastream list
  ''' </summary>
  ''' <param name="sender"></param>
  ''' <param name="e"></param>
  ''' <remarks></remarks>
  Private Sub DataStreams_Changed(ByVal sender As Object, ByVal e As ListChangedEventArgs)
    Select Case e.ListChangedType
      Case ListChangedType.ItemAdded
        'set the parent of the added DS to this object
        Dim ds As FoxmlDatastream = DataStreams.Item(e.NewIndex)
        ds.ParentDigitalObject = Me

      Case ListChangedType.ItemDeleted
        'This is really not very useful since the item has already been deleted once this event fires, 
        'May not really be neccisary to do anything anyway since once a DS is removed it will probably be discarded
        'and if it isn't discarded it might be attached to a different object in which case its parent will be
        'updated at that point

      Case ListChangedType.Reset
        'reset all the parent pointers to this object
        For Each ds As FoxmlDatastream In Me.DataStreams
          ds.ParentDigitalObject = Me
        Next

    End Select

  End Sub


  Public Sub New(id As String)
    'Taken from the Fedora documentation https://wiki.duraspace.org/display/FEDORA36/Fedora+Identifiers#FedoraIdentifiers-PIDspids 
    Dim re As New RegularExpressions.Regex("^([A-Za-z0-9]|-|\.)+:(([A-Za-z0-9])|-|\.|~|_|(%[0-9A-F]{2}))+$")

    If Not re.IsMatch(id) Then
      Throw New FoxmlException(String.Format("PID '{0}' is not valid; does not match regular expression.", id))
    End If
    If id.Length > 64 Then
      Throw New FoxmlException(String.Format("PID '{0}' is not valid; length is greater than 64.", id))
    End If

    DataStreams = New BindingList(Of FoxmlDatastream)
    AddHandler DataStreams.ListChanged, AddressOf DataStreams_Changed

    _pid = id
  End Sub

  Public Sub New(elem As XmlElement)
    DataStreams = New BindingList(Of FoxmlDatastream)
    AddHandler DataStreams.ListChanged, AddressOf DataStreams_Changed

    If elem.NamespaceURI <> FedoraObject.FoxmlNamespace Or elem.LocalName <> "digitalObject" Then
      Throw New FoxmlException("The document element must be <foxml:digitalObject>.")
    End If

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("foxml", FedoraObject.FoxmlNamespace)

    Dim verAttr As XmlAttribute = elem.SelectSingleNode("/foxml:digitalObject/@VERSION", xmlns)
    If verAttr Is Nothing Then
      Throw New FoxmlException("VERSION is a required attribute.")
    End If
    If verAttr.Value <> FoxmlDigitalObject.Version Then
      Throw New FoxmlException(String.Format("Foxml version {0} is not supported.  Version must be {1}.", verAttr.Value, FoxmlDigitalObject.Version))
    End If

    Dim pidAttr As XmlAttribute = elem.SelectSingleNode("/foxml:digitalObject/@PID", xmlns)
    If pidAttr Is Nothing Then
      Throw New FoxmlException("PID is a required attribute.")
    End If
    _pid = pidAttr.Value

    Dim propElem As XmlElement

    propElem = elem.SelectSingleNode(String.Format("/foxml:digitalObject/foxml:objectProperties/foxml:property[@NAME='{0}{1}']", FedoraObject.FedoraModelNamespace, "state"), xmlns)
    If propElem Is Nothing Then
      Throw New FoxmlException("State is a required property.")
    End If
    State = [Enum].Parse(GetType(StateCodes), propElem.GetAttribute("VALUE").Substring(0, 1), True)

    propElem = elem.SelectSingleNode(String.Format("/foxml:digitalObject/foxml:objectProperties/foxml:property[@NAME='{0}{1}']", FedoraObject.FedoraModelNamespace, "label"), xmlns)
    If propElem IsNot Nothing Then
      Label = propElem.GetAttribute("VALUE")
    End If

    propElem = elem.SelectSingleNode(String.Format("/foxml:digitalObject/foxml:objectProperties/foxml:property[@NAME='{0}{1}']", FedoraObject.FedoraModelNamespace, "ownerId"), xmlns)
    If propElem IsNot Nothing Then
      OwnerId = propElem.GetAttribute("VALUE")
    End If

    propElem = elem.SelectSingleNode(String.Format("/foxml:digitalObject/foxml:objectProperties/foxml:property[@NAME='{0}{1}']", FedoraObject.FedoraModelNamespace, "createdDate"), xmlns)
    If propElem IsNot Nothing Then
      _createdDate = Date.Parse(propElem.GetAttribute("VALUE"))
    End If

    propElem = elem.SelectSingleNode(String.Format("/foxml:digitalObject/foxml:objectProperties/foxml:property[@NAME='{0}{1}']", FedoraObject.FedoraViewNamespace, "lastModifiedDate"), xmlns)
    If propElem IsNot Nothing Then
      _lastModifiedDate = Date.Parse(propElem.GetAttribute("VALUE"))
    End If

    Dim strmElems As XmlNodeList = elem.SelectNodes("/foxml:digitalObject/foxml:datastream", xmlns)
    For Each strmElem As XmlElement In strmElems
      Dim strm As FoxmlDatastream = New FoxmlDatastream(strmElem)
      strm.ParentDigitalObject = Me
      Me.DataStreams.Add(strm)
    Next

  End Sub

  Private Sub GetXMLRoot(xmlwr As XmlWriter)
    xmlwr.WriteStartElement("digitalObject", FedoraObject.FoxmlNamespace)
    xmlwr.WriteAttributeString("xsi", "schemaLocation", "http://www.w3.org/2001/XMLSchema-instance", String.Format("{0} {1}", FedoraObject.FoxmlNamespace, FoxmlDigitalObject.FoxmlSchemaLocation))
    xmlwr.WriteAttributeString("VERSION", FoxmlDigitalObject.Version)
    xmlwr.WriteAttributeString("PID", Pid)
  End Sub

  Public Function GetXML() As String
    Dim sb As New StringBuilder()
    Dim xmlwr As XmlWriter = XmlWriter.Create(sb, New XmlWriterSettings With {.Indent = True, .Encoding = Encoding.UTF8, .OmitXmlDeclaration = True})

    Me.GetXMLRoot(xmlwr)

    xmlwr.WriteStartElement("objectProperties")

    xmlwr.WriteStartElement("property")
    xmlwr.WriteAttributeString("NAME", String.Format("{0}{1}", FedoraObject.FedoraModelNamespace, "state"))
    xmlwr.WriteAttributeString("VALUE", [Enum].GetName(GetType(StateCodes), State))
    xmlwr.WriteEndElement()

    If Not String.IsNullOrWhiteSpace(Label) Then
      xmlwr.WriteStartElement("property")
      xmlwr.WriteAttributeString("NAME", String.Format("{0}{1}", FedoraObject.FedoraModelNamespace, "label"))
      xmlwr.WriteAttributeString("VALUE", Label)
      xmlwr.WriteEndElement()
    End If

    If Not String.IsNullOrWhiteSpace(OwnerId) Then
      xmlwr.WriteStartElement("property")
      xmlwr.WriteAttributeString("NAME", String.Format("{0}{1}", FedoraObject.FedoraModelNamespace, "ownerId"))
      xmlwr.WriteAttributeString("VALUE", OwnerId)
      xmlwr.WriteEndElement()
    End If

    If CreatedDate.HasValue Then
      xmlwr.WriteStartElement("property")
      xmlwr.WriteAttributeString("NAME", String.Format("{0}{1}", FedoraObject.FedoraModelNamespace, "createdDate"))
      xmlwr.WriteAttributeString("VALUE", CreatedDate.Value.ToString("yyyy-MM-ddThh:mm:ss.fffK"))
      xmlwr.WriteEndElement()
    End If

    If LastModifiedDate.HasValue Then
      xmlwr.WriteStartElement("property")
      xmlwr.WriteAttributeString("NAME", String.Format("{0}{1}", FedoraObject.FedoraViewNamespace, "lastModifiedDate"))
      xmlwr.WriteAttributeString("VALUE", LastModifiedDate.Value.ToString("yyyy-MM-ddThh:mm:ss.fffK"))
      xmlwr.WriteEndElement()
    End If

    xmlwr.WriteEndElement() 'objectProperties

    For Each ds As FoxmlDatastream In DataStreams
      ds.GetXml(xmlwr)
    Next


    xmlwr.WriteEndElement() 'digitalObject
    xmlwr.Close()

    Dim xmlStr As String = sb.ToString

    Validate(xmlStr)

    Return xmlStr

  End Function

  ''' <summary>
  ''' Save the Premis Container as a single XML file
  ''' </summary>
  ''' <param name="fileName"></param>
  ''' <remarks></remarks>
  Public Sub SaveXML(ByVal fileName As String)
    Dim xmlStr As String = Me.GetXML

    Using txtWr As New StreamWriter(fileName, False, Encoding.UTF8)
      txtWr.Write(xmlStr)
      txtWr.Close()
    End Using

  End Sub

  Private Sub Validate(ByVal xmlStr As String)

    If Not ValidateXML Then Exit Sub

    If _schemas Is Nothing Then
      _schemas = New XmlSchemaSet
      _schemas.Add(FedoraObject.FoxmlNamespace, FoxmlDigitalObject.FoxmlSchemaLocation)
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

Public Enum ControlGroups
  E = 1 'Externally Referenced
  R = 2 'Redirected
  M = 3 'Managed
  X = 4 'Inline XML
End Enum

Public Enum ContentLocationTypes
  URL = 1
  INTERNAL_ID = 2
End Enum


