using System.Text.Json;
using MsiSoftwarePackager.Core.Models;

namespace MsiSoftwarePackager.Core.Services;

public static class GlobalSettingsService {
  private static readonly JsonSerializerOptions JsonOptions = new() {
    WriteIndented = true
  };

  public static string GetSettingsDirectory() {
    string appData =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    return Path.Combine(
        appData,
        "PB BZH Concept",
        "MsiSoftwarePackager"
    );
  }

  public static string GetSettingsPath() {
    return Path.Combine(
        GetSettingsDirectory(),
        "GlobalSettings.json"
    );
  }

  public static GlobalSettings Load() {
    string path = GetSettingsPath();
    GlobalSettings settings;

    if (!File.Exists(path)) {
      settings = new GlobalSettings();
    }
    else {
      string json =
          File.ReadAllText(path);

      settings =
          JsonSerializer.Deserialize<GlobalSettings>(json)
          ?? new GlobalSettings();
    }

    Normalize(settings);

    return settings;
  }

  private static void Normalize(GlobalSettings settings) {
    if (settings.Signing == null)
      settings.Signing = new GlobalSigningSettings();

    // --------------------------------------------------
    // Certificate thumbprint
    // --------------------------------------------------
    string certificateThumbprint =
        NormalizeThumbprint(settings.Signing.CertificateThumbprint);

    if (string.IsNullOrWhiteSpace(certificateThumbprint) ||
        !IsExpectedCertificateThumbprint(certificateThumbprint)) {
      settings.Signing.CertificateThumbprint =
          SigningOptions.DefaultCertificateThumbprint;
    }
    else {
      settings.Signing.CertificateThumbprint =
          certificateThumbprint;
    }


    // --------------------------------------------------
    // Timestamp URL
    // --------------------------------------------------
    if (string.IsNullOrWhiteSpace(settings.Signing.TimestampUrl) ||
        string.Equals(
            settings.Signing.TimestampUrl.Trim(),
            "http://timestamp.digicert.com",
            StringComparison.OrdinalIgnoreCase)) {
      settings.Signing.TimestampUrl =
          SigningOptions.DefaultTimestampUrl;
    }


    // --------------------------------------------------
    // SSL.com CodeSignTool path
    // --------------------------------------------------
    if (string.IsNullOrWhiteSpace(settings.Signing.SslComCodeSignToolPath) ||
        string.Equals(
            settings.Signing.SslComCodeSignToolPath.Trim(),
            "SslComCodeSignToolPath",
            StringComparison.OrdinalIgnoreCase)) {
      settings.Signing.SslComCodeSignToolPath =
          SigningOptions.DefaultSslComCodeSignToolPath;
    }
  }

  private static string NormalizeThumbprint(string? thumbprint) {
    if (string.IsNullOrWhiteSpace(thumbprint))
      return string.Empty;

    return thumbprint
        .Replace(" ",string.Empty)
        .Trim()
        .ToUpperInvariant();
  }

  private static bool IsExpectedCertificateThumbprint(string thumbprint) {
    string expectedThumbprint =
        NormalizeThumbprint(SigningOptions.DefaultCertificateThumbprint);

    return string.Equals(
        thumbprint,
        expectedThumbprint,
        StringComparison.OrdinalIgnoreCase
    );
  }

  public static void Save(GlobalSettings settings) {
    string directory = GetSettingsDirectory();

    Directory.CreateDirectory(directory);

    string json =
        JsonSerializer.Serialize(
            settings,
            JsonOptions
        );

    File.WriteAllText(
        GetSettingsPath(),
        json
    );
  }
}