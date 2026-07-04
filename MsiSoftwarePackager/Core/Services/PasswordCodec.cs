using System.Text;

namespace MsiSoftwarePackager.Core.Services;

public static class PasswordCodec {
  private const string Suffix = "Password";

  public static string EncodePasswordUnicodeBom(string plainPassword) {
    if (string.IsNullOrEmpty(plainPassword))
      return string.Empty;

    string valueToEncode = plainPassword + Suffix;

    Encoding encoding = Encoding.Unicode; // UTF-16 LE

    byte[] bom = encoding.GetPreamble();
    byte[] content = encoding.GetBytes(valueToEncode);

    byte[] bytes = new byte[bom.Length + content.Length];

    Buffer.BlockCopy(bom,0,bytes,0,bom.Length);
    Buffer.BlockCopy(content,0,bytes,bom.Length,content.Length);

    return Convert.ToBase64String(bytes);
  }

  public static string DecodePasswordUnicodeBom(string encodedPassword) {
    if (string.IsNullOrWhiteSpace(encodedPassword))
      return string.Empty;

    byte[] bytes = Convert.FromBase64String(encodedPassword);

    Encoding encoding = Encoding.Unicode; // UTF-16 LE

    int offset = 0;

    byte[] bom = encoding.GetPreamble();

    if (bytes.Length >= bom.Length &&
        bytes[0] == bom[0] &&
        bytes[1] == bom[1]) {
      offset = bom.Length;
    }

    string decoded = encoding.GetString(
        bytes,
        offset,
        bytes.Length - offset
    );

    if (decoded.EndsWith(Suffix,StringComparison.Ordinal))
      return decoded[..^Suffix.Length];

    return decoded;
  }

  public static string DecodePasswordUnicodeBomSafe(string encodedPassword) {
    if (string.IsNullOrWhiteSpace(encodedPassword))
      return string.Empty;

    try {
      return DecodePasswordUnicodeBom(encodedPassword);
    }
    catch {
      return encodedPassword;
    }
  }
}