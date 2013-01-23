Imports System.Text
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Uiuc.Library.CollectionsRegistry
Imports System.Net
Imports Uiuc.Library.Medusa

<TestClass()> Public Class CollectionsRegistryTests

  <TestMethod()> Public Sub TestCollectionCreate()
    'need to deal with SSL
    ServicePointManager.ServerCertificateValidationCallback = AddressOf ValidateCert
    ServicePointManager.DefaultConnectionLimit = 10


    Dim json As String = "{""id"": 102, ""uuid"": ""97e98a00-43d2-0130-d503-005056b22849-f"", ""title"": ""JSON test"", ""root_directory_id"": 304, ""file_group_ids"": [94, 95] }"

    Dim encoding As New System.Text.UTF8Encoding
    Dim bytes() As Byte = encoding.GetBytes(json)

    Using os As New MemoryStream
      os.Write(bytes, 0, bytes.Length)
      os.Position = 0

      Dim c As Collection = Collection.Create(os)

      Assert.IsTrue(c.Id = 102)
      Assert.IsTrue(c.Uuid = "97e98a00-43d2-0130-d503-005056b22849-f")
      Assert.IsTrue(c.Title = "JSON test")
      Assert.IsTrue(c.RootDirectoryId = 304)
      Assert.IsTrue(c.FileGroupIds.Count = 2)
      Assert.IsTrue(c.FileGroupIds.Contains(94))
      Assert.IsTrue(c.FileGroupIds.Contains(95))

    End Using

    Dim c2 As Collection = Collection.Create(json)

    Assert.IsTrue(c2.Id = 102)
    Assert.IsTrue(c2.Uuid = "97e98a00-43d2-0130-d503-005056b22849-f")
    Assert.IsTrue(c2.Title = "JSON test")
    Assert.IsTrue(c2.RootDirectoryId = 304)
    Assert.IsTrue(c2.FileGroupIds.Count = 2)
    Assert.IsTrue(c2.FileGroupIds.Contains(94))
    Assert.IsTrue(c2.FileGroupIds.Contains(95))

    Dim c3 As Collection = Collection.Create(102)

    Assert.IsTrue(c2.Id = 102)
    Assert.IsTrue(c2.Uuid = "97e98a00-43d2-0130-d503-005056b22849-f")
    Assert.IsTrue(c2.Title = "JSON test")
    Assert.IsTrue(c2.RootDirectoryId = 304)
    Assert.IsTrue(c2.FileGroupIds.Count = 2)
    Assert.IsTrue(c2.FileGroupIds.Contains(94))
    Assert.IsTrue(c2.FileGroupIds.Contains(95))
  End Sub

  Function ValidateCert(ByVal sender As Object, _
    ByVal cert As System.Security.Cryptography.X509Certificates.X509Certificate, _
    ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, _
    ByVal sslPolicyErrors As Security.SslPolicyErrors) As Boolean
    If MedusaAppSettings.Settings.IgnoreBadCert = True Then
      Return True
    Else
      Return sslPolicyErrors = Security.SslPolicyErrors.None
    End If
  End Function

End Class