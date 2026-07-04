using System.Security.Cryptography;
using System.Text;

namespace MsiSoftwarePackager.Core.Services;

public static class WindowsPasswordProtector {
  private static readonly byte[] Entropy =
      Encoding.UTF8.GetBytes("MsiSoftwarePackager.Upload.Password.v1");

  public static string Protect(string plainText) {
    if (string.IsNullOrEmpty(plainText))
      return string.Empty;

    byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

    byte[] protectedBytes = ProtectedData.Protect(
        plainBytes,
        Entropy,
        DataProtectionScope.CurrentUser
    );

    return Convert.ToBase64String(protectedBytes);
  }

  public static string Unprotect(string protectedText) {
    if (string.IsNullOrWhiteSpace(protectedText))
      return string.Empty;

    byte[] protectedBytes = Convert.FromBase64String(protectedText);

    byte[] plainBytes = ProtectedData.Unprotect(
        protectedBytes,
        Entropy,
        DataProtectionScope.CurrentUser
    );

    return Encoding.UTF8.GetString(plainBytes);
  }

  public static string UnprotectSafe(string protectedText) {
    if (string.IsNullOrWhiteSpace(protectedText))
      return string.Empty;

    try {
      return Unprotect(protectedText);
    }
    catch {
      return protectedText;
    }
  }
}