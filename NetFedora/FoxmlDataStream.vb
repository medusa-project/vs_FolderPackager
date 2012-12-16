Imports System.Xml

Public Class FoxmlDataStream

  Public Property ParentDigitalObject As FoxmlDigitalObject

  Public Property Id As String

  Public Property State As StateCodes = StateCodes.A

  Public Property ControlGroup As ControlGroups

  Public Property Versionable As Boolean?

  Public Property DatastreamVersions As List(Of FoxmlDatastreamVersion)

  Public Sub New()
    DatastreamVersions = New List(Of FoxmlDatastreamVersion)
  End Sub

  Public Sub New(id As String, controlGroup As ControlGroups)
    Me.New()
    Me.Id = id
    Me.ControlGroup = controlGroup
  End Sub

  Public Sub GetXml(xmlwr As XmlWriter)

    xmlwr.WriteStartElement("datastream")

    xmlwr.WriteAttributeString("ID", Id)
    xmlwr.WriteAttributeString("STATE", [Enum].GetName(GetType(StateCodes), State))
    xmlwr.WriteAttributeString("CONTROL_GROUP", [Enum].GetName(GetType(ControlGroups), ControlGroup))

    If Versionable.HasValue Then
      xmlwr.WriteAttributeString("VERSIONABLE", Versionable.Value.ToString.ToLower)
    End If

    For Each dsv As FoxmlDatastreamVersion In DatastreamVersions
      dsv.GetXml(xmlwr)
    Next


    xmlwr.WriteEndElement() 'datastream

  End Sub

End Class

Public Enum ControlGroups
  E = 1 'Externally Referenced
  R = 2 'Redirected
  M = 3 'Managed
  X = 4 'Inline XML
End Enum