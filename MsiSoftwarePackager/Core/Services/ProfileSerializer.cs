using System.Text.Json;
using System.Text.Json.Serialization;
using MsiSoftwarePackager.Core.Models;

namespace MsiSoftwarePackager.Core.Services;

public static class ProfileSerializer {
  private static readonly JsonSerializerOptions Options = new() {
    WriteIndented = true,
    Converters =
      {
            new JsonStringEnumConverter()
        }
  };

  public static MsiPackageProfile FromJson(string json) {
    return JsonSerializer.Deserialize<MsiPackageProfile>(
        json,
        Options
    ) ?? new MsiPackageProfile();
  }

  public static string ToJson(MsiPackageProfile profile) {
    return JsonSerializer.Serialize(profile,Options);
  }

  public static void Save(string filePath,MsiPackageProfile profile) {
    string json = ToJson(profile);

    Directory.CreateDirectory(
        Path.GetDirectoryName(filePath)!
    );

    File.WriteAllText(filePath,json);
  }

  public static MsiPackageProfile Load(string filePath) {
    if (!File.Exists(filePath))
      return new MsiPackageProfile();

    string json = File.ReadAllText(filePath);

    return JsonSerializer.Deserialize<MsiPackageProfile>(
        json,
        Options
    ) ?? new MsiPackageProfile();
  }
}