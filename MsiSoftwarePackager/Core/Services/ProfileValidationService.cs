using MsiSoftwarePackager.Core.Models;

namespace MsiSoftwarePackager.Core.Services;

public sealed class ProfileValidationResult {
  public List<string> Errors { get; } = [];
  public List<string> Warnings { get; } = [];
  public List<string> Infos { get; } = [];

  public bool HasErrors => Errors.Count > 0;
  public bool HasWarnings => Warnings.Count > 0;
  public bool IsValid => !HasErrors;
}

public static class ProfileValidationService {
  public static ProfileValidationResult Validate(
      MsiPackageProfile profile,
      string templateRoot) {
    ProfileValidationResult result = new();

    ValidateProduct(profile,result);
    ValidateProject(profile,result);
    ValidateOutput(profile,result);
    ValidateTemplates(templateRoot,result);
    ValidateAssets(templateRoot,result);
    ValidateWeb(profile,result);
    ValidateAndroid(profile,result);
    ValidateUpload(profile,result);

    return result;
  }

  private static void ValidateAndroid(
    MsiPackageProfile profile,
    ProfileValidationResult result) {
    if (!profile.Android.PublishApk)
      return;

    if (string.IsNullOrWhiteSpace(profile.Android.ApkFilePath)) {
      result.Errors.Add("APK file path is empty.");
      return;
    }

    if (!profile.Android.ApkFilePath.EndsWith(".apk",StringComparison.OrdinalIgnoreCase))
      result.Errors.Add("APK file must end with .apk.");

    if (!File.Exists(profile.Android.ApkFilePath))
      result.Errors.Add("APK file not found : " + profile.Android.ApkFilePath);
  }

  private static void ValidateAssets(
    string templateRoot,
    ProfileValidationResult result) {
    string? applicationRoot =
        Directory.GetParent(templateRoot)?.FullName;

    if (string.IsNullOrWhiteSpace(applicationRoot)) {
      result.Warnings.Add(
          "Unable to resolve application root from template root : " +
          templateRoot
      );

      return;
    }

    string assetsRoot =
        Path.Combine(applicationRoot,"Assets");

    string pbLogo =
        Path.Combine(assetsRoot,"pb-bzh-logo.png");

    string softwareIcon =
        Path.Combine(assetsRoot,"software-icon.png");

    if (!File.Exists(pbLogo))
      result.Warnings.Add("PB-BZH logo not found : " + pbLogo);

    if (!File.Exists(softwareIcon))
      result.Warnings.Add("Software icon not found : " + softwareIcon);
  }

  private static void ValidateProduct(
      MsiPackageProfile profile,
      ProfileValidationResult result) {
    if (string.IsNullOrWhiteSpace(profile.Product.ProductName))
      result.Errors.Add("Product name is empty.");

    if (string.IsNullOrWhiteSpace(profile.Product.Manufacturer))
      result.Errors.Add("Manufacturer is empty.");

    if (string.IsNullOrWhiteSpace(profile.Product.Version)) {
      result.Errors.Add("Version is empty.");
    }
    else if (!Version.TryParse(profile.Product.Version,out _)) {
      result.Errors.Add("Version is invalid : " + profile.Product.Version);
    }

    if (!Guid.TryParse(profile.Product.UpgradeCode,out _))
      result.Errors.Add("UpgradeCode is invalid.");

    if (!string.IsNullOrWhiteSpace(profile.Product.IconPath) &&
        !File.Exists(profile.Product.IconPath)) {
      result.Warnings.Add("Icon file not found : " + profile.Product.IconPath);
    }
  }

  private static void ValidateProject(
      MsiPackageProfile profile,
      ProfileValidationResult result) {
    if (string.IsNullOrWhiteSpace(profile.TargetProject.ProjectFile)) {
      result.Errors.Add("Project file is empty.");
    }
    else if (!File.Exists(profile.TargetProject.ProjectFile)) {
      result.Errors.Add("Project file not found : " + profile.TargetProject.ProjectFile);
    }

    if (string.IsNullOrWhiteSpace(profile.TargetProject.ExecutableName))
      result.Errors.Add("Executable name is empty.");

    if (!profile.TargetProject.ExecutableName.EndsWith(".exe",StringComparison.OrdinalIgnoreCase))
      result.Warnings.Add("Executable name does not end with .exe.");
  }

  private static void ValidateOutput(
      MsiPackageProfile profile,
      ProfileValidationResult result) {
    if (string.IsNullOrWhiteSpace(profile.Output.WixOutputDirectory))
      result.Errors.Add("WiX output directory is empty.");

    if (string.IsNullOrWhiteSpace(profile.Output.MsiOutputDirectory))
      result.Errors.Add("MSI output directory is empty.");

    if (string.IsNullOrWhiteSpace(profile.Output.MsiFileName))
      result.Errors.Add("MSI file name is empty.");

    if (!profile.Output.MsiFileName.EndsWith(".msi",StringComparison.OrdinalIgnoreCase))
      result.Errors.Add("MSI file name must end with .msi.");
  }

  private static void ValidateTemplates(
      string templateRoot,
      ProfileValidationResult result) {
    string[] requiredTemplates =
    [
        "Package.wxs.template",
        "Folders.wxs.template",
        "Components.wxs.template",
        "Package.fr-fr.wxl.template",
        "Bundle.wxs.template",
        "WebBundle.wxs.template",
        Path.Combine("Web", "catalog.index.php.template"),
        Path.Combine("Web", "msi-software-packager.index.php.template"),
        Path.Combine("Web", "download.php.template")
    ];

    foreach (string relativePath in requiredTemplates) {
      string fullPath =
          Path.Combine(templateRoot,relativePath);

      if (!File.Exists(fullPath))
        result.Errors.Add("Template not found : " + fullPath);
    }
  }

  private static void ValidateWeb(
      MsiPackageProfile profile,
      ProfileValidationResult result) {
    if (!profile.WebInstaller.BuildWebInstaller)
      return;

    if (string.IsNullOrWhiteSpace(profile.WebInstaller.WebSetupFileName))
      result.Errors.Add("Web setup file name is empty.");

    if (!profile.WebInstaller.WebSetupFileName.EndsWith(".exe",StringComparison.OrdinalIgnoreCase))
      result.Errors.Add("Web setup file name must end with .exe.");

    if (string.IsNullOrWhiteSpace(profile.WebInstaller.MsiDownloadUrl)) {
      result.Errors.Add("MSI download URL is empty.");
    }
    else if (!Uri.TryCreate(profile.WebInstaller.MsiDownloadUrl,UriKind.Absolute,out Uri? uri) ||
             uri.Scheme is not ("http" or "https")) {
      result.Errors.Add("MSI download URL is invalid : " + profile.WebInstaller.MsiDownloadUrl);
    }

    if (!Guid.TryParse(profile.WebInstaller.WebBundleUpgradeCode,out _))
      result.Errors.Add("WebBundleUpgradeCode is invalid.");

    if (string.IsNullOrWhiteSpace(profile.WebInstaller.WebPublishDirectory))
      result.Warnings.Add("Web publish directory is empty.");
  }

  private static void ValidateUpload(
      MsiPackageProfile profile,
      ProfileValidationResult result) {
    if (!profile.Upload.UploadWebFilesAfterBuild)
      return;

    if (string.IsNullOrWhiteSpace(profile.Upload.Protocol))
      result.Errors.Add("Upload protocol is empty.");

    if (string.IsNullOrWhiteSpace(profile.Upload.Host))
      result.Errors.Add("Upload host is empty.");

    if (string.IsNullOrWhiteSpace(profile.Upload.UserName))
      result.Errors.Add("Upload user name is empty.");

    if (string.IsNullOrWhiteSpace(profile.Upload.LocalDirectory))
      result.Errors.Add("Upload local directory is empty.");

    if (!string.IsNullOrWhiteSpace(profile.Upload.LocalDirectory) &&
        !Directory.Exists(profile.Upload.LocalDirectory)) {
      result.Warnings.Add("Upload local directory not found : " + profile.Upload.LocalDirectory);
    }

    if (string.IsNullOrWhiteSpace(profile.Upload.WinScpPath)) {
      result.Warnings.Add("WinSCP path is empty.");
    }
    else if (!File.Exists(profile.Upload.WinScpPath)) {
      result.Warnings.Add("WinSCP not found : " + profile.Upload.WinScpPath);
    }

    if (string.IsNullOrWhiteSpace(profile.Upload.WebRemoteSite)) {
      result.Warnings.Add("Web remote site is empty.");
    }
    else if (!Uri.TryCreate(profile.Upload.WebRemoteSite,UriKind.Absolute,out _)) {
      result.Warnings.Add("Web remote site URL is invalid : " + profile.Upload.WebRemoteSite);
    }
  }
}