using System.Diagnostics;
using System.Text;

namespace MsiSoftwarePackager.Core.Services;

public static class CodeSigningCertificateHelper {
  private const int SelfSignedCertificateValidityYears = 3;
  public static string GenerateSelfSignedCodeSigningCertificate(string publisherName) {
    if (string.IsNullOrWhiteSpace(publisherName))
      publisherName = "PB BZH Concept";

    string subject = "CN=" + publisherName.Replace("\"","");

    string psCommand =
        "$cert = New-SelfSignedCertificate " +
        "-Type CodeSigningCert " +
        "-Subject \"" + subject + "\" " +
        "-CertStoreLocation \"Cert:\\CurrentUser\\My\" " +
        "-NotAfter (Get-Date).AddYears(" + SelfSignedCertificateValidityYears + "); " +
        "$cert.Thumbprint";

    ProcessStartInfo psi = new() {
      FileName = "powershell.exe",
      Arguments =
            "-NoProfile -ExecutionPolicy Bypass -Command \"" +
            psCommand.Replace("\"","\\\"") +
            "\"",
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true,
      StandardOutputEncoding = Encoding.UTF8,
      StandardErrorEncoding = Encoding.UTF8
    };

    using Process process = new() {
      StartInfo = psi
    };

    process.Start();

    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();

    process.WaitForExit();

    if (process.ExitCode != 0) {
      throw new InvalidOperationException(
          "Self-signed certificate generation failed." +
          Environment.NewLine +
          error
      );
    }

    string thumbprint =
        output
            .Trim()
            .Replace(" ","");

    if (string.IsNullOrWhiteSpace(thumbprint)) {
      throw new InvalidOperationException(
          "Self-signed certificate was created but no thumbprint was returned."
      );
    }

    return thumbprint;
  }
}