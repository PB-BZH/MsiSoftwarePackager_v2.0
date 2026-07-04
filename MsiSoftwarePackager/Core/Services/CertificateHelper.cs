using System.Security.Cryptography.X509Certificates;

namespace MsiSoftwarePackager.Core.Services;

public sealed class CertificateStatus {
  public string Thumbprint { get; set; } = "";
  public bool IsConfigured { get; set; }
  public bool IsFound { get; set; }
  public string StoreLocation { get; set; } = "";
  public string Subject { get; set; } = "";
  public DateTime? NotAfter { get; set; }
  public bool IsExpired { get; set; }
  public bool ExpiresSoon { get; set; }
}

public static class CodeSigningCertificateService {
  public static CertificateStatus GetCertificateStatus(string thumbprint) {
    CertificateStatus status = new();

    if (string.IsNullOrWhiteSpace(thumbprint))
      return status;

    string normalizedThumbprint =
        thumbprint
            .Replace(" ","")
            .Trim()
            .ToUpperInvariant();

    status.Thumbprint = normalizedThumbprint;
    status.IsConfigured = true;

    X509Certificate2? certificate =
        FindCertificateByThumbprint(
            normalizedThumbprint,
            out string storeLocationText
        );

    if (certificate == null)
      return status;

    status.IsFound = true;
    status.StoreLocation = storeLocationText;
    status.Subject = certificate.Subject;
    status.NotAfter = certificate.NotAfter;
    status.IsExpired = certificate.NotAfter < DateTime.Now;
    status.ExpiresSoon =
        !status.IsExpired &&
        certificate.NotAfter < DateTime.Now.AddDays(30);

    return status;
  }

  private static X509Certificate2? FindCertificateByThumbprint(
      string thumbprint,
      out string storeLocationText) {
    X509Certificate2? certificate =
        FindCertificateByThumbprintInStore(
            thumbprint,
            StoreLocation.CurrentUser,
            out storeLocationText
        );

    if (certificate != null)
      return certificate;

    certificate =
        FindCertificateByThumbprintInStore(
            thumbprint,
            StoreLocation.LocalMachine,
            out storeLocationText
        );

    return certificate;
  }

  private static X509Certificate2? FindCertificateByThumbprintInStore(
      string thumbprint,
      StoreLocation storeLocation,
      out string storeLocationText) {
    storeLocationText = storeLocation + @"\My";

    using X509Store store =
        new(StoreName.My,storeLocation);

    store.Open(OpenFlags.ReadOnly);

    foreach (X509Certificate2 certificate in store.Certificates) {
      string currentThumbprint =
          certificate.Thumbprint?
              .Replace(" ","")
              .Trim()
              .ToUpperInvariant()
          ?? "";

      if (currentThumbprint == thumbprint)
        return new X509Certificate2(certificate);
    }

    return null;
  }
}