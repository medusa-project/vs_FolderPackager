Imports System.IO
Imports System.Configuration
Imports System.Security
Imports System.Net
Imports Uiuc.Library.Ldap
Imports Uiuc.Library.MetadataUtilities

'TODO:  Provide an in-place packager option to rearrange and package files into different location on the same drive
'       Add PackageMode paramater: COPY or MOVE or HARDLINK
'TODO:  XSLT to transform MARC to MODS
'TODO: Ability to fetch OPAC record from online catalog
'TOOD: Ability to merge based on the idmap.csv file and the ability to identify the significant and unique portion of 
'      the filename that can be used for matching objects
'TODO: In cases of failure ability to restart an ingest at a given object
'TODO: Add a software agent for FITS
'TODO: Add SignficantCharacterstics from the FITS output
'TODO: Add a software agent to the packager itself (look at the creation event)
'TODO: Determine which file is the METS file, if any

Module Packager

  ''' <summary>
  ''' "Remember the Maine!"
  ''' </summary>
  ''' <remarks></remarks>
  Sub Main()
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
    Trace.TraceInformation("Source Folder: {0}", MedusaAppSettings.Settings.SourceFolder)

    Dim collHandle As String = proc.ProcessCollection()

    If Debugger.IsAttached Then
      proc.StartProcess(collHandle)
    Else
      Try
        proc.StartProcess(collHandle)
      Catch ex As Exception
        Console.Error.WriteLine(String.Format("Error: {0}", ex.Message))
        Trace.TraceError("{0}", ex.Message)
        'email fatal errors to operator
        MedusaHelpers.EmailException(ex)
      End Try
    End If

    Dim finished As DateTime = Now
    Trace.TraceInformation("Finished: {0}  Elapsed Time: {1} minutes", finished.ToString("s"), DateDiff(DateInterval.Minute, started, finished))
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


End Module
