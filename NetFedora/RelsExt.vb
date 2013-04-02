Imports System.Xml
Imports System.Xml.Schema

Public Class RelsExt
  Public Const RdfNamespace As String = "http://www.w3.org/1999/02/22-rdf-syntax-ns#"
  Public Const RelsExtUri As String = "info:fedora/fedora-system:FedoraRELSExt-1.0"

  Private subjectUri As Uri
  Public ReadOnly Property Subject As Uri
    Get
      Return subjectUri
    End Get
  End Property
  Public ReadOnly Property Pid As String
    Get
      Dim ret As String = ""
      If subjectUri.ToString.StartsWith(FedoraObject.FedoraUriPrefix) Then
        ret = subjectUri.ToString.Substring(FedoraObject.FedoraUriPrefix.Length)
      Else
        Throw New FedoraException("Subject of RelsExt is not a valid Fedora Object URI")
      End If
      Return ret
    End Get
  End Property

  Private xmlns As XmlNamespaceManager

  Private relats As List(Of RdfPredicateObject)

  Public Function GetRelatedObjectPids(predicate As Uri) As List(Of String)
    Dim ret As New List(Of String)
    Dim rs = relats.Where(Function(r1) Uri.Compare(r1.PredicateUri, predicate, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.Ordinal) = 0)
    For Each r In rs
      If Not String.IsNullOrWhiteSpace(r.ObjectPid) Then
        ret.Add(r.ObjectPid)
      End If
    Next
    Return ret
  End Function

  Public Function GetRelatedObjectPids(localName As String, ns As String) As List(Of String)
    Return Me.GetRelatedObjectPids(New Uri(ns & localName))
  End Function

  Public Function GetRelatedObjectUris(predicate As Uri) As List(Of Uri)
    Dim ret As New List(Of Uri)
    Dim rs = relats.Where(Function(r1) Uri.Compare(r1.PredicateUri, predicate, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.Ordinal) = 0)
    For Each r In rs
      ret.Add(r.ObjectUri)
    Next
    Return ret
  End Function

  Public Function GetRelatedObjectUris(localName As String, ns As String) As List(Of Uri)
    Return Me.GetRelatedObjectUris(New Uri(ns & localName))
  End Function

  Private nextPrefix As Integer = AscW("a") - 1
  Private Function GetNextPrefix() As String
    nextPrefix = nextPrefix + 1
    If nextPrefix > AscW("z") Then
      Throw New FedoraException("Too many different namespaces.")
    End If
    Return String.Format("_{0}", ChrW(nextPrefix))
  End Function

  Public Sub New(pid As String)
    subjectUri = New Uri(String.Format("{0}{1}", FedoraObject.FedoraUriPrefix, pid))
    xmlns = New XmlNamespaceManager(New NameTable)
    relats = New List(Of RdfPredicateObject)
  End Sub

  ''' <summary>
  ''' Parse the given XML to create a new instance of the RelsExt object
  ''' </summary>
  ''' <param name="elem"></param>
  ''' <remarks>This is not a general RDF XML parser; it assumes the XML serialization as shown in the Fedora Reference Example
  ''' https://wiki.duraspace.org/display/FEDORA36/FOXML+Reference+Example </remarks>
  Public Sub New(elem As XmlElement)
    Dim xns As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
    xns.AddNamespace("rdf", RelsExt.RdfNamespace)

    Dim descr As XmlElement = elem.SelectSingleNode("//rdf:Description", xns)

    If descr IsNot Nothing Then
      Dim about As String = descr.GetAttribute("about", RelsExt.RdfNamespace)
      If String.IsNullOrWhiteSpace(about) Then
        Throw New FedoraException("RelsExt XML is missing rdf:Description/@rdf:about attribute")
      Else
        subjectUri = New Uri(about)
        xmlns = New XmlNamespaceManager(New NameTable)
        relats = New List(Of RdfPredicateObject)

        Dim props As XmlNodeList = descr.SelectNodes("*")
        For Each prop As XmlElement In props
          Dim objStr As String = prop.GetAttribute("resource", RelsExt.RdfNamespace)
          If String.IsNullOrWhiteSpace(prop.Prefix) Then
            Me.AddRelationship(prop.LocalName, prop.NamespaceURI, New Uri(objStr))
          Else
            Me.AddRelationship(prop.Prefix, prop.LocalName, prop.NamespaceURI, New Uri(objStr))

          End If
        Next
      End If
    Else
      Throw New FedoraException("RelsExt XML is missing rdf:Description")
    End If


  End Sub


  Public Sub AddRelationship(relatPrefix As String, relatLocalName As String, relatNamespace As String, pid As String)
    Me.AddRelationship(relatPrefix, relatLocalName, relatNamespace, New Uri(FedoraObject.FedoraUriPrefix & pid))
  End Sub

  Public Sub AddRelationship(relatPrefix As String, relatLocalName As String, relatNamespace As String, objUri As Uri)
    Dim newRelat As New RdfPredicateObject(relatPrefix, relatLocalName, relatNamespace, objUri)
    xmlns.AddNamespace(relatPrefix, relatNamespace)
    relats.Add(newRelat)
  End Sub

  Public Sub AddRelationship(relatLocalName As String, relatNamespace As String, pid As String)
    Me.AddRelationship(relatLocalName, relatNamespace, New Uri(FedoraObject.FedoraUriPrefix & pid))
  End Sub

  Public Sub AddRelationship(relatLocalName As String, relatNamespace As String, objUri As Uri)
    Dim prefix As String = xmlns.LookupPrefix(relatNamespace)
    If String.IsNullOrWhiteSpace(prefix) Then
      prefix = Me.GetNextPrefix
    End If
    Me.AddRelationship(prefix, relatLocalName, relatNamespace, objUri)
  End Sub

  Private Sub GetXMLRoot(xmlwr As XmlWriter)
    xmlwr.WriteStartElement("rdf", "RDF", RelsExt.RdfNamespace)

    For Each ns In xmlns
      If ns <> "xmlns" And ns <> "xml" And ns <> "" Then
        xmlwr.WriteAttributeString("xmlns", ns, Nothing, xmlns.LookupNamespace(ns))
      End If
    Next

  End Sub

  Public Sub GetXml(xmlwr As XmlWriter)
    Me.GetXMLRoot(xmlwr)

    xmlwr.WriteStartElement("rdf", "Description", RelsExt.RdfNamespace)
    xmlwr.WriteAttributeString("rdf", "about", RelsExt.RdfNamespace, Me.subjectUri.ToString)

    For Each pred In Me.relats
      xmlwr.WriteStartElement(pred.PredicatePrefix, pred.PredicateLocalName, pred.PredicateNamespace.ToString)
      xmlwr.WriteAttributeString("rdf", "resource", RelsExt.RdfNamespace, pred.ObjectUri.ToString)
      xmlwr.WriteEndElement()
    Next

    xmlwr.WriteEndElement() 'rdf:Description

    xmlwr.WriteEndElement() 'rdf:RDF
  End Sub

  Public ReadOnly Property Xml As XmlDocument
    Get
      Dim xmlDoc As New XmlDocument
      Using writer As XmlWriter = xmlDoc.CreateNavigator.AppendChild
        Me.GetXml(writer)
        writer.Close()
      End Using
      Return xmlDoc
    End Get
  End Property


  Private Class RdfPredicateObject
    Public PredicatePrefix As String
    Public PredicateLocalName As String
    Public PredicateNamespace As String
    Public ObjectUri As Uri

    Public Sub New(prefix As String, localName As String, ns As String, objUri As Uri)
      PredicatePrefix = prefix
      PredicateLocalName = localName
      PredicateNamespace = ns
      ObjectUri = objUri
    End Sub

    Public ReadOnly Property PredicateUri As Uri
      Get
        Return New Uri(String.Format("{0}{1}", PredicateNamespace, PredicateLocalName))
      End Get
    End Property

    Public ReadOnly Property ObjectPid As String
      Get
        Dim ret As String = ""
        If ObjectUri.ToString.StartsWith(FedoraObject.FedoraUriPrefix) Then
          ret = ObjectUri.ToString.Substring(FedoraObject.FedoraUriPrefix.Length)
        End If
        Return ret
      End Get
    End Property

  End Class

  Public ReadOnly Property Datastream As FoxmlDatastream
    Get
      Dim fxDSRelsExt As New FoxmlDatastream("RELS-EXT", ControlGroups.X)
      Dim fxDSRelsExtV As New FoxmlDatastreamVersion("application/rdf+xml", Me.Xml)
      fxDSRelsExtV.FormatUri = New Uri(RelsExt.RelsExtUri)

      fxDSRelsExt.DatastreamVersions.Add(fxDSRelsExtV)

      Return fxDSRelsExt
    End Get
  End Property

End Class
