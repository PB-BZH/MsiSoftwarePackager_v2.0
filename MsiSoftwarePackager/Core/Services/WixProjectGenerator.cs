using MsiSoftwarePackager.Core.Models;
using PB.BZH.Help.Library.Core.Services;

namespace MsiSoftwarePackager.Core.Services;

public sealed class WixProjectGenerator {
  public static void Generate(
    MsiPackageProfile profile,
    string templateRoot,
    string outputRoot) {

    Directory.CreateDirectory(outputRoot);

    Dictionary<string,string> values =
      BuildTemplateValues(profile);

    ConfigureLicenseTemplateValues(
      profile,
      templateRoot,
      outputRoot,
      values);

    GenerateFile(
      templateRoot,
      outputRoot,
      "Package.wxs.template",
      "Package.wxs",
      values);

    GenerateFile(
      templateRoot,
      outputRoot,
      "Folders.wxs.template",
      "Folders.wxs",
      values);

    GenerateFile(
      templateRoot,
      outputRoot,
      "Components.wxs.template",
      "Components.wxs",
      values);

    GenerateFile(
      templateRoot,
      outputRoot,
      "Package.fr-fr.wxl.template",
      "Package.fr-fr.wxl",
      values);

    GenerateFile(
      templateRoot,
      outputRoot,
      "msi-software-packager.wixproj.template",
      profile.Product.ProductName + ".wixproj",
      values);

    GenerateFile(
      templateRoot,
      outputRoot,
      "Bundle.wxs.template",
      "Bundle.wxs",
      values);

    if (profile.WebInstaller.BuildWebInstaller) {
      GenerateFile(
        templateRoot,
        outputRoot,
        "WebBundle.wxs.template",
        "WebBundle.wxs",
        values);
    }
  }

  private static void ConfigureLicenseTemplateValues(
  MsiPackageProfile profile,
  string templateRoot,
  string outputRoot,
  Dictionary<string,string> values) {

    values["MsiLicenseUiBlock"] = string.Empty;

    values["BundleBootstrapperApplicationBlock"] = """
<BootstrapperApplication>
  <bal:WixStandardBootstrapperApplication
    Theme="hyperlinkLicense"
    LicenseUrl="" />
</BootstrapperApplication>
""";

    if (!profile.License.RequireLicenseAcceptance) {
      return;
    }

    string licenseRtfSource =
      ResolveLicenseRtfSource(
        profile,
        templateRoot);

    if (!File.Exists(licenseRtfSource)) {
      throw new FileNotFoundException(
        "Le fichier de licence RTF est introuvable.",
        licenseRtfSource);
    }

    string licenseRtfOutput =
      Path.Combine(
        outputRoot,
        Path.GetFileName(licenseRtfSource));

    File.Copy(
      licenseRtfSource,
      licenseRtfOutput,
      overwrite: true);

    string licenseRtfPathXml =
      EscapeXmlAttribute(licenseRtfOutput);

    values["MsiLicenseUiBlock"] =
  $"""
<ui:WixUI
  Id="WixUI_InstallDir"
  InstallDirectory="INSTALLFOLDER" />

<WixVariable
  Id="WixUILicenseRtf"
  Value="{licenseRtfPathXml}" />
""";

    values["BundleBootstrapperApplicationBlock"] =
    $"""
<BootstrapperApplication>
  <bal:WixStandardBootstrapperApplication
    Theme="rtfLicense"
    LicenseFile="{licenseRtfPathXml}" />
</BootstrapperApplication>
""";
  }

  private static string ResolveLicenseRtfSource(
    MsiPackageProfile profile,
    string templateRoot) {

    string licenseFile =
      string.IsNullOrWhiteSpace(profile.License.LicenseRtfFile)
        ? "License.rtf"
        : profile.License.LicenseRtfFile.Trim();

    if (Path.IsPathRooted(licenseFile)) {
      return licenseFile;
    }

    return Path.Combine(
      templateRoot,
      licenseFile);
  }

  private static string EscapeXmlAttribute(
    string value) {

    return value
      .Replace("&","&amp;")
      .Replace("\"","&quot;")
      .Replace("<","&lt;")
      .Replace(">","&gt;");
  }

  private static void GenerateFile(
      string templateRoot,
      string outputRoot,
      string templateFile,
      string outputFile,
      Dictionary<string,string> values) {
    string templatePath = Path.Combine(templateRoot,templateFile);

    string template = File.ReadAllText(templatePath);

    string rendered = TemplateRenderer.Render(template,values);

    string outputPath = Path.Combine(outputRoot,outputFile);

    File.WriteAllText(outputPath,rendered);
  }

  private static Dictionary<string,string> BuildTemplateValues(MsiPackageProfile profile) {
    return new Dictionary<string,string> {
      ["ProductName"] = profile.Product.ProductName,
      ["ProductDisplayName"] = string.IsNullOrWhiteSpace(profile.Product.ProductName) ? "Product" : profile.Product.ProductName,
      ["WebProductFolder"] = string.IsNullOrWhiteSpace(profile.WebInstaller.WebProductFolder)
        ? UpdateSettingsHelper.GetSafeFileName(profile.Product.ProductName)
        : profile.WebInstaller.WebProductFolder,
      ["Manufacturer"] = profile.Product.Manufacturer,
      ["Version"] = profile.Product.Version,
      ["UpgradeCode"] = profile.Product.UpgradeCode,
      ["InstallFolderName"] = profile.Msi.InstallFolderName,
      ["ExecutableName"] = profile.TargetProject.ExecutableName,
      ["PublishDirectory"] = profile.Publish.PublishDirectory,
      ["ProgramFilesFolder"] = profile.Msi.ProgramFilesFolder,
      ["ShortcutName"] = profile.Shortcuts.ShortcutName,
      ["ComponentGroupId"] = "ApplicationComponents",
      ["IconPath"] = profile.Product.IconPath,
      ["StartMenuShortcut"] = BuildStartMenuShortcut(profile),
      ["DesktopShortcut"] = BuildDesktopShortcut(profile),
      ["PathEnvironment"] = BuildPathEnvironment(profile),
      ["Architecture"] = profile.Msi.Architecture,
      ["MsiOutputDirectory"] = profile.Output.MsiOutputDirectory,
      ["BundleName"] = string.IsNullOrWhiteSpace(profile.Bundle.BundleName)
        ? profile.Product.ProductName + " Setup"
        : profile.Bundle.BundleName,
      ["BundleUpgradeCode"] = profile.Bundle.BundleUpgradeCode,
      ["MsiPath"] = Path.Combine(
        profile.Output.MsiOutputDirectory,
        profile.Output.MsiFileName
        ),
      ["MsiFileName"] = profile.Output.MsiFileName,
      ["WebBundleName"] = string.IsNullOrWhiteSpace(profile.WebInstaller.WebBundleName)
        ? profile.Product.ProductName
        : profile.WebInstaller.WebBundleName,
      ["WebBundleUpgradeCode"] = profile.WebInstaller.WebBundleUpgradeCode,
      ["WebOutputDirectory"] = profile.WebInstaller.WebOutputDirectory,
      ["WebSetupFileName"] = profile.WebInstaller.WebSetupFileName,
      ["MsiDownloadUrl"] = profile.WebInstaller.MsiDownloadUrl,
    };
  }

  private static string BuildStartMenuShortcut(
      MsiPackageProfile profile) {
    if (!profile.Shortcuts.CreateStartMenuShortcut)
      return string.Empty;

    return
$"""
<Shortcut Id="StartMenuShortcut"
          Directory="ProgramMenuDir"
          Name="{profile.Shortcuts.ShortcutName}"
          WorkingDirectory="INSTALLFOLDER"
          Advertise="no"
          Icon="AppIcon.ico"
          Target="[INSTALLFOLDER]{profile.TargetProject.ExecutableName}" />
""";
  }

  private static string BuildDesktopShortcut(
      MsiPackageProfile profile) {
    if (!profile.Shortcuts.CreateDesktopShortcut)
      return string.Empty;

    return
$"""
<Shortcut Id="DesktopShortcut"
          Directory="DesktopFolder"
          Name="{profile.Shortcuts.ShortcutName}"
          WorkingDirectory="INSTALLFOLDER"
          Advertise="no"
          Icon="AppIcon.ico"
          Target="[INSTALLFOLDER]{profile.TargetProject.ExecutableName}" />
""";
  }

  private static string BuildPathEnvironment(
      MsiPackageProfile profile) {
    if (!profile.Environment.AddInstallFolderToPath)
      return string.Empty;

    return
"""
<Environment Id="PATH"
             Name="PATH"
             Action="set"
             Part="last"
             System="yes"
             Value="[INSTALLFOLDER]" />
""";
  }
}