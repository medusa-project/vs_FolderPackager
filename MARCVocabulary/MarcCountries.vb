Imports System.Xml

''' <summary>
''' Provides access to the LoC code list for copuntries.
''' </summary>
''' <remarks></remarks>
Public Class MarcCountries
  Const CountriesXmlUrl As String = "http://www.loc.gov/standards/codelists/countries.xml"

  Public Property Countries As Dictionary(Of String, String)

  Private Shared _thisInst As MarcCountries


  Protected Sub New()
    Dim x As New XmlDocument()
    x.Load(CountriesXmlUrl)

    Countries = New Dictionary(Of String, String)

    Dim xns As New XmlNamespaceManager(x.NameTable)
    xns.AddNamespace("cc", "info:lc/xmlns/codelist-v1")

    Dim nds = x.SelectNodes("//cc:country", xns)

    For Each nd As XmlElement In nds
      Dim nm As XmlElement = nd.SelectSingleNode("cc:name", xns)
      Dim cd As XmlElement = nd.SelectSingleNode("cc:code", xns)

      Countries.Add(cd.InnerText, nm.InnerText)
    Next
  End Sub


  Public Shared Function Create() As MarcCountries
    If _thisInst Is Nothing Then
      _thisInst = New MarcCountries
    End If

    Return _thisInst
  End Function

  Public Shared Function GetName(code As String) As String
    If _thisInst Is Nothing Then
      _thisInst = New MarcCountries
    End If

    If code IsNot Nothing AndAlso _thisInst.Countries.ContainsKey(code) Then
      Return _thisInst.Countries.Item(code)
    Else
      Return code
    End If
  End Function
End Class
