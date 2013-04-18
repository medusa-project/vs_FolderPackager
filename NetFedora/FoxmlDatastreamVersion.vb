Imports System.Net
Imports System.IO
Imports System.Xml
Imports Uiuc.Library.MetadataUtilities

Public Class FoxmlDatastreamVersion

  Public Property ParentDatastream As FoxmlDatastream

  Private _id As String
  Public ReadOnly Property Id As String
    Get
      Return _id
    End Get
  End Property

  Public Property AltIds As List(Of Uri)

  Public Property MimeType As String

  Public Property FormatUri As Uri

  Public Property Label As String

  Private _size As Long?
  Public ReadOnly Property Size As Long?
    Get
      Return _size
    End Get
  End Property

  Private _created As Date?
  Public ReadOnly Property Created As Date?
    Get
      Return _created
    End Get
  End Property

  Public Property XmlContent As XmlDocument

  Public Property BinaryContent As Byte()

  Public Property ContentLocationRef As Uri

  Private _contentLocationType As ContentLocationTypes = ContentLocationTypes.URL
  Public ReadOnly Property ContentLocationType As ContentLocationTypes
    Get
      Return _contentLocationType
    End Get
  End Property


  ''' <summary>
  ''' Content digest checksum algorithm
  ''' </summary>
  ''' <value>Name of the checksum algorithm to use: MD5 SHA-1 SHA-256 SHA-384 SHA-512 DISABLED DEFAULT</value>
  ''' <returns></returns>
  ''' <remarks></remarks>
  Public Property ContentDigestType As String

  Private _contentDigest As String
  ''' <summary>
  ''' Content digest checksum value for the datastream version.
  ''' </summary>
  ''' <value>Hexidecimal string value</value>
  ''' <returns></returns>
  ''' <remarks>This value must be a hexidecimal string, and beause of Fedora is must be lower-case letters.</remarks>
  Public Property ContentDigest As String
    Get
      If _contentDigest IsNot Nothing Then
        Return _contentDigest.ToLower
      Else
        Return _contentDigest
      End If
    End Get
    Set(value As String)
      If Not MetadataFunctions.IsHexString(value) Then
        Throw New FoxmlException("ContentDigest is not a valid hex string")
      End If
      _contentDigest = value.ToLower
    End Set
  End Property

  Public Sub New(mimeType As String, xml As XmlDocument)
    AltIds = New List(Of Uri)
    Me.MimeType = mimeType
    Me.XmlContent = xml

  End Sub

  Public Sub New(mimeType As String, location As Uri)
    AltIds = New List(Of Uri)
    Me.MimeType = mimeType
    Me.ContentLocationRef = location
    _contentLocationType = ContentLocationTypes.URL
  End Sub

  Public Sub New(elem As XmlElement)
    AltIds = New List(Of Uri)

    If elem.NamespaceURI <> FedoraObject.FoxmlNamespace Or elem.LocalName <> "datastreamVersion" Then
      Throw New FoxmlException("The starting element must be <foxml:datastreamVersion>.")
    End If

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("foxml", FedoraObject.FoxmlNamespace)

    Dim attr As XmlAttribute

    attr = elem.SelectSingleNode("@ID", xmlns)
    If attr Is Nothing Then
      Throw New FoxmlException("ID is a required attribute.")
    End If
    _id = attr.Value

    attr = elem.SelectSingleNode("@MIMETYPE", xmlns)
    If attr Is Nothing Then
      Throw New FoxmlException("MIMETYPE is a required attribute.")
    End If
    MimeType = attr.Value

    attr = elem.SelectSingleNode("@LABEL", xmlns)
    If attr IsNot Nothing Then
      Label = attr.Value
    End If

    attr = elem.SelectSingleNode("@ALT_IDS", xmlns)
    If attr IsNot Nothing Then
      Dim delims() As Char = {}
      Dim alts() As String = attr.Value.Split(delims, StringSplitOptions.RemoveEmptyEntries)
      For Each alt As String In alts
        AltIds.Add(New Uri(alt))
      Next
    End If

    attr = elem.SelectSingleNode("@FORMAT_URI", xmlns)
    If attr IsNot Nothing Then
      FormatUri = New Uri(attr.Value)
    End If

    attr = elem.SelectSingleNode("@SIZE", xmlns)
    If attr IsNot Nothing Then
      _size = Long.Parse(attr.Value)
    End If

    attr = elem.SelectSingleNode("@CREATED", xmlns)
    If attr Is Nothing Then
      Throw New FoxmlException("CREATED is a required attribute.")
    End If
    _created = Date.Parse(attr.Value)

    Dim propElem As XmlElement

    propElem = elem.SelectSingleNode("foxml:contentDigest", xmlns)
    If propElem IsNot Nothing Then
      ContentDigestType = propElem.GetAttribute("TYPE")
      ContentDigest = propElem.GetAttribute("DIGEST")
    End If

    propElem = elem.SelectSingleNode("foxml:contentLocation", xmlns)
    If propElem IsNot Nothing Then
      _contentLocationType = [Enum].Parse(GetType(ContentLocationTypes), propElem.GetAttribute("TYPE"))
      ContentLocationRef = New Uri(propElem.GetAttribute("REF"))
    End If

    propElem = elem.SelectSingleNode("foxml:xmlContent", xmlns)
    If propElem IsNot Nothing Then
      Dim newXML As New XmlDocument
      Dim newNd As XmlNode = newXML.ImportNode(propElem, True)
      newXML.AppendChild(newNd.CloneNode(True))
      XmlContent = newXML
    End If

    propElem = elem.SelectSingleNode("foxml:binaryContent", xmlns)
    If propElem IsNot Nothing Then
      BinaryContent = Convert.FromBase64String(propElem.InnerText)
    End If

  End Sub

  Public Function GetContent() As Stream
    Dim ret As Stream = Nothing
    If ParentDatastream.ControlGroup = ControlGroups.X Then
      ret = New MemoryStream()
      Me.XmlContent.Save(ret)
    Else
      If Me.BinaryContent IsNot Nothing Then
        ret = New MemoryStream(Me.BinaryContent)
      ElseIf Me.ContentLocationType = ContentLocationTypes.URL And Me.ContentLocationRef IsNot Nothing Then
        'TODO: add some rational error handling here
        Dim req = WebRequest.CreateDefault(Me.ContentLocationRef)
        Dim res = req.GetResponse
        ret = res.GetResponseStream
      ElseIf Me.ContentLocationType = ContentLocationTypes.INTERNAL_ID Then
        'convert internal id into 
      Else
        Throw New FoxmlException("Unable to retrieve content.")
      End If
    End If
    Return ret
  End Function



  Public Sub GetXml(xmlwr As XmlWriter)

    xmlwr.WriteStartElement("datastreamVersion")

    If String.IsNullOrWhiteSpace(Me.Id) Then
      _id = String.Format("{0}.0", Me.ParentDatastream.Id)
    End If
    xmlwr.WriteAttributeString("ID", Me.Id)


    xmlwr.WriteAttributeString("MIMETYPE", MimeType)

    If Not String.IsNullOrWhiteSpace(Label) Then
      xmlwr.WriteAttributeString("LABEL", Label)
    End If

    If Created.HasValue Then
      xmlwr.WriteAttributeString("CREATED", Created.Value.ToString("yyyy-MM-ddThh:mm:ss.fffK"))
    End If

    If AltIds.Count > 0 Then
      xmlwr.WriteAttributeString("ALT_IDS", Join(AltIds.ToArray, " "))
    End If

    If FormatUri IsNot Nothing Then
      xmlwr.WriteAttributeString("FORMAT_URI", FormatUri.ToString)
    End If

    If Size.HasValue Then
      xmlwr.WriteAttributeString("SIZE", Size.Value.ToString)
    End If

    If Not String.IsNullOrWhiteSpace(ContentDigestType) Then
      xmlwr.WriteStartElement("contentDigest")
      xmlwr.WriteAttributeString("TYPE", ContentDigestType)
      xmlwr.WriteAttributeString("DIGEST", ContentDigest)
      xmlwr.WriteEndElement() 'contentDigest
    End If

    If ParentDatastream.ControlGroup = ControlGroups.X Then
      If XmlContent IsNot Nothing Then
        xmlwr.WriteStartElement("xmlContent")
        xmlwr.WriteNode(XmlContent.CreateNavigator, True)
        xmlwr.WriteEndElement() ' xmlContent
      Else
        Throw New Exception(String.Format("CONTROL_GROUP '{0}' is missing XML Content", ParentDatastream.ControlGroup))
      End If

    ElseIf ParentDatastream.ControlGroup = ControlGroups.E Or ParentDatastream.ControlGroup = ControlGroups.M Or ParentDatastream.ControlGroup = ControlGroups.R Then
      If ContentLocationRef IsNot Nothing Then
        xmlwr.WriteStartElement("contentLocation")
        xmlwr.WriteAttributeString("TYPE", [Enum].GetName(GetType(ContentLocationTypes), ContentLocationType))
        xmlwr.WriteAttributeString("REF", ContentLocationRef.ToString)
        xmlwr.WriteEndElement() ' contentLocation
      Else
        Throw New Exception(String.Format("CONTROL_GROUP '{0}' is missing Content Location", ParentDatastream.ControlGroup))
      End If
    Else
      Throw New Exception(String.Format("Unexpected CONTROL_GROUP '{0}'", ParentDatastream.ControlGroup))
    End If

    xmlwr.WriteEndElement() 'datastreamVersion
  End Sub

End Class
