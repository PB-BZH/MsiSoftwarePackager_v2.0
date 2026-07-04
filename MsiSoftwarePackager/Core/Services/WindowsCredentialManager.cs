using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace MsiSoftwarePackager.Core.Services;

public static class WindowsCredentialManager {
  private const int CRED_TYPE_GENERIC = 1;
  private const int CRED_PERSIST_LOCAL_MACHINE = 2;

  [StructLayout(LayoutKind.Sequential,CharSet = CharSet.Unicode)]
  private struct CREDENTIAL {
    public int Flags;
    public int Type;
    public string TargetName;
    public string Comment;
    public FILETIME LastWritten;
    public int CredentialBlobSize;
    public IntPtr CredentialBlob;
    public int Persist;
    public int AttributeCount;
    public IntPtr Attributes;
    public string TargetAlias;
    public string UserName;
  }

  [DllImport("advapi32.dll",CharSet = CharSet.Unicode,SetLastError = true)]
  private static extern bool CredWrite(
      ref CREDENTIAL credential,
      int flags
  );

  [DllImport("advapi32.dll",CharSet = CharSet.Unicode,SetLastError = true)]
  private static extern bool CredRead(
      string target,
      int type,
      int reservedFlag,
      out IntPtr credentialPtr
  );

  [DllImport("advapi32.dll",SetLastError = true)]
  private static extern bool CredDelete(
      string target,
      int type,
      int flags
  );

  [DllImport("advapi32.dll",SetLastError = true)]
  private static extern void CredFree(IntPtr buffer);

  public static void SavePassword(
      string target,
      string userName,
      string password) {
    if (string.IsNullOrWhiteSpace(target))
      throw new ArgumentException("Credential target is empty.",nameof(target));

    if (password == null)
      password = string.Empty;

    byte[] passwordBytes = Encoding.Unicode.GetBytes(password);

    IntPtr passwordBlob = Marshal.AllocCoTaskMem(passwordBytes.Length);

    try {
      Marshal.Copy(passwordBytes,0,passwordBlob,passwordBytes.Length);

      CREDENTIAL credential = new() {
        Type = CRED_TYPE_GENERIC,
        TargetName = target,
        UserName = userName ?? string.Empty,
        CredentialBlob = passwordBlob,
        CredentialBlobSize = passwordBytes.Length,
        Persist = CRED_PERSIST_LOCAL_MACHINE,
        Comment = "Stored by MsiSoftwarePackager"
      };

      if (!CredWrite(ref credential,0)) {
        int error = Marshal.GetLastWin32Error();

        throw new InvalidOperationException(
            "Unable to write Windows credential. Win32 error: " + error
        );
      }
    }
    finally {
      Marshal.FreeCoTaskMem(passwordBlob);
    }
  }

  public static string ReadPassword(string target) {
    if (string.IsNullOrWhiteSpace(target))
      return string.Empty;

    if (!CredRead(
            target,
            CRED_TYPE_GENERIC,
            0,
            out IntPtr credentialPtr)) {
      return string.Empty;
    }

    try {
      CREDENTIAL credential =
          Marshal.PtrToStructure<CREDENTIAL>(credentialPtr);

      if (credential.CredentialBlob == IntPtr.Zero ||
          credential.CredentialBlobSize <= 0) {
        return string.Empty;
      }

      byte[] passwordBytes = new byte[credential.CredentialBlobSize];

      Marshal.Copy(
          credential.CredentialBlob,
          passwordBytes,
          0,
          credential.CredentialBlobSize
      );

      return Encoding.Unicode.GetString(passwordBytes);
    }
    finally {
      CredFree(credentialPtr);
    }
  }

  public static bool DeletePassword(string target) {
    if (string.IsNullOrWhiteSpace(target))
      return false;

    return CredDelete(
        target,
        CRED_TYPE_GENERIC,
        0
    );
  }

  public static string BuildTargetName(
      string protocol,
      string host,
      string userName) {
    protocol = NormalizeCredentialPart(
        protocol,
        "SFTP"
    );

    host = NormalizeCredentialPart(
        host,
        "unknown-host"
    );

    userName = NormalizeCredentialPart(
        userName,
        "unknown-user"
    );

    return BuildCredentialTarget(
        "Upload",
        protocol,
        host,
        userName
    );
  }

  public static string BuildSslComSigningTargetName(
      string userName) {
    userName = NormalizeCredentialPart(
        userName,
        "unknown-user"
    );

    return BuildCredentialTarget(
        "CodeSigning",
        "SSLCom",
        "eSigner",
        userName
    );
  }

  public static string BuildSslComTotpSecretTargetName(
      string userName) {
    userName = NormalizeCredentialPart(
        userName,
        "unknown-user"
    );

    return BuildCredentialTarget(
        "CodeSigning",
        "SSLCom",
        "TotpSecret",
        userName
    );
  }

  public static string BuildUploadTargetName(
    string protocol,
    string host,
    string userName) {
    protocol = NormalizeCredentialPart(
        protocol,
        "SFTP"
    );

    host = NormalizeCredentialPart(
        host,
        "unknown-host"
    );

    userName = NormalizeCredentialPart(
        userName,
        "unknown-user"
    );

    return BuildCredentialTarget(
        "Upload",
        protocol,
        host,
        userName
    );
  }

  private static string BuildCredentialTarget(
      params string[] parts) {
    List<string> normalizedParts = new();

    foreach (string part in parts) {
      normalizedParts.Add(
          NormalizeCredentialPart(
              part,
              "unknown"
          )
      );
    }

    return "MsiSoftwarePackager:" +
           string.Join(":",normalizedParts);
  }

  private static string NormalizeCredentialPart(
      string value,
      string defaultValue) {
    if (string.IsNullOrWhiteSpace(value))
      return defaultValue;

    return value
        .Trim()
        .Replace(":","-")
        .Replace("\\","-")
        .Replace("/","-");
  }
}
