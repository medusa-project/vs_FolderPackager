Imports System.IO
Imports System.Text
Imports Uiuc.Library.MARCVocabulary
Imports Uiuc.Library.FolderPackager
Imports Uiuc.Library.CollectionsRegistry
Imports System.Configuration
Imports System.Security
Imports System.Net
Imports Uiuc.Library.Ldap
Imports System.Net.Mail
Imports Uiuc.Library.MetadataUtilities

Public Class FolderPackager


  Private Sub btnSourceFolder_Click(sender As System.Object, e As System.EventArgs) Handles btnSourceFolder.Click
    fldrSourceFolder.SelectedPath = txtSourceFolder.Text
    Dim result As DialogResult = fldrSourceFolder.ShowDialog()

    If result = Windows.Forms.DialogResult.OK Then
      txtSourceFolder.Text = fldrSourceFolder.SelectedPath
    End If


  End Sub

  Private Sub btnDestinationFolder_Click(sender As System.Object, e As System.EventArgs) Handles btnDestinationFolder.Click
    fldrDestinationFolder.SelectedPath = txtDestinationFolder.Text
    Dim result As DialogResult = fldrDestinationFolder.ShowDialog()

    If result = Windows.Forms.DialogResult.OK Then
      txtDestinationFolder.Text = fldrDestinationFolder.SelectedPath
    End If
  End Sub

  Private Sub btnClose_Click(sender As System.Object, e As System.EventArgs) Handles btnClose.Click
    Me.Close()
  End Sub


  Private Sub btnCheckCollection_Click(sender As System.Object, e As System.EventArgs) Handles btnCheckCollection.Click
    Dim cr As New CollectionRecord(updnCollectionID.Value)
    If String.IsNullOrWhiteSpace(cr.Uuid) Then
      lblCollectionUuid.Text = "Error: Not Found"
    Else
      lblCollectionUuid.Text = String.Format("{0} [{1}]", cr.CollectionTitle, cr.Uuid)
    End If
    If Not String.IsNullOrWhiteSpace(cr.Filename) Then
      If File.Exists(cr.Filename) Then
        File.Delete(cr.Filename) 'delete the temporary file
      End If
    End If
  End Sub

  Private Sub btnStart_Click(sender As System.Object, e As System.EventArgs) Handles btnStart.Click
    Dim sw As New StringWriter(New StringBuilder(txtOut.Text))
    Console.SetOut(sw)
    Console.Out.WriteLine("Starting")

    Dim started As DateTime = Now

    Dim proc As New Processor

    'need to deal with SSL
    ServicePointManager.ServerCertificateValidationCallback = AddressOf ValidateCert
    ServicePointManager.DefaultConnectionLimit = 10


    'set current directory to working folder; this is the destination folder for the package
    Directory.CreateDirectory(MedusaAppSettings.Settings.WorkingFolder)
    Directory.SetCurrentDirectory(MedusaAppSettings.Settings.WorkingFolder)

    'init tracing/logging
    Trace.Listeners.Clear()
    Trace.Listeners.Add(New TextWriterTraceListener(MedusaAppSettings.Settings.LogFilePath))

    Trace.TraceInformation("Started: {0} By: {1} ", started.ToString("s"), Principal.WindowsIdentity.GetCurrent.Name)
    Trace.TraceInformation("Working Folder: {0}", Directory.GetCurrentDirectory)

    Dim collHandle As String = proc.ProcessCollection()

    Try
      proc.StartProcess(collHandle)
    Catch ex As Exception
      Console.Error.WriteLine(String.Format("Error: {0}", ex.Message))
      Trace.TraceError("{0}", ex.Message)
      'email fatal errors to operator
      Try
        Dim usr = UIUCLDAPUser.Create(Principal.WindowsIdentity.GetCurrent.Name)
        Dim msg As New System.Net.Mail.MailMessage("medusa-admin@library.illinois.edu", usr.EMail)
        msg.Subject = String.Format("Medusa Error Report: {0}", My.Application.Info.Title)
        msg.Body = String.Format("{0}{2}{2}{1}", ex.Message, ex.StackTrace, vbCrLf)

        Dim smtp As New SmtpClient()
        smtp.Send(msg)
      Catch ex2 As Exception
        Console.Error.WriteLine(String.Format("Email Error: {0}", ex2.Message))
        Trace.TraceError("{0}", ex2.Message)
      End Try

    End Try

    Dim finished As DateTime = Now
    Trace.TraceInformation("Finished: {0}  Elapsed Time: {1} seconds", finished.ToString("s"), DateDiff(DateInterval.Second, started, finished))
    Trace.Flush()

    'Console.Out.WriteLine("Done.  Press Enter to finish.")
    'Console.In.ReadLine()

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

