namespace MsiSoftwarePackager.Core.Services;

internal class ToolPathResolver {
  private static string FindLatestSignToolPath() {
    string baseDir =
        @"C:\Program Files (x86)\Windows Kits\10\bin";

    if (!Directory.Exists(baseDir))
      return string.Empty;

    string[] candidates =
        Directory
            .GetFiles(baseDir,"signtool.exe",SearchOption.AllDirectories)
            .Where(path =>
                path.Contains(
                    Path.DirectorySeparatorChar + "x64" + Path.DirectorySeparatorChar,
                    StringComparison.OrdinalIgnoreCase
                ))
            .Select(Path.GetFullPath)
            .OrderByDescending(path => TryExtractWindowsKitVersion(path))
            .ToArray();

    return candidates.Length > 0
        ? candidates[0]
        : string.Empty;
  }

  private static Version TryExtractWindowsKitVersion(string signToolPath) {
    string? directory =
        Path.GetDirectoryName(signToolPath);

    if (string.IsNullOrWhiteSpace(directory))
      return new Version(0,0,0,0);

    DirectoryInfo x64Directory =
        new(directory);

    DirectoryInfo? versionDirectory =
        x64Directory.Parent;

    if (versionDirectory == null)
      return new Version(0,0,0,0);

    return Version.TryParse(versionDirectory.Name,out Version? version)
        ? version
        : new Version(0,0,0,0);
  }

  internal static string ResolveSignToolPath(string currentPath) {
    if (!string.IsNullOrWhiteSpace(currentPath) &&
        File.Exists(currentPath)) {
      return currentPath;
    }

    string detectedPath =
        FindLatestSignToolPath();

    return string.IsNullOrWhiteSpace(detectedPath)
        ? currentPath
        : detectedPath;
  }
}
