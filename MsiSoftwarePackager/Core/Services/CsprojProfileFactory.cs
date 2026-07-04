using System.Xml.Linq;
using MsiSoftwarePackager.Core.Models;
using PB.BZH.Help.Library.Core.Services;

namespace MsiSoftwarePackager.Core.Services;

public static class CsprojProfileFactory {
  public static MsiPackageProfile CreateFromCsproj(string csprojPath) {
    if (string.IsNullOrWhiteSpace(csprojPath))
      throw new ArgumentException("Project file path is empty.",nameof(csprojPath));

    if (!File.Exists(csprojPath))
      throw new FileNotFoundException("Project file not found.",csprojPath);

    string projectDirectory =
        Path.GetDirectoryName(csprojPath)
        ?? throw new InvalidOperationException("Unable to get project directory.");

    string defaultName = Path.GetFileNameWithoutExtension(csprojPath);

    XDocument document = XDocument.Load(csprojPath);

    string assemblyName =
        ReadProperty(document,"AssemblyName")
        ?? defaultName;

    string productName =
        ReadProperty(document,"Product")
        ?? assemblyName;

    string productId =
        ReadProperty(document,"ProductId")
        ?? assemblyName;

    string productFolder =
        ReadProperty(document,"ProductFolder")
        ?? assemblyName;

    string version =
        ReadProperty(document,"Version")
        ?? ReadProperty(document,"PackageVersion")
        ?? ReadProperty(document,"VersionPrefix")
        ?? ReadProperty(document,"AssemblyVersion")
        ?? ReadProperty(document,"FileVersion")
        ?? ReadProperty(document,"InformationalVersion")
        ?? "1.0.0";

    string manufacturer =
        ReadProperty(document,"Company")
        ?? "PB BZH Concept";

    string description =
        ReadProperty(document,"Description")
        ?? string.Empty;

    string? iconPath =
        ResolveIconPath(projectDirectory,ReadProperty(document,"ApplicationIcon"));

    string upgradeCode =
        ReadProperty(document,"UpgradeCode")
        ?? Guid.NewGuid().ToString().ToUpperInvariant();

    string bundleUpgradeCode =
        ReadProperty(document,"BundleUpgradeCode")
        ?? Guid.NewGuid().ToString().ToUpperInvariant();

    string webBundleUpgradeCode =
        ReadProperty(document,"WebBundleUpgradeCode")
        ?? Guid.NewGuid().ToString().ToUpperInvariant();

    MsiPackageProfile profile = new();

    string safeName = UpdateSettingsHelper.GetSafeFileName(productName);

    profile.Product.ProductName = productName;
    profile.Product.ProductId = productId;
    profile.Product.ProductFolder = productFolder;
    profile.Product.Manufacturer = manufacturer;
    profile.Product.Version = NormalizeVersion(version);
    profile.Product.Description = description;
    profile.Product.UpgradeCode = upgradeCode;

    if (!string.IsNullOrWhiteSpace(iconPath))
      profile.Product.IconPath = iconPath;

    profile.TargetProject.ProjectFile = csprojPath;
    profile.TargetProject.ProjectDirectory = projectDirectory;
    profile.TargetProject.ExecutableName = assemblyName + ".exe";

    profile.Publish.Configuration = "Release";
    profile.Publish.RuntimeIdentifier = "win-x64";
    profile.Publish.SelfContained = true;
    profile.Publish.SingleFile = false;
    profile.Publish.ReadyToRun = true;
    profile.Publish.Trimmed = false;
    profile.Publish.PublishDirectory = @"E:\msi-software-packager\Build\Publish";

    profile.Msi.Architecture = "x64";
    profile.Msi.ProgramFilesFolder = "ProgramFiles6432Folder";
    profile.Msi.InstallFolderName = productName;
    profile.Msi.InstallPerMachine = true;
    profile.Msi.MajorUpgrade = true;
    profile.Msi.RequireAdministrator = true;

    profile.Shortcuts.CreateDesktopShortcut = true;
    profile.Shortcuts.CreateStartMenuShortcut = true;
    profile.Shortcuts.ShortcutName = productName;

    profile.Environment.AddInstallFolderToPath = true;
    profile.Environment.SystemPath = true;

    profile.Output.WixOutputDirectory = @"E:\msi-software-packager\Build\Wix";
    profile.Output.MsiOutputDirectory = @"E:\msi-software-packager\Build\Installer";
    profile.Output.MsiFileName = safeName + ".msi";

    profile.Bundle.BuildBundle = true;
    profile.Bundle.BundleUpgradeCode = bundleUpgradeCode;
    profile.Bundle.BundleOutputDirectory = @"E:\msi-software-packager\Build\Bundle";
    profile.Bundle.BundleFileName = safeName + "Setup.exe";
    profile.Bundle.BundleName = productName;

    profile.WebInstaller.BuildWebInstaller = true;
    profile.WebInstaller.WebBundleName = productName;
    profile.WebInstaller.WebOutputDirectory = @"E:\msi-software-packager\Build\Web";
    profile.WebInstaller.WebSetupFileName = safeName + "WebSetup.exe";
    profile.WebInstaller.WebBundleUpgradeCode = webBundleUpgradeCode;
    profile.WebInstaller.PrepareWebPublishFolder = true;
    profile.WebInstaller.WebPublishDirectory = @"C:\wamp64\www\softwares-site\public\softwares";

    profile.Upload.UploadWebFilesAfterBuild = true;
    profile.Upload.Protocol = "SFTP";
    profile.Upload.Host = "access-5020244126.webspace-host.com";
    profile.Upload.UserName = "su185260";
    profile.Upload.RemoteDirectory = "/";
    profile.Upload.LocalDirectory = @"C:\wamp64\www\softwares-site";
    profile.Upload.WinScpPath = @"C:\Program Files (x86)\WinSCP\WinSCP.com";
    profile.Upload.WebRemoteSite = "https://softwares.pb-bzh-concept.fr/softwares/";
    profile.Upload.Password = "";
    return profile;
  }

  private static string NormalizeVersion4(string version) {
    string normalized = NormalizeVersion(version);

    if (!Version.TryParse(normalized,out Version? parsed))
      return "1.0.0.0";

    int build = parsed.Build >= 0 ? parsed.Build : 0;

    return $"{parsed.Major}.{parsed.Minor}.{build}.0";
  }

  public static string UpdateCsprojFromProfile(MsiPackageProfile profile) {
    if (string.IsNullOrWhiteSpace(profile.TargetProject.ProjectFile))
      throw new InvalidOperationException("Project file path is empty.");

    string csprojPath = profile.TargetProject.ProjectFile;

    if (!File.Exists(csprojPath))
      throw new FileNotFoundException("Project file not found.",csprojPath);

    string projectDirectory =
        Path.GetDirectoryName(csprojPath)
        ?? throw new InvalidOperationException("Unable to get project directory.");

    // Sauvegarde de sécurité
    string backupPath = csprojPath + ".bak_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

    File.Copy(csprojPath,backupPath,overwrite: false);

    XDocument document = XDocument.Load(csprojPath);

    XElement propertyGroup = FindOrCreateMainPropertyGroup(document);

    SetPropertyInMainGroup(propertyGroup,"Product",profile.Product.ProductName);
    SetPropertyInMainGroup(propertyGroup,"ProductId",profile.Product.ProductId);
    SetPropertyInMainGroup(propertyGroup,"ProductFolder",profile.Product.ProductFolder);
    SetPropertyInMainGroup(propertyGroup,"Company",profile.Product.Manufacturer);
    SetPropertyInMainGroup(propertyGroup,"Version",NormalizeVersion(profile.Product.Version));
    SetPropertyInMainGroup(propertyGroup,"Description",profile.Product.Description);
    SetPropertyInMainGroup(propertyGroup,"UpgradeCode",profile.Product.UpgradeCode);
    SetPropertyInMainGroup(propertyGroup,"BundleUpgradeCode",profile.Bundle.BundleUpgradeCode);
    SetPropertyInMainGroup(propertyGroup,"WebBundleUpgradeCode",profile.WebInstaller.WebBundleUpgradeCode);


    string version3 = NormalizeVersion(profile.Product.Version);
    string version4 = NormalizeVersion4(profile.Product.Version);

    SetPropertyInMainGroup(propertyGroup,"AssemblyVersion",version4);
    SetPropertyInMainGroup(propertyGroup,"FileVersion",version4);
    SetPropertyInMainGroup(propertyGroup,"InformationalVersion",version3);


    if (!string.IsNullOrWhiteSpace(profile.Product.IconPath)) {
      string iconValue = MakeRelativePathIfPossible(
          projectDirectory,
          profile.Product.IconPath
      );

      SetPropertyInMainGroup(propertyGroup,"ApplicationIcon",iconValue);
    }

    document.Save(csprojPath);

    return backupPath;
  }

  private static void SetPropertyInMainGroup(
    XElement propertyGroup,
    string propertyName,
    string? value) {
    if (string.IsNullOrWhiteSpace(value))
      return;

    XElement? element =
        propertyGroup
            .Elements()
            .FirstOrDefault(e => e.Name.LocalName.Equals(
                propertyName,
                StringComparison.OrdinalIgnoreCase));

    if (element == null) {
      XName name = propertyGroup.Name.Namespace + propertyName;
      element = new XElement(name);
      propertyGroup.Add(element);
    }

    element.Value = value.Trim();
  }

  private static XElement FindOrCreateMainPropertyGroup(XDocument document) {
    XElement project =
        document.Root
        ?? throw new InvalidOperationException("Invalid project file.");

    XElement? propertyGroup =
        project
            .Elements()
            .FirstOrDefault(e =>
                e.Name.LocalName == "PropertyGroup" &&
                !e.Attributes().Any(a => a.Name.LocalName == "Condition"));

    if (propertyGroup != null)
      return propertyGroup;

    propertyGroup = new XElement(project.Name.Namespace + "PropertyGroup");

    project.AddFirst(propertyGroup);

    return propertyGroup;
  }

  private static string MakeRelativePathIfPossible(
    string baseDirectory,
    string fullPath) {
    if (string.IsNullOrWhiteSpace(fullPath))
      return string.Empty;

    if (!Path.IsPathRooted(fullPath))
      return fullPath;

    try {
      string relative = Path.GetRelativePath(baseDirectory,fullPath);

      if (!relative.StartsWith(".."))
        return relative;
    }
    catch {
      // fallback below
    }

    return fullPath;
  }

  private static string? ReadProperty(XDocument document,string propertyName) {
    return document
        .Descendants()
        .Where(e => e.Name.LocalName == propertyName)
        .Select(e => e.Value.Trim())
        .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
  }

  private static string? ResolveIconPath(string projectDirectory,string? iconValue) {
    if (string.IsNullOrWhiteSpace(iconValue))
      return null;

    string iconPath = iconValue.Trim();

    if (Path.IsPathRooted(iconPath))
      return File.Exists(iconPath) ? iconPath : null;

    string fullPath = Path.GetFullPath(
        Path.Combine(projectDirectory,iconPath)
    );

    return File.Exists(fullPath) ? fullPath : null;
  }

  private static string NormalizeVersion(string version) {
    if (string.IsNullOrWhiteSpace(version))
      return "1.0.0";

    string clean = version.Trim();

    int dashIndex = clean.IndexOf('-');

    if (dashIndex >= 0)
      clean = clean[..dashIndex];

    if (!Version.TryParse(clean,out Version? parsed))
      return "1.0.0";

    // MSI accepte généralement Major.Minor.Build.
    // On évite donc le format 1.2.5.0 si possible.
    if (parsed.Build >= 0)
      return $"{parsed.Major}.{parsed.Minor}.{parsed.Build}";

    return $"{parsed.Major}.{parsed.Minor}.0";
  }
}