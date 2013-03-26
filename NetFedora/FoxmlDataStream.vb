Imports System.Xml
Imports System.Text

Public Class FoxmlDatastream

  Public Property ParentDigitalObject As FoxmlDigitalObject

  Private _id As String
  Public ReadOnly Property Id As String
    Get
      Return _id
    End Get
  End Property

  Public Property State As StateCodes = StateCodes.A

  Private _controlGroup As ControlGroups
  Public ReadOnly Property ControlGroup As ControlGroups
    Get
      Return _controlGroup
    End Get
  End Property

  Public Property Versionable As Boolean?

  Public Property DatastreamVersions As List(Of FoxmlDatastreamVersion)

  Public Sub New(id As String, controlGroup As ControlGroups)
    'XML NCNAME (except does not support Unicode in the supplemenatary range \u10000-\ueffff )
    'Taken from http://www.w3.org/TR/REC-xml/#sec-common-syn with the colon removed from the NameStartChar
    Dim re As New RegularExpressions.Regex("^[_A-Za-z\u00c0-\u00d6\u00d8-\u00f6\u00f8-\u02ff\u0370-\u037d\u037f-\u1fff\u200c-\u200d\u2070-\u218f\u2c00-\u2fef\u3001-\ud7ff\uf900-\ufdcf\ufdf0-\ufffd][_A-Za-z\u00c0-\u00d6\u00d8-\u00f6\u00f8-\u02ff\u0370-\u037d\u037f-\u1fff\u200c-\u200d\u2070-\u218f\u2c00-\u2fef\u3001-\ud7ff\uf900-\ufdcf\ufdf0-\ufffd\-\.0-9\u00b7\u0300-\u036f\u203f-\u2040]*$")

    If Not re.IsMatch(id) Then
      Throw New FoxmlException(String.Format("ID '{0}' is not valid; it is not a valid NCNAME.", id))
    End If
    If id.Length > 64 Then
      Throw New FoxmlException(String.Format("ID '{0}' is not valid; length is greater than 64.", id))
    End If

    DatastreamVersions = New List(Of FoxmlDatastreamVersion)
    _id = id
    _controlGroup = controlGroup
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

  Public Sub New(elem As XmlElement)
    DatastreamVersions = New List(Of FoxmlDatastreamVersion)

    If elem.NamespaceURI <> FedoraObject.FoxmlNamespace Or elem.LocalName <> "datastream" Then
      Throw New FoxmlException("The starting element must be <foxml:datastream>.")
    End If

    Dim xmlns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xmlns.AddNamespace("foxml", FedoraObject.FoxmlNamespace)

    Dim attr As XmlAttribute

    attr = elem.SelectSingleNode("@ID", xmlns)
    If attr Is Nothing Then
      Throw New FoxmlException("ID is a required attribute.")
    End If
    _id = attr.Value

    attr = elem.SelectSingleNode("@STATE", xmlns)
    If attr Is Nothing Then
      Throw New FoxmlException("STATE is a required attribute.")
    End If
    State = [Enum].Parse(GetType(StateCodes), attr.Value.Substring(0, 1), True)

    attr = elem.SelectSingleNode("@CONTROL_GROUP", xmlns)
    If attr Is Nothing Then
      Throw New FoxmlException("CONTROL_GROUP is a required attribute.")
    End If
    _controlGroup = [Enum].Parse(GetType(ControlGroups), attr.Value.Substring(0, 1), True)

    attr = elem.SelectSingleNode("@VERSIONABLE", xmlns)
    If attr Is Nothing Then
      Throw New FoxmlException("VERSIONABLE is a required attribute.")
    End If
    Versionable = Boolean.Parse(attr.Value)

    Dim strmElems As XmlNodeList = elem.SelectNodes("foxml:datastreamVersion", xmlns)
    For Each strmElem As XmlElement In strmElems
      Dim strmVer As FoxmlDatastreamVersion = New FoxmlDatastreamVersion(strmElem)
      strmVer.ParentDatastream = Me
      Me.DatastreamVersions.Add(strmVer)
    Next
  End Sub


  Public Function GetLatestVersion() As FoxmlDatastreamVersion
    Dim vers As New Dictionary(Of Date, FoxmlDatastreamVersion)

    For Each v As FoxmlDatastreamVersion In Me.DatastreamVersions
      vers.Add(v.Created, v)
    Next

    Dim sortedVers = vers.OrderByDescending(Function(kv) kv.Key)

    Return sortedVers.First.Value

  End Function



End Class
