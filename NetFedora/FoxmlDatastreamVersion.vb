Imports System.Xml

Public Class FoxmlDatastreamVersion

  Protected Property ParentDatastream As FoxmlDataStream

  Protected Property Id As String

  Public Property AltIds As List(Of Uri)

  Public Property MimeType As String

  Public Property FormatUri As Uri

  Public Property Label As String

  Public Property Size As Long?

  Public Property Created As Date?

  Public Property Versionable As Boolean = True

  Public Property XmlContent As XmlDocument

  Public Property ContentLocationRef As Uri

  Public Property ContentLocationType As ContentLocationTypes = ContentLocationTypes.URL

  Public Property ContentDigestType As String

  Public Property ContentDigest As String

  Public Sub New()
    AltIds = New List(Of Uri)
  End Sub

  Public Sub New(id As String, mimeType As String, xml As XmlDocument)
    Me.New()
    Me.Id = id
    Me.MimeType = mimeType
    Me.XmlContent = xml
  End Sub

  Public Sub New(id As String, mimeType As String, location As Uri)
    Me.New()
    Me.Id = id
    Me.MimeType = mimeType
    Me.ContentLocationRef = location
    Me.ContentLocationType = ContentLocationTypes.URL
  End Sub

  Public Sub GetXml(xmlwr As XmlWriter)

    xmlwr.WriteStartElement("datastreamVersion")

    xmlwr.WriteAttributeString("ID", MimeType)
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

Public Enum ContentLocationTypes
  URL = 1
  INTERNAL_ID = 2
End Enum

