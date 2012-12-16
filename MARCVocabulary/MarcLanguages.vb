Imports System.Xml

''' <summary>
''' Provides access to the LoC code list for languages.
''' </summary>
''' <remarks></remarks>
Public Class MarcLanguages
  Const LanguagesXmlUrl As String = "http://www.loc.gov/standards/codelists/languages.xml"

  Public Property Languages As Dictionary(Of String, String)

  Private Shared _thisInst As MarcLanguages


  Protected Sub New()
    Dim x As New XmlDocument()
    x.Load(LanguagesXmlUrl)

    Languages = New Dictionary(Of String, String)

    Dim xns As New XmlNamespaceManager(x.NameTable)
    xns.AddNamespace("cc", "info:lc/xmlns/codelist-v1")

    Dim nds = x.SelectNodes("//cc:language", xns)

    For Each nd As XmlElement In nds
      Dim nm As XmlElement = nd.SelectSingleNode("cc:name", xns)
      Dim cd As XmlElement = nd.SelectSingleNode("cc:code", xns)

      Languages.Add(cd.InnerText, nm.InnerText)
    Next
  End Sub

  Public Shared Function Create() As MarcLanguages
    If _thisInst Is Nothing Then
      _thisInst = New MarcLanguages
    End If

    Return _thisInst
  End Function

  Public Shared Function GetName(code As String) As String
    If _thisInst Is Nothing Then
      _thisInst = New MarcLanguages
    End If

    If code IsNot Nothing AndAlso _thisInst.Languages.ContainsKey(code) Then
      Return _thisInst.Languages.Item(code)
    Else
      Return code
    End If
  End Function

End Class
