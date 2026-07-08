using System.Text.Json.Serialization;

namespace MsiSoftwarePackager.Core.Models;


public sealed class MsiPackageProfile {
  public ProductOptions Product { get; set; } = new();
  public TargetProjectOptions TargetProject { get; set; } = new();
  public PublishOptions Publish { get; set; } = new();
  public MsiOptions Msi { get; set; } = new();
  public ShortcutOptions Shortcuts { get; set; } = new();
  public EnvironmentOptions Environment { get; set; } = new();
  public OutputOptions Output { get; set; } = new();
  public BundleOptions Bundle { get; set; } = new();
  public WebInstallerOptions WebInstaller { get; set; } = new();
  public AndroidPackageOptions Android { get; set; } = new();
  public UploadOptions Upload { get; set; } = new();
  public UpdateManifest UpdateManifest { get; set; } = new();
  public SigningOptions Signing { get; set; } = new();
  public SigningLicense License { get; set; } = new();
}

public sealed class AndroidPackageOptions {
  public bool PublishApk { get; set; } = false;
  public string ApkFilePath { get; set; } = "";
  public string Configuration { get; set; } = "Release";
  public string TargetFramework { get; set; } = "net10.0-android";
  public string KeystoreFile { get; set; } = "";
  public string KeyAlias { get; set; } = "";
  public string KeystorePasswordCredentialTarget { get; set; } = "";
  public string KeyPasswordCredentialTarget { get; set; } = "";
}

public sealed class SigningLicense {
  public bool RequireLicenseAcceptance { get; set; } = false;
  public string LicenseRtfFile { get; set; } = "License.rtf";
  public bool TechnicalLicenseRequired { get; set; } = false;
}

public sealed class UploadOptions {
  public bool UploadWebFilesAfterBuild { get; set; } = false;
  public string Protocol { get; set; } = "SFTP";
  public string Host { get; set; } = "access-5020244126.webspace-host.com";
  public string UserName { get; set; } = "su185260";
  public string RemoteDirectory { get; set; } = "/";
  public string LocalDirectory { get; set; } = @"C:\wamp64\www\pb-bzh-concept.fr";
  public string WinScpPath { get; set; } = @"C:\Program Files (x86)\WinSCP\WinSCP.com";
  public string WebRemoteSite { get; set; } = "https://www.pb-bzh-concept.fr/";
  public string Password { get; set; } = "";
  public string CredentialTarget { get; set; } = "";
}

public sealed class ProductOptions {
  public string ProductName { get; set; } = "Msi Software Packager";
  public string ProductId { get; set; } = "MsiSoftwarePackager";
  public string ProductFolder { get; set; } = "MsiSoftwarePackager";
  public string Manufacturer { get; set; } = "PB BZH Concept";
  public string Version { get; set; } = "1.0.0";
  public string Description { get; set; } = "MSI Builder, upload WebSetup and Msi package";
  public string UpgradeCode { get; set; } = "";
  public string IconPath { get; set; } = "";
  [JsonIgnore]
  public Image? LogoImage { get; set; } = Properties.Resources.Application;
  public string DownloadPageUrl { get; set; } = "https://www.pb-bzh-concept.fr";
  public string PrivacyPageUrl { get; set; } = "https://www.pb-bzh-concept.fr/privacy.php";
  public string Copyright { get; set; } = "© Copyright PB BZH Concept 2026";
  public string EmailContact { get; set; } = "admin@pb-bzh-concept.fr";
}

public sealed class TargetProjectOptions {
  public string ProjectFile { get; set; } = "";
  public string ExecutableName { get; set; } = "";
  public string ProjectDirectory { get; set; } = "";
}

public sealed class PublishOptions {
  public string Configuration { get; set; } = "Release";
  public string RuntimeIdentifier { get; set; } = "win-x64";
  public bool SelfContained { get; set; } = true;
  public bool SingleFile { get; set; } = false;
  public bool ReadyToRun { get; set; } = true;
  public bool Trimmed { get; set; } = false;
  public string PublishDirectory { get; set; } = "Build\\Publish";
}

public sealed class MsiOptions {
  public string Architecture { get; set; } = "x64";
  public string ProgramFilesFolder { get; set; } = "ProgramFiles6432Folder";
  public string InstallFolderName { get; set; } = "MyApplication";
  public bool InstallPerMachine { get; set; } = true;
  public bool MajorUpgrade { get; set; } = true;
  public bool RequireAdministrator { get; set; } = true;
}

public sealed class ShortcutOptions {
  public bool CreateDesktopShortcut { get; set; } = true;
  public bool CreateStartMenuShortcut { get; set; } = true;
  public string ShortcutName { get; set; } = "MyApplication";
}

public sealed class EnvironmentOptions {
  public bool AddInstallFolderToPath { get; set; } = false;
  public bool SystemPath { get; set; } = true;
}

public sealed class OutputOptions {
  public string WixOutputDirectory { get; set; } = "Build\\Wix";
  public string MsiOutputDirectory { get; set; } = "Build\\Msi";
  public string MsiFileName { get; set; } = "MyApplication.msi";
}

public sealed class WebInstallerOptions {
  public bool BuildWebInstaller { get; set; } = false;
  public string WebBundleName { get; set; } = "";
  public string WebOutputDirectory { get; set; } = "Build\\Web";
  public string WebSetupFileName { get; set; } = "WebSetup.exe";
  public string MsiDownloadUrl { get; set; } = "";
  public string WebBundleUpgradeCode { get; set; } = "";
  public bool PrepareWebPublishFolder { get; set; } = true;
  public string WebPublishDirectory { get; set; } = @"C:\wamp64\www\pb-bzh-concept.fr\";
  public string WebProductFolder { get; set; } = "";
}


public sealed class BundleOptions {
  public bool BuildBundle { get; set; } = false;
  public string BundleUpgradeCode { get; set; } = "";
  public string BundleOutputDirectory { get; set; } = "Build\\Bundle";
  public string BundleFileName { get; set; } = "Setup.exe";
  public string BundleName { get; set; } = "";
}

public sealed class UpdateManifest {
  public string ProductName { get; set; } = "Msi Software Packager";
  public string Version { get; set; } = "";
  public string Publisher { get; set; } = "";
  public string DownloadPage { get; set; } = "https://www.pb-bzh-concept.fr";
  public string PrivacyPage { get; set; } = "https://www.pb-bzh-concept.fr/softwares/privacy.php";
  public string MsiUrl { get; set; } = "";
  public string WebSetupUrl { get; set; } = "";
  public string UpdateManifestUrl { get; set; } = "";
  public string ReleaseDate { get; set; } = "";
  public string ApplicationId { get; set; } = "";
}

public sealed class SigningOptions {
  public const string DefaultTimestampUrl = "http://ts.ssl.com";
  public const string DefaultSslComCodeSignToolPath = @"E:\msi-software-packager\SSL\CodeSignTool.bat";
  public const string DefaultCertificateThumbprint = "f43bc96bb830b10478fddefd92405f2766d3a339";
  public string TimestampUrl { get; set; } = DefaultTimestampUrl;
  public bool SignArtifacts { get; set; } = false;
  public string SignToolPath { get; set; } = @"C:\Program Files (x86)\Windows Kits\10\bin\x64\signtool.exe";
  public string CertificateThumbprint { get; set; } = DefaultCertificateThumbprint;
  public bool SignMsi { get; set; } = true;
  public bool SignBundle { get; set; } = true;
  public bool SignWebSetup { get; set; } = true;
  public bool SignExecutable { get; set; } = true;
  public string SigningProvider { get; set; } = "SignToolCka";
  public string SslComCodeSignToolPath { get; set; } = DefaultSslComCodeSignToolPath;
  public string SslComCredentialId { get; set; } = string.Empty;
  public string SslComLogin { get; set; } = "PB BZH Concept";
  public string SslComCredentialTarget { get; set; } = string.Empty;
  public string SslComTotpSecretCredentialTarget { get; set; } = string.Empty;
}

public sealed class GlobalSettings {
  public GlobalUploadSettings Upload { get; set; } = new();
  public GlobalSigningSettings Signing { get; set; } = new();
  public GlobalToolsSettings Tools { get; set; } = new();
  public GlobalBuildPathSettings BuildPaths { get; set; } = new();

}

public sealed class GlobalUploadSettings {
  public string Protocol { get; set; } = "SFTP";
  public string Host { get; set; } = "access-5020244126.webspace-host.com";
  public string UserName { get; set; } = "su185260";
  public string RemoteDirectory { get; set; } = "/";
  public string LocalDirectory { get; set; } = @"C:\wamp64\www\pb-bzh-concept.fr\";
  public string WebRemoteSite { get; set; } = "https://www.pb-bzh-concept.fr/softwares/";
  public string CredentialTarget { get; set; } = string.Empty;
}

public sealed class GlobalSigningSettings {
  public const string DefaultTimestampUrl = "http://ts.ssl.com";
  public const string DefaultSslComCodeSignToolPath = @"E:\msi-software-packager\SSL\CodeSignTool.bat";
  public const string DefaultCertificateThumbprint = "f43bc96bb830b10478fddefd92405f2766d3a339";
  public string TimestampUrl { get; set; } = DefaultTimestampUrl;
  public string SignToolPath { get; set; } = string.Empty;
  public string CertificateThumbprint { get; set; } = DefaultCertificateThumbprint;
  public string SigningProvider { get; set; } = "SslComCodeSignTool";
  public string SslComCodeSignToolPath { get; set; } = DefaultSslComCodeSignToolPath;
  public string SslComCredentialId { get; set; } = "5e46ad1b-3756-455c-bb2d-b6d4738e6238";
  public string SslComLogin { get; set; } = "PB BZH Concept";
  public string SslComCredentialTarget { get; set; } = string.Empty;
  public string SslComTotpSecretCredentialTarget { get; set; } = string.Empty;
}

public sealed class GlobalToolsSettings {
  public string WinScpPath { get; set; } = @"C:\Program Files (x86)\WinSCP\WinSCP.com";
  public string NotepadPlusPlusPath { get; set; } = @"C:\Program Files\Notepad++\notepad++.exe";
}

public sealed class GlobalBuildPathSettings {
  public string BuildRoot { get; set; } = @"E:\msi-software-packager\Build";
}
