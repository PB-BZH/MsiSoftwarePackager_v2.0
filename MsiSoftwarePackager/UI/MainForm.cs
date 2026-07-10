using System.Diagnostics;
using System.Drawing.Printing;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MsiSoftwarePackager.Core.Models;
using MsiSoftwarePackager.Core.Services;
using MsiSoftwarePackager.UI.Forms;
using PB.BZH.Help.Library.Core.Services;
using PB.BZH.Licensing.Core.Services;
using PB.BZH.Licensing.UI.Theming;

namespace MsiSoftwarePackager.UI;

public partial class MainForm: Form {
  private MsiPackageProfile _profile = new();
  private string? _currentProfilePath;
  private bool _isLoadingProfile;
  private readonly Dictionary<string,RichTextBox> _previewEditors = [];
  private readonly Dictionary<RichTextBox,string> _editorFileNames = [];
  private string _buildLogPath = string.Empty;
  private readonly object _buildLogLock = new();
  private DateTime _buildStartTime;
  private bool _buildFileLoggingEnabled = false;
  private string _lastUploadScriptPath = string.Empty;
  private string _lastUploadLogPath = string.Empty;
  private bool _lastUploadSuccess = false;
  private const string WebCatalogFileName = "index.php";
  private const string WebAssetsFolderName = "assets";
  private const string WebCategoryFolderName = "msi-software-packager";
  private bool _isUpdatingMsiDownloadUrl;
  private PrintDocument? _printDocument;
  private string[] _printLines = [];
  private int _currentPrintLine;
  private static readonly JsonSerializerOptions JsonIndentedOptions = new() {
    WriteIndented = true
  };
  private static readonly JsonSerializerOptions JsonCaseInsensitiveOptions = new() {
    PropertyNameCaseInsensitive = true
  };

  private GlobalSettings _globalSettings = new();
  private bool _profileHasUnsavedChanges;
  private LicenseService _licenseService;
  public AndroidPackageOptions AndroidOptions { get; private set; }

  public MainForm() {
    InitializeComponent();
    _globalSettings = GlobalSettingsService.Load();
    _licenseService = LicenseHelper.CreerLicenseService(_profile);
    InitializePreviewTabs();
    txtLog.ContextMenuStrip = CreateEditorContextMenu(txtLog);
    ThemeManager.ApplyDarkTheme(this);
    ThemeManager.StyleDarkButtons(this);
    ThemeManager.ApplyDarkDialogBorder(this,pnlMain);
    ThemeManager.ApplyDarkTitleBar(this);
    chkBuildBundle.CheckedChanged += (_,_) => UpdateBundleUiState();
    chkBuildWebInstaller.CheckedChanged += (_,_) => UpdateWebInstallerUiState();
    UpdateWebInstallerUiState();
    HookUiEvents();
    UpdateBundleUiState();

  }

  //  public SigningOptions Signing { get; private set; }

  private enum FileSignatureStatus {
    Missing,
    NotSigned,
    SignedAndTrusted,
    SignedButNotTrusted,
    VerificationError
  }

  private FileSignatureStatus GetFileSignatureStatus(string filePath) {
    if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
      return FileSignatureStatus.Missing;

    string signToolPath =
        ToolPathResolver.ResolveSignToolPath(
            _profile.Signing.SignToolPath
        );

    if (string.IsNullOrWhiteSpace(signToolPath) || !File.Exists(signToolPath))
      return FileSignatureStatus.VerificationError;

    ProcessStartInfo psi = new() {
      FileName = signToolPath,
      Arguments = "verify /pa /v \"" + filePath + "\"",
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true,
      StandardOutputEncoding = Encoding.UTF8,
      StandardErrorEncoding = Encoding.UTF8
    };

    using Process process = new() {
      StartInfo = psi
    };

    process.Start();

    string output =
        process.StandardOutput.ReadToEnd() +
        Environment.NewLine +
        process.StandardError.ReadToEnd();

    process.WaitForExit();

    if (process.ExitCode == 0)
      return FileSignatureStatus.SignedAndTrusted;

    if (output.Contains("No signature found",StringComparison.OrdinalIgnoreCase))
      return FileSignatureStatus.NotSigned;

    if (output.Contains("Signature Index:",StringComparison.OrdinalIgnoreCase) ||
        output.Contains("Signing Certificate Chain:",StringComparison.OrdinalIgnoreCase) ||
        output.Contains("The signature is timestamped",StringComparison.OrdinalIgnoreCase)) {
      return FileSignatureStatus.SignedButNotTrusted;
    }

    return FileSignatureStatus.VerificationError;
  }

  private void ShowReleaseSummary() {
    try {
      string report =
          BuildReleaseSummaryText();

      AppendLog(report);

      if (_previewEditors.TryGetValue("Release Summary",out RichTextBox? editor)) {
        SetPlainPreviewText(editor,report);
        ApplyPreviewSyntaxHighlighting(editor,"Release Summary");

        foreach (TabPage tab in tabPreview.TabPages) {
          if (tab.Text.Equals("Release Summary",StringComparison.OrdinalIgnoreCase)) {
            tabPreview.SelectedTab = tab;
            break;
          }
        }
      }
    }
    catch (Exception ex) {
      AppendLog("[ERROR] Release summary failed : " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Release summary",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private static void AppendCertificateStatus(
    StringBuilder report,
    ref bool hasWarning,
    string label,
    string thumbprint) {
    CertificateStatus status =
        CodeSigningCertificateService.GetCertificateStatus(thumbprint);

    if (!status.IsConfigured) {
      report.AppendLine("[INFO] " + label + " certificate thumbprint is empty.");
      return;
    }

    report.AppendLine(
        "[OK] " + label + " certificate thumbprint : " +
        status.Thumbprint
    );

    if (!status.IsFound) {
      report.AppendLine(
          "[WARN] " + label +
          " certificate not found in CurrentUser\\My or LocalMachine\\My."
      );

      hasWarning = true;
      return;
    }

    report.AppendLine(
        "[OK] " + label + " certificate found in " +
        status.StoreLocation +
        " : " +
        status.Subject
    );

    if (status.NotAfter == null)
      return;

    string expiration =
        status.NotAfter.Value.ToString("yyyy-MM-dd HH:mm:ss");

    if (status.IsExpired) {
      report.AppendLine(
          "[WARN] " + label + " certificate is expired since : " +
          expiration
      );

      hasWarning = true;
    }
    else if (status.ExpiresSoon) {
      report.AppendLine(
          "[WARN] " + label + " certificate expires soon : " +
          expiration
      );

      hasWarning = true;
    }
    else {
      report.AppendLine(
          "[OK] " + label + " certificate valid until : " +
          expiration
      );
    }
  }

  private void CheckGlobalSettings(
    StringBuilder report,
    ref bool hasWarning) {
    report.AppendLine();
    report.AppendLine("Global settings");

    string globalSettingsPath =
        GlobalSettingsService.GetSettingsPath();

    if (File.Exists(globalSettingsPath)) {
      report.AppendLine(
          "[OK] Global settings file : " +
          globalSettingsPath
      );
    }
    else {
      report.AppendLine(
          "[WARN] Global settings file not found : " +
          globalSettingsPath
      );

      hasWarning = true;
    }

    string buildRoot =
        _globalSettings.BuildPaths.BuildRoot;

    if (string.IsNullOrWhiteSpace(buildRoot)) {
      report.AppendLine("[WARN] Global build root is empty.");
      hasWarning = true;
    }
    else {
      report.AppendLine(
          "[OK] Global build root : " +
          Path.GetFullPath(buildRoot)
      );
    }

    if (string.IsNullOrWhiteSpace(_globalSettings.Upload.Host)) {
      report.AppendLine("[WARN] Global upload host is empty.");
      hasWarning = true;
    }
    else {
      report.AppendLine(
          "[OK] Global upload host : " +
          _globalSettings.Upload.Host
      );
    }

    if (string.IsNullOrWhiteSpace(_globalSettings.Upload.UserName)) {
      report.AppendLine("[WARN] Global upload user name is empty.");
      hasWarning = true;
    }
    else {
      report.AppendLine(
          "[OK] Global upload user name : " +
          _globalSettings.Upload.UserName
      );
    }

    if (string.IsNullOrWhiteSpace(_globalSettings.Upload.CredentialTarget)) {
      report.AppendLine("[WARN] Global credential target is empty.");
      hasWarning = true;
    }
    else {
      report.AppendLine(
          "[OK] Global credential target : " +
          _globalSettings.Upload.CredentialTarget
      );

      string password =
          WindowsCredentialManager.ReadPassword(
              _globalSettings.Upload.CredentialTarget
          );

      if (string.IsNullOrWhiteSpace(password)) {
        report.AppendLine(
            "[WARN] Global upload password was not found in Windows Credential Manager."
        );

        hasWarning = true;
      }
      else {
        report.AppendLine(
            "[OK] Global upload password found in Windows Credential Manager."
        );
      }
    }

    if (string.IsNullOrWhiteSpace(_globalSettings.Upload.WebRemoteSite)) {
      report.AppendLine("[WARN] Global web remote site is empty.");
      hasWarning = true;
    }
    else {
      report.AppendLine(
          "[OK] Global web remote site : " +
          _globalSettings.Upload.WebRemoteSite
      );
    }

    if (string.IsNullOrWhiteSpace(_globalSettings.Tools.WinScpPath)) {
      report.AppendLine("[WARN] Global WinSCP path is empty.");
      hasWarning = true;
    }
    else if (File.Exists(_globalSettings.Tools.WinScpPath)) {
      report.AppendLine(
          "[OK] Global WinSCP path : " +
          _globalSettings.Tools.WinScpPath
      );
    }
    else {
      report.AppendLine(
          "[WARN] Global WinSCP path not found : " +
          _globalSettings.Tools.WinScpPath
      );

      hasWarning = true;
    }

    if (string.IsNullOrWhiteSpace(_globalSettings.Signing.SignToolPath)) {
      report.AppendLine("[WARN] Global SignTool path is empty.");
      hasWarning = true;
    }
    else if (File.Exists(_globalSettings.Signing.SignToolPath)) {
      report.AppendLine(
          "[OK] Global SignTool path : " +
          _globalSettings.Signing.SignToolPath
      );
    }
    else {
      report.AppendLine(
          "[WARN] Global SignTool path not found : " +
          _globalSettings.Signing.SignToolPath
      );

      hasWarning = true;
    }

    if (string.IsNullOrWhiteSpace(_globalSettings.Signing.CertificateThumbprint)) {
      report.AppendLine("[INFO] Global certificate thumbprint is empty.");
    }
    else {
      AppendCertificateStatus(
          report,
          ref hasWarning,
          "Global",
          _globalSettings.Signing.CertificateThumbprint
      );
    }

    if (string.IsNullOrWhiteSpace(_globalSettings.Signing.TimestampUrl)) {
      report.AppendLine("[WARN] Global timestamp URL is empty.");
      hasWarning = true;
    }
    else {
      report.AppendLine(
          "[OK] Global timestamp URL : " +
          _globalSettings.Signing.TimestampUrl
      );
    }
  }

  private void MarkProfileDirty() {
    if (_isLoadingProfile)
      return;

    _profileHasUnsavedChanges = true;

    AutoSaveProfile();
  }

  private void AskConfigureSigningForNewProfile() {
    bool hasSigningDefaults =
        !string.IsNullOrWhiteSpace(_profile.Signing.SignToolPath) &&
        !string.IsNullOrWhiteSpace(_profile.Signing.CertificateThumbprint) &&
        !string.IsNullOrWhiteSpace(_profile.Signing.TimestampUrl);

    string message = hasSigningDefaults
        ? "Code signing defaults are already configured. Do you want to review or enable code signing for this profile?"
        : "Do you want to configure code signing for this profile?";

    DialogResult result = MessageBox.Show(
        this,
        message,
        "Code signing",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    );

    if (result != DialogResult.Yes)
      return;

    using CodeSigningSettingsForm dialog =
        new(_profile.Signing);

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    _profile.Signing = dialog.Signing;

    ApplyProfileToUi();
    RefreshPreviewTabs();

    _profileHasUnsavedChanges = true;

    AutoSaveProfile();

    AppendLog("[INFO] Code signing settings configured for current profile.");
  }

  private void ApplyGlobalSettingsToProfileDefaults() {
    string buildRoot = Path.GetFullPath(_globalSettings.BuildPaths.BuildRoot);

    _profile.Output.WixOutputDirectory = Path.Combine(buildRoot,"Wix");
    _profile.Publish.PublishDirectory = Path.Combine(buildRoot,"Publish");
    _profile.Output.MsiOutputDirectory = Path.Combine(buildRoot,"Installer");
    _profile.Bundle.BundleOutputDirectory = Path.Combine(buildRoot,"Bundle");
    _profile.WebInstaller.WebOutputDirectory = Path.Combine(buildRoot,"Web");

    // Upload defaults
    _profile.Upload.Protocol = _globalSettings.Upload.Protocol;
    _profile.Upload.Host = _globalSettings.Upload.Host;
    _profile.Upload.UserName = _globalSettings.Upload.UserName;
    _profile.Upload.RemoteDirectory = _globalSettings.Upload.RemoteDirectory;
    _profile.Upload.LocalDirectory = _globalSettings.Upload.LocalDirectory;
    _profile.Upload.WebRemoteSite = _globalSettings.Upload.WebRemoteSite;
    _profile.Upload.WinScpPath = _globalSettings.Tools.WinScpPath;

    string uploadCredentialTarget = _globalSettings.Upload.CredentialTarget;

    if (string.IsNullOrWhiteSpace(uploadCredentialTarget)) {
      uploadCredentialTarget =
          WindowsCredentialManager.BuildUploadTargetName(
              _profile.Upload.Protocol,
              _profile.Upload.Host,
              _profile.Upload.UserName
          );
    }

    _profile.Upload.CredentialTarget = uploadCredentialTarget;
    _profile.Upload.Password = string.Empty;

    // Signing defaults
    _profile.Signing.SignToolPath = ToolPathResolver.ResolveSignToolPath(_globalSettings.Signing.SignToolPath);
    _profile.Signing.CertificateThumbprint = string.IsNullOrWhiteSpace(_globalSettings.Signing.CertificateThumbprint)
            ? SigningOptions.DefaultCertificateThumbprint
            : _globalSettings.Signing.CertificateThumbprint.Trim();

    _profile.Signing.TimestampUrl = string.IsNullOrWhiteSpace(_globalSettings.Signing.TimestampUrl)
            ? SigningOptions.DefaultTimestampUrl
            : _globalSettings.Signing.TimestampUrl.Trim();

    _profile.Signing.SigningProvider = string.IsNullOrWhiteSpace(_globalSettings.Signing.SigningProvider)
            ? "SignToolCka"
            : _globalSettings.Signing.SigningProvider;

    _profile.Signing.SslComCodeSignToolPath =
        string.IsNullOrWhiteSpace(_globalSettings.Signing.SslComCodeSignToolPath)
            ? SigningOptions.DefaultSslComCodeSignToolPath
            : _globalSettings.Signing.SslComCodeSignToolPath.Trim();

    _profile.Signing.SslComCredentialId = _globalSettings.Signing.SslComCredentialId;
    _profile.Signing.SslComLogin = _globalSettings.Signing.SslComLogin;

    string sslComCredentialTarget = _globalSettings.Signing.SslComCredentialTarget;

    if (string.IsNullOrWhiteSpace(sslComCredentialTarget) &&
        !string.IsNullOrWhiteSpace(_profile.Signing.SslComLogin)) {
      sslComCredentialTarget = WindowsCredentialManager.BuildSslComSigningTargetName(_profile.Signing.SslComLogin);
    }

    _profile.Signing.SslComCredentialTarget = sslComCredentialTarget;

    string sslComTotpSecretCredentialTarget = _globalSettings.Signing.SslComTotpSecretCredentialTarget;

    if (string.IsNullOrWhiteSpace(sslComTotpSecretCredentialTarget) &&
        !string.IsNullOrWhiteSpace(_profile.Signing.SslComLogin)) {
      sslComTotpSecretCredentialTarget = WindowsCredentialManager.BuildSslComTotpSecretTargetName(_profile.Signing.SslComLogin);
    }

    _profile.Signing.SslComTotpSecretCredentialTarget = sslComTotpSecretCredentialTarget;
  }

  private object BuildUpdateManifest() {
    SyncUpdateManifestFromProfile();

    if (_profile.Android.PublishApk &&
        !_profile.WebInstaller.BuildWebInstaller &&
        !_profile.Bundle.BuildBundle) {
      return BuildAndroidUpdateManifest();
    }

    return _profile.UpdateManifest;
  }

  private void WriteUpdateManifest(string projectPublishDir) {
    object manifest = BuildUpdateManifest();

    string json =
        JsonSerializer.Serialize(
            manifest,
            JsonIndentedOptions
        );

    string manifestPath =
        Path.Combine(
            projectPublishDir,
            "update.json"
        );

    File.WriteAllText(
        manifestPath,
        json,
        EncodingHelper.Utf8NoBom
    );

    AppendLog("[OK] Update manifest generated : " + manifestPath);
  }

  private object BuildAndroidUpdateManifest() {
    string apkUrl = BuildApkUrl();

    return new {
      ProductName = _profile.Product.ProductName,
      Version = _profile.Product.Version,
      Publisher = _profile.Product.Manufacturer,
      DownloadPage = BuildDownloadPageUrl(),
      PrivacyPage = _profile.Product.PrivacyPageUrl,
      ApkUrl = apkUrl,
      UpdateManifestUrl = BuildUpdateManifestUrl(),
      ReleaseDate = DateTime.Now.ToString("yyyy-MM-dd"),
      ApplicationId = GetWebProductFolderName(),
      TargetFramework = _profile.Android.TargetFramework
    };
  }

  private string BuildApkUrl() {
    if (!_profile.Android.PublishApk ||
        string.IsNullOrWhiteSpace(_profile.Android.ApkFilePath))
      return string.Empty;

    string webRemoteSite =
        _profile.Upload.WebRemoteSite.Trim().TrimEnd('/');

    return webRemoteSite +
        "/" +
        Uri.EscapeDataString(WebCategoryFolderName) +
        "/" +
        Uri.EscapeDataString(GetWebProductFolderName()) +
        "/" +
        Uri.EscapeDataString(Path.GetFileName(_profile.Android.ApkFilePath));
  }

  private bool ValidateWebOnlyBeforeAction() {
    List<string> errors = [];

    if (string.IsNullOrWhiteSpace(_profile.WebInstaller.WebPublishDirectory))
      errors.Add("Web publish directory is required.");

    if (string.IsNullOrWhiteSpace(_profile.Upload.LocalDirectory))
      errors.Add("Upload local directory is required.");

    if (string.IsNullOrWhiteSpace(_profile.Upload.WebRemoteSite))
      errors.Add("Web remote site is required.");

    ValidateRequiredTemplates(errors);
    ValidateWebAssets(errors);
    ValidateMsiDownloadUrlConsistency(errors);

    if (errors.Count == 0) {
      AppendLog("[VALIDATION OK] Web site validation successful.");
      return true;
    }

    AppendLog("[WEB VALIDATION FAILED]");

    foreach (string error in errors)
      AppendLog("[ERROR] " + error);

    MessageBox.Show(
        this,
        string.Join(Environment.NewLine + Environment.NewLine,errors),
        "Web site validation",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning
    );

    return false;
  }

  private void ValidateMsiDownloadUrlConsistency(List<string> errors) {
    if (!_profile.WebInstaller.BuildWebInstaller)
      return;

    string expectedUrl = BuildMsiDownloadUrl();

    if (string.IsNullOrWhiteSpace(expectedUrl)) {
      errors.Add("Unable to compute expected MSI download URL.");
      return;
    }

    if (!string.Equals(
            _profile.WebInstaller.MsiDownloadUrl,
            expectedUrl,
            StringComparison.OrdinalIgnoreCase)) {
      errors.Add(
          "MSI download URL is not consistent with current profile." +
          Environment.NewLine +
          "Expected : " + expectedUrl +
          Environment.NewLine +
          "Current  : " + _profile.WebInstaller.MsiDownloadUrl
      );
    }
  }

  private void ValidateUploadPassword(List<string> errors) {
    if (!_profile.Upload.UploadWebFilesAfterBuild)
      return;

    string password = GetUploadPassword();

    if (string.IsNullOrWhiteSpace(password)) {
      errors.Add(
          "Upload password is empty or cannot be retrieved from Windows Credential Manager."
      );
    }

    if (string.IsNullOrWhiteSpace(_profile.Upload.CredentialTarget) &&
        string.IsNullOrWhiteSpace(_profile.Upload.Password)) {
      errors.Add(
          "No upload credential is configured. Save the upload password to Windows Credential Manager."
      );
    }
  }

  private static void ValidateWebAssets(List<string> errors) {
    string assetsRoot = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Assets"
    );

    string pbLogo = Path.Combine(
        assetsRoot,
        "pb-bzh-logo.png"
    );

    string softwareIcon = Path.Combine(
        assetsRoot,
        "software-icon.png"
    );

    if (!File.Exists(pbLogo))
      errors.Add("PB-BZH logo asset not found : " + pbLogo);

    if (!File.Exists(softwareIcon))
      errors.Add("Software icon asset not found : " + softwareIcon);
  }

  private static void ValidateRequiredTemplates(List<string> errors) {
    string templateRoot = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Templates"
    );

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
      string fullPath = Path.Combine(templateRoot,relativePath);

      if (!File.Exists(fullPath))
        errors.Add("Template file not found : " + fullPath);
    }
  }

  private void UiFieldChangedRefreshPreview(object? sender,EventArgs e) {
    RefreshProfileAndPreviewFromUi();
  }

  private void RefreshProfileAndPreviewFromUi() {
    if (_isLoadingProfile)
      return;

    if (_isUpdatingMsiDownloadUrl)
      return;

    MarkProfileDirty();

    ApplyUiToProfile();

    RefreshMsiDownloadUrl();

    ApplyUiToProfile();

    RefreshPreviewTabs();
  }

  private bool TryGetSelectedPreviewEditor(out RichTextBox editor) {
    editor = null!;

    if (tabPreview.SelectedTab == null)
      return false;

    string key = tabPreview.SelectedTab.Text;

    return _previewEditors.TryGetValue(key,out editor!);
  }

  private static string? FindNotepadPlusPlus() {
    string[] candidates =
    [
        @"C:\Program Files\Notepad++\notepad++.exe",
        @"C:\Program Files (x86)\Notepad++\notepad++.exe"
    ];

    foreach (string path in candidates) {
      if (File.Exists(path))
        return path;
    }

    return null;
  }

  private void PrintDocument_PrintPage(object sender,PrintPageEventArgs e) {
    if (e.Graphics == null) {
      e.HasMorePages = false;
      return;
    }

    using Font printFont = new("Consolas",9);

    float left = e.MarginBounds.Left;
    float top = e.MarginBounds.Top;
    float lineHeight = printFont.GetHeight(e.Graphics);

    int linesPerPage = (int)(e.MarginBounds.Height / lineHeight);
    int count = 0;

    while (count < linesPerPage && _currentPrintLine < _printLines.Length) {
      e.Graphics.DrawString(
          _printLines[_currentPrintLine],
          printFont,
          Brushes.Black,
          left,
          top + count * lineHeight
      );

      count++;
      _currentPrintLine++;
    }

    e.HasMorePages = _currentPrintLine < _printLines.Length;
  }

  private void RefreshMsiDownloadUrl() {
    if (_isLoadingProfile)
      return;

    if (_isUpdatingMsiDownloadUrl)
      return;

    try {
      _isUpdatingMsiDownloadUrl = true;


      txtMsiDownloadUrl.Text = BuildMsiDownloadUrl();
      _profile.WebInstaller.MsiDownloadUrl = BuildMsiDownloadUrl();


      UpdateToolTips();
    }
    finally {
      _isUpdatingMsiDownloadUrl = false;
    }
  }

  private static string LoadWebTemplate(string templateFileName) {
    string templatePath =
        Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Templates",
            "Web",
            templateFileName
        );

    if (!File.Exists(templatePath)) {
      return
          "WEB TEMPLATE NOT FOUND:" +
          Environment.NewLine +
          templatePath;
    }

    return File.ReadAllText(
        templatePath,
        Encoding.UTF8
    );
  }

  private string ApplyWebTemplateValues(string content) {

    string assetVersion =
    string.IsNullOrWhiteSpace(_profile.Product.Version)
        ? "1.0.0"
        : Uri.EscapeDataString(_profile.Product.Version);

    return content
        .Replace(
            "{{CATALOG_TITLE}}",
            "PB-BZH Concept - Softwares"
        )
        .Replace(
            "{{CATALOG_SUBTITLE}}",
            "Catalogue des logiciels disponibles."
        )
        .Replace(
            "{{CATEGORY_TITLE}}",
            "msi-software-packager"
        )
        .Replace(
            "{{CATEGORY_SUBTITLE}}",
            "Packages MSI, bundles offline et web installers."
        )
        .Replace(
            "{{ASSET_VERSION}}",
            assetVersion
        );
  }

  private string BuildProductBaseUrl() {
    string webRemoteSite =
        _profile.Upload.WebRemoteSite
            .Trim()
            .TrimEnd('/');

    return
        webRemoteSite +
        "/" +
        Uri.EscapeDataString(WebCategoryFolderName) +
        "/" +
        Uri.EscapeDataString(GetWebProductFolderName());
  }

  private string BuildUpdateManifestUrl() {
    return BuildProductBaseUrl() + "/update.json";
  }

  private string BuildMsiDownloadUrl() {
    return
        BuildProductBaseUrl() +
        "/" +
        Uri.EscapeDataString(_profile.Output.MsiFileName);
  }

  private string GetWebProductFolderName() {
    if (!string.IsNullOrWhiteSpace(_profile.WebInstaller.WebProductFolder))
      return _profile.WebInstaller.WebProductFolder.Trim();

    return UpdateSettingsHelper.GetSafeFileName(_profile.Product.ProductName);
  }

  private string BuildWebSetupUrl() {
    return
        BuildProductBaseUrl() +
        "/" +
        Uri.EscapeDataString(_profile.WebInstaller.WebSetupFileName);
  }

  private string BuildDownloadPageUrl() {
    string webRemoteSite =
        _profile.Upload.WebRemoteSite
            .Trim()
            .TrimEnd('/');

    return
        webRemoteSite +
        "/download.php?category=" +
        Uri.EscapeDataString(WebCategoryFolderName) +
        "&product=" +
        Uri.EscapeDataString(GetWebProductFolderName());
  }

  private bool ValidateUploadOptions() {
    ApplyUiToProfile();

    List<string> errors = [];

    if (!_profile.Upload.UploadWebFilesAfterBuild)
      return true;

    if (string.IsNullOrWhiteSpace(_profile.Upload.Protocol))
      errors.Add("Upload protocol is required.");

    if (string.IsNullOrWhiteSpace(_profile.Upload.Host))
      errors.Add("Upload host is required.");

    if (string.IsNullOrWhiteSpace(_profile.Upload.UserName))
      errors.Add("Upload user name is required.");

    if (string.IsNullOrWhiteSpace(GetUploadPassword()))
      errors.Add("Upload password is required or cannot be retrieved.");

    if (string.IsNullOrWhiteSpace(_profile.Upload.RemoteDirectory))
      errors.Add("Upload remote directory is required.");

    if (string.IsNullOrWhiteSpace(_profile.Upload.LocalDirectory))
      errors.Add("Upload local directory is required.");

    if (!string.IsNullOrWhiteSpace(_profile.Upload.LocalDirectory) &&
        !Directory.Exists(_profile.Upload.LocalDirectory))
      errors.Add("Upload local directory does not exist.");

    if (string.IsNullOrWhiteSpace(_profile.Upload.WinScpPath))
      errors.Add("WinSCP path is required.");

    if (!string.IsNullOrWhiteSpace(_profile.Upload.WinScpPath) &&
        !File.Exists(_profile.Upload.WinScpPath))
      errors.Add("WinSCP.com was not found.");

    if (_profile.Upload.UploadWebFilesAfterBuild) {
      string localSiteRoot =
          Path.GetFullPath(
              _profile.Upload.LocalDirectory
          );

      string localAssetsDir =
          Path.Combine(
              localSiteRoot,
              "public",
              "softwares",
              WebAssetsFolderName
          );

      if (!Directory.Exists(localAssetsDir)) {
        errors.Add(
            "Upload assets directory does not exist: " + localAssetsDir
        );
      }
    }

    if (errors.Count == 0)
      return true;

    AppendLog("[UPLOAD VALIDATION FAILED]");

    foreach (string error in errors)
      AppendLog("[ERROR] " + error);

    MessageBox.Show(
        string.Join(Environment.NewLine,errors),
        "Upload validation",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning
    );

    return false;
  }

  private static string GetWinScpProtocol(string protocol) {
    return protocol.Trim().ToUpperInvariant() switch {
      "FTP" => "ftp",
      "FTPS" => "ftpes",
      "SFTP" => "sftp",
      _ => "sftp"
    };
  }

  private void SaveUploadPasswordToCredentialManager() {
    ApplyUiToProfile();

    string password =
        txtUploadPassword.Text;

    if (string.IsNullOrWhiteSpace(password)) {
      MessageBox.Show(
          this,
          "Upload password is empty.",
          "Save upload password",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );

      return;
    }

    string credentialTarget =
        WindowsCredentialManager.BuildTargetName(
            _profile.Upload.Protocol,
            _profile.Upload.Host,
            _profile.Upload.UserName
        );

    WindowsCredentialManager.SavePassword(
        credentialTarget,
        _profile.Upload.UserName,
        password
    );

    _profile.Upload.CredentialTarget = credentialTarget;
    _profile.Upload.Password = "";

    AutoSaveProfile();

    AppendLog("[OK] Upload password saved to Windows Credential Manager.");
  }

  private string GenerateWinScpUploadScript() {
    ApplyUiToProfile();

    string buildRoot =
        Path.GetDirectoryName(
            _profile.Output.MsiOutputDirectory
        )!;

    string uploadDir =
        Path.Combine(buildRoot,"Upload");

    Directory.CreateDirectory(uploadDir);

    string scriptPath =
        Path.Combine(uploadDir,"winscp-upload.txt");

    string protocol =
        GetWinScpProtocol(
            _profile.Upload.Protocol
        );

    if (!string.Equals(protocol,"sftp",StringComparison.OrdinalIgnoreCase)) {
      throw new InvalidOperationException(
          "Remote directory creation and cleanup require SFTP."
      );
    }

    string host =
        _profile.Upload.Host.Trim();

    string userName =
        _profile.Upload.UserName.Trim();

    string password =
        GetUploadPassword();

    if (string.IsNullOrWhiteSpace(password)) {
      throw new InvalidOperationException(
          "Upload password is empty or cannot be retrieved."
      );
    }

    string escapedUserName =
        Uri.EscapeDataString(userName);

    string escapedPassword =
        Uri.EscapeDataString(password);

    string openCommand =
        $"{protocol}://{escapedUserName}:{escapedPassword}@{host}/";

    string localSiteRoot =
        Path.GetFullPath(
            _profile.Upload.LocalDirectory
        ).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        );

    string remoteSiteRoot =
        _profile.Upload.RemoteDirectory
            .Trim()
            .Replace("\\","/")
            .TrimEnd('/');

    if (string.IsNullOrWhiteSpace(remoteSiteRoot)) {
      remoteSiteRoot = "/";
    }

    if (!remoteSiteRoot.StartsWith('/')) {
      remoteSiteRoot = "/" + remoteSiteRoot;
    }

    string productFolder =
        GetWebProductFolderName();

    string localPublicDir =
        Path.Combine(
            localSiteRoot,
            "public"
        );

    string localSoftwaresDir =
        Path.Combine(
            localPublicDir,
            "softwares"
        );

    string localAssetsDir =
        Path.Combine(
            localSoftwaresDir,
            WebAssetsFolderName
        );

    string localMsiSoftwarePackagerDir =
        Path.Combine(
            localSoftwaresDir,
            "msi-software-packager"
        );

    string localProjectDir =
        Path.Combine(
            localMsiSoftwarePackagerDir,
            productFolder
        );

    string remotePublicDir =
        CombineRemotePath(
            remoteSiteRoot,
            "public"
        );

    string remoteSoftwaresDir =
        CombineRemotePath(
            remotePublicDir,
            "softwares"
        );

    string remoteAssetsDir =
        CombineRemotePath(
            remoteSoftwaresDir,
            WebAssetsFolderName
        );

    string remoteMsiSoftwarePackagerDir =
        CombineRemotePath(
            remoteSoftwaresDir,
            "msi-software-packager"
        );

    string remoteProjectDir =
        CombineRemotePath(
            remoteMsiSoftwarePackagerDir,
            productFolder
        );
    // Sécurité minimale : on refuse les chemins dangereux.
    if (!string.Equals(remoteSoftwaresDir,"/public/softwares",StringComparison.OrdinalIgnoreCase)) {
      throw new InvalidOperationException(
          "Unsafe remote softwares directory: " + remoteSoftwaresDir
      );
    }

    if (string.IsNullOrWhiteSpace(productFolder) ||
        productFolder.Contains("..") ||
        productFolder.Contains('/') ||
        productFolder.Contains('\\')) {
      throw new InvalidOperationException(
          "Unsafe product folder name: " + productFolder
      );
    }

    StringBuilder script = new();

    script.AppendLine("option batch abort");
    script.AppendLine("option confirm off");
    script.AppendLine();

    script.AppendLine($"open \"{openCommand}\"");
    script.AppendLine();

    script.AppendLine("# ==================================================");
    script.AppendLine("# Bootstrap remote tree");
    script.AppendLine("# Creates missing remote directories without mkdir/call");
    script.AppendLine("# ==================================================");

    script.AppendLine($"synchronize remote \"{localPublicDir}\" \"{remotePublicDir}\"");
    script.AppendLine();

    script.AppendLine("# ==================================================");
    script.AppendLine("# Upload /public/softwares/index.php");
    script.AppendLine("# ==================================================");
    script.AppendLine($"cd \"{remoteSoftwaresDir}\"");
    script.AppendLine($"lcd \"{localSoftwaresDir}\"");
    script.AppendLine($"put \"{WebCatalogFileName}\"");
    script.AppendLine();

    script.AppendLine("# ==================================================");
    script.AppendLine("# Synchronize assets");
    script.AppendLine("# ==================================================");
    script.AppendLine(
        $"synchronize remote -delete \"{localAssetsDir}\" \"{remoteAssetsDir}\""
    );
    script.AppendLine();

    script.AppendLine("# ==================================================");
    script.AppendLine("# Upload /public/softwares/msi-software-packager/index.php");
    script.AppendLine("# ==================================================");
    script.AppendLine($"cd \"{remoteMsiSoftwarePackagerDir}\"");
    script.AppendLine($"lcd \"{localMsiSoftwarePackagerDir}\"");
    script.AppendLine($"put \"{WebCatalogFileName}\"");
    script.AppendLine();

    script.AppendLine("# ==================================================");
    script.AppendLine("# Synchronize current project files");
    script.AppendLine("# ==================================================");
    script.AppendLine(
        $"synchronize remote -delete \"{localProjectDir}\" \"{remoteProjectDir}\""
    );
    script.AppendLine();

    script.AppendLine("exit");

    File.WriteAllText(
        scriptPath,
        script.ToString(),
        EncodingHelper.Utf8Bom
    );

    return scriptPath;
  }

  private static string CombineRemotePath(string root,string child) {
    string cleanRoot =
        string.IsNullOrWhiteSpace(root)
            ? "/"
            : root.Trim().Replace("\\","/").TrimEnd('/');

    string cleanChild =
        child.Trim().Replace("\\","/").Trim('/');

    if (cleanRoot == "")
      cleanRoot = "/";

    if (cleanRoot == "/")
      return "/" + cleanChild;

    return cleanRoot + "/" + cleanChild;
  }

  private string GetUploadPassword() {
    // 1. Priorité au Gestionnaire d'identifiants Windows
    if (!string.IsNullOrWhiteSpace(_profile.Upload.CredentialTarget)) {
      string credentialPassword =
          WindowsCredentialManager.ReadPassword(
              _profile.Upload.CredentialTarget
          );

      if (!string.IsNullOrEmpty(credentialPassword))
        return credentialPassword;
    }

    // 2. Fallback : ancien stockage JSON encodé Unicode BOM
    if (!string.IsNullOrWhiteSpace(_profile.Upload.Password)) {
      string decodedPassword =
          PasswordCodec.DecodePasswordUnicodeBomSafe(
              _profile.Upload.Password
          );

      if (!string.IsNullOrEmpty(decodedPassword))
        return decodedPassword;
    }

    // 3. Dernier fallback : champ UI courant
    return txtUploadPassword.Text;
  }

  private void RestoreDefaultWinScpPath() {
    const string defaultWinScpPath =
        @"C:\Program Files (x86)\WinSCP\WinSCP.com";

    if (!File.Exists(defaultWinScpPath)) {
      MessageBox.Show(
          this,
          "Le chemin WinSCP par défaut est introuvable :" +
          Environment.NewLine +
          defaultWinScpPath,
          "Restore WinSCP path",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );

      return;
    }

    txtWinScpPath.Text = defaultWinScpPath;
    _profile.Upload.WinScpPath = defaultWinScpPath;

    ApplyUiToProfile();
    RefreshPreviewTabs();

    AppendLog("[INFO] WinSCP path restored manually : " + defaultWinScpPath);
  }

  private void RunWinScpUpload(bool forceUpload = false) {

    if (_isLoadingProfile)
      return;

    if (_isUpdatingMsiDownloadUrl)
      return;

    ApplyUiToProfile();

    RefreshMsiDownloadUrl();

    ApplyUiToProfile();

    RefreshPreviewTabs();

    if (!_profile.Upload.UploadWebFilesAfterBuild && !forceUpload)
      return;

    if (!ValidateUploadOptions()) {
      throw new InvalidOperationException(
          "Upload validation failed. Build process stopped."
      );
    }

    string scriptPath = GenerateWinScpUploadScript();

    _lastUploadScriptPath = scriptPath;
    _lastUploadSuccess = false;

    string buildRoot = Path.GetDirectoryName(_profile.Output.MsiOutputDirectory)!;

    string uploadLogDir = Path.Combine(buildRoot,"Logs");

    Directory.CreateDirectory(uploadLogDir);

    string uploadLogPath = Path.Combine(uploadLogDir,$"winscp-upload_{DateTime.Now:yyyyMMdd_HHmmss}.log");

    _lastUploadLogPath = uploadLogPath;

    AppendLog("[INFO] Upload script generated : " + scriptPath);
    AppendLog("[INFO] Upload started...");

    ProcessStartInfo psi = new() {
      FileName = _profile.Upload.WinScpPath,
      Arguments =
            $"/script=\"{scriptPath}\" " +
            $"/log=\"{uploadLogPath}\"",
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true,
      StandardOutputEncoding = Encoding.UTF8,
      StandardErrorEncoding = Encoding.UTF8
    };

    using Process process = new() {
      StartInfo = psi
    };

    process.OutputDataReceived += (_,e) => {
      if (!string.IsNullOrWhiteSpace(e.Data))
        AppendLog("[WinSCP] " + e.Data);
    };

    process.ErrorDataReceived += (_,e) => {
      if (!string.IsNullOrWhiteSpace(e.Data))
        AppendLog("[WinSCP ERROR] " + e.Data);
    };

    process.Start();

    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    process.WaitForExit();

    if (process.ExitCode != 0) {
      throw new InvalidOperationException(
          "WinSCP upload failed. Exit code: " +
          process.ExitCode
      );
    }

    AppendLog("[SUCCESS] Upload completed.");
    AppendLog("[INFO] Upload log saved : " + uploadLogPath);
    _lastUploadSuccess = true;

    SaveMaskedWinScpScriptAndDeleteOriginal(scriptPath);
  }

  private void SaveMaskedWinScpScriptAndDeleteOriginal(string scriptPath) {
    try {
      if (!File.Exists(scriptPath)) {
        AppendLog("[INFO] Original WinSCP script not found after upload: " + scriptPath);
        return;
      }

      string scriptText = File.ReadAllText(scriptPath,EncodingHelper.Utf8Bom);

      string maskedScriptText = MaskWinScpPasswordInScript(scriptText);

      string directory =
          Path.GetDirectoryName(scriptPath)
          ?? AppDomain.CurrentDomain.BaseDirectory;

      string maskedScriptPath = Path.Combine(
          directory,
          "winscp-upload.masked.txt"
      );

      File.WriteAllText(maskedScriptPath,maskedScriptText,EncodingHelper.Utf8Bom);

      File.Delete(scriptPath);

      AppendLog("[INFO] Masked WinSCP script saved : " + maskedScriptPath);
      AppendLog("[INFO] Original WinSCP script deleted.");
    }
    catch (Exception ex) {
      AppendLog("[WARN] Unable to mask/delete WinSCP script : " + ex.Message);
    }
  }

  private static string MaskWinScpPasswordInScript(string scriptText) {
    if (string.IsNullOrWhiteSpace(scriptText))
      return string.Empty;

    return OpenSftpPasswordRegex().Replace(scriptText,"$1********$3");
  }

  private void txtMsiDownloadUrl_TextChanged(object? sender,EventArgs e) {
    toolTip.SetToolTip(
        txtMsiDownloadUrl,
        txtMsiDownloadUrl.Text
    );
  }

  private void HookUiEvents() {
    chkBuildBundle.CheckedChanged += (_,_) => {
      UpdateBundleUiState();
      UiFieldChangedRefreshPreview(null,EventArgs.Empty);
    };

    chkBuildWebInstaller.CheckedChanged += (_,_) => {
      UpdateWebInstallerUiState();
      UiFieldChangedRefreshPreview(null,EventArgs.Empty);
    };

    chkUploadWebFilesAfterBuild.CheckedChanged += (_,_) => {
      UpdateWebUploadUiState();
      UiFieldChangedRefreshPreview(null,EventArgs.Empty);
    };

    txtMsiDownloadUrl.TextChanged += txtMsiDownloadUrl_TextChanged;

    txtWebRemoteSite.TextChanged += UiFieldChangedRefreshPreview;
    txtProductName.TextChanged += UiFieldChangedRefreshPreview;
    txtExecutableName.TextChanged += UiFieldChangedRefreshPreview;

    txtManufacturer.TextChanged += UiFieldChangedRefreshPreview;
    txtVersion.TextChanged += UiFieldChangedRefreshPreview;
    txtUpgradeCode.TextChanged += UiFieldChangedRefreshPreview;
    txtIconPath.TextChanged += UiFieldChangedRefreshPreview;

    txtProjectFile.TextChanged += UiFieldChangedRefreshPreview;
    txtProductName.TextChanged += txtProductName_TextChanged;
  }

  private void txtProductName_TextChanged(object? sender,EventArgs e) {
    if (_isLoadingProfile)
      return;

    SyncGeneratedNamesFromProductName();

    ApplyUiToProfile();
    RefreshMsiDownloadUrl();
    RefreshPreviewTabs();
  }

  private void SyncUpdateManifestFromProfile() {
    _profile.UpdateManifest.ProductName = _profile.Product.ProductName;
    _profile.UpdateManifest.Version = _profile.Product.Version;
    _profile.UpdateManifest.Publisher = _profile.Product.Manufacturer;
    _profile.UpdateManifest.DownloadPage = BuildDownloadPageUrl();
    _profile.UpdateManifest.MsiUrl = BuildMsiDownloadUrl();
    _profile.UpdateManifest.WebSetupUrl = BuildWebSetupUrl();
    _profile.UpdateManifest.UpdateManifestUrl = BuildUpdateManifestUrl();
    _profile.UpdateManifest.ApplicationId = GetWebProductFolderName();
    _profile.UpdateManifest.ReleaseDate = DateTime.Now.ToString("yyyy-MM-dd");
  }

  private void SyncGeneratedNamesFromProductName() {
    string productName = txtProductName.Text.Trim();
    if (string.IsNullOrWhiteSpace(productName))
      return;
    string safeName = UpdateSettingsHelper.GetSafeFileName(productName);
    txtExecutableName.Text = safeName + ".exe";
    txtBundleName.Text = productName;
    txtBundleFileName.Text = safeName + "Setup.exe";
    txtWebBundleName.Text = productName;
    txtWebSetupFileName.Text = safeName + "WebSetup.exe";
    if (string.IsNullOrWhiteSpace(txtWebProductFolder.Text)) {
      txtWebProductFolder.Text = safeName;
    }
    _profile.Output.MsiFileName = safeName + ".msi";
  }

  private string GetBundlePath() {
    string bundleDir =
        Path.GetFullPath(_profile.Bundle.BundleOutputDirectory);

    return Path.Combine(
        bundleDir,
        _profile.Bundle.BundleFileName
    );
  }

  private void IncrementVersion(VersionIncrementMode mode) {
    if (!Version.TryParse(txtVersion.Text.Trim(),out Version? version)) {
      MessageBox.Show(
          "Invalid version format. Expected format: 1.0.0",
          "Version",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );

      return;
    }

    int major = version.Major;
    int minor = version.Minor;
    int patch = version.Build >= 0 ? version.Build : 0;

    switch (mode) {
      case VersionIncrementMode.Patch:
        patch++;
        break;

      case VersionIncrementMode.Minor:
        minor++;
        patch = 0;
        break;

      case VersionIncrementMode.Major:
        major++;
        minor = 0;
        patch = 0;
        break;
    }

    txtVersion.Text =
        $"{major}.{minor}.{patch}";

    ApplyUiToProfile();

    AppendLog("[INFO] Version updated to " + txtVersion.Text);
    AutoSaveProfile();
  }

  private enum VersionIncrementMode { Patch, Minor, Major }

  private void btnVersionPatch_Click(object? sender,EventArgs e) {
    IncrementVersion(VersionIncrementMode.Patch);
  }

  private void btnVersionMinor_Click(object? sender,EventArgs e) {
    IncrementVersion(VersionIncrementMode.Minor);
  }

  private void btnVersionMajor_Click(object? sender,EventArgs e) {
    IncrementVersion(VersionIncrementMode.Major);
  }

  private static string FormatFileSize(long bytes) {
    string[] units =
    [
        "B",
        "KB",
        "MB",
        "GB",
        "TB"
    ];

    double size = bytes;
    int unitIndex = 0;

    while (size >= 1024 && unitIndex < units.Length - 1) {
      size /= 1024;
      unitIndex++;
    }

    return $"{size:0.##} {units[unitIndex]}";
  }

  private void SaveMsiBuildReport(bool success,string message) {
    string reportDir = Path.Combine(
        Path.GetDirectoryName(_profile.Output.MsiOutputDirectory)!,
        "Reports"
    );

    Directory.CreateDirectory(reportDir);

    string reportPath = Path.Combine(
        reportDir,
        $"MSI_BUILD_REPORT_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
    );

    string msiPath = Path.Combine(
        _profile.Output.MsiOutputDirectory,
        _profile.Output.MsiFileName
    );

    StringBuilder report = new(); report.AppendLine("==================================================");
    report.AppendLine("MsiSoftwarePackager Build Report");
    report.AppendLine("==================================================");
    report.AppendLine();

    report.AppendLine($"Date     : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    report.AppendLine($"Duration : {(DateTime.Now - _buildStartTime):hh\\:mm\\:ss}");
    report.AppendLine();

    report.AppendLine("==================================================");
    report.AppendLine("MSI IDENTITY");
    report.AppendLine("==================================================");
    report.AppendLine($"Product      : {_profile.Product.ProductName}");
    report.AppendLine($"Version      : {_profile.Product.Version}");
    report.AppendLine($"Manufacturer : {_profile.Product.Manufacturer}");
    report.AppendLine($"UpgradeCode  : {_profile.Product.UpgradeCode}");
    report.AppendLine($"MajorUpgrade : {(_profile.Msi.MajorUpgrade ? "enabled" : "disabled")}");
    report.AppendLine();

    report.AppendLine("==================================================");
    report.AppendLine("MSI OUTPUT");
    report.AppendLine("==================================================");

    report.AppendLine($"MSI dir  : {_profile.Output.MsiOutputDirectory}");
    report.AppendLine($"MSI file : {msiPath}");

    if (File.Exists(msiPath)) {
      FileInfo msiInfo = new(msiPath);
      report.AppendLine("MSI size : " + FormatFileSize(msiInfo.Length));
    }
    else {
      report.AppendLine("MSI size : not found");
    }

    report.AppendLine();

    report.AppendLine("==================================================");
    report.AppendLine("BUNDLE");
    report.AppendLine("==================================================");

    if (_profile.Bundle.BuildBundle) {
      string bundlePath = GetBundlePath();

      report.AppendLine("Enabled      : yes");
      report.AppendLine($"Name         : {_profile.Bundle.BundleName}");
      report.AppendLine($"UpgradeCode  : {_profile.Bundle.BundleUpgradeCode}");
      report.AppendLine($"Output dir   : {_profile.Bundle.BundleOutputDirectory}");
      report.AppendLine($"File name    : {_profile.Bundle.BundleFileName}");
      report.AppendLine($"Bundle file  : {bundlePath}");

      if (File.Exists(bundlePath)) {
        FileInfo bundleInfo = new(bundlePath);
        report.AppendLine("Bundle size  : " + FormatFileSize(bundleInfo.Length));
      }
      else {
        report.AppendLine("Bundle size  : not found");
      }
    }
    else {
      report.AppendLine("Enabled      : no");
    }

    report.AppendLine();
    report.AppendLine("==================================================");
    report.AppendLine("WEB INSTALLER");
    report.AppendLine("==================================================");

    if (_profile.WebInstaller.BuildWebInstaller) {
      string webInstallerPath = GetWebInstallerPathForReport();

      report.AppendLine("Enabled      : yes");
      report.AppendLine($"Name         : {_profile.WebInstaller.WebBundleName}");
      report.AppendLine($"UpgradeCode  : {_profile.WebInstaller.WebBundleUpgradeCode}");
      report.AppendLine($"Output dir   : {_profile.WebInstaller.WebOutputDirectory}");
      report.AppendLine($"File name    : {_profile.WebInstaller.WebSetupFileName}");
      report.AppendLine($"Download URL : {_profile.WebInstaller.MsiDownloadUrl}");
      report.AppendLine($"Web setup    : {webInstallerPath}");

      if (File.Exists(webInstallerPath)) {
        FileInfo webInfo = new(webInstallerPath);

        report.AppendLine(
            "Web size     : " +
            FormatFileSize(webInfo.Length)
        );
      }
      else {
        report.AppendLine("Web size     : not found");
      }
    }
    else {
      report.AppendLine("Enabled      : no");
    }

    report.AppendLine();

    report.AppendLine("==================================================");
    report.AppendLine("PATHS");
    report.AppendLine("==================================================");
    report.AppendLine($"Project file : {_profile.TargetProject.ProjectFile}");
    report.AppendLine($"Publish dir  : {_profile.Publish.PublishDirectory}");
    report.AppendLine($"WiX dir      : {_profile.Output.WixOutputDirectory}");
    report.AppendLine($"Build log    : {_buildLogPath}");
    report.AppendLine();

    report.AppendLine("==================================================");
    report.AppendLine("WEB UPLOAD");
    report.AppendLine("==================================================");

    if (_profile.Upload.UploadWebFilesAfterBuild) {
      report.AppendLine("Enabled      : yes");
      report.AppendLine($"Protocol     : {_profile.Upload.Protocol}");
      report.AppendLine($"Host         : {_profile.Upload.Host}");
      report.AppendLine($"User name    : {_profile.Upload.UserName}");
      report.AppendLine($"Remote dir   : {_profile.Upload.RemoteDirectory}");
      report.AppendLine($"Local dir    : {_profile.Upload.LocalDirectory}");
      report.AppendLine($"Web site     : {_profile.Upload.WebRemoteSite}");
      report.AppendLine($"WinSCP path  : {_profile.Upload.WinScpPath}");
      report.AppendLine($"Upload state : {(_lastUploadSuccess ? "SUCCESS" : "FAILED / NOT COMPLETED")}");

      if (!string.IsNullOrWhiteSpace(_lastUploadScriptPath))
        report.AppendLine($"Script       : {_lastUploadScriptPath}");

      if (!string.IsNullOrWhiteSpace(_lastUploadLogPath))
        report.AppendLine($"Upload log   : {_lastUploadLogPath}");
    }
    else {
      report.AppendLine("Enabled      : no");
    }

    report.AppendLine();

    report.AppendLine("==================================================");
    report.AppendLine("FINAL STATUS");
    report.AppendLine("==================================================");
    report.AppendLine(success ? "SUCCESS" : "FAILED");
    report.AppendLine(message);
    File.WriteAllText(reportPath,report.ToString());

    AppendLog("[INFO] Build report saved : " + reportPath);
    ExportCurrentMsiBuildArchive(reportPath);
  }

  private string GetWebInstallerPathForReport() {
    string webDir =
        Path.GetFullPath(
            _profile.WebInstaller.WebOutputDirectory
        );

    return Path.Combine(
        webDir,
        _profile.WebInstaller.WebSetupFileName
    );
  }

  private void CleanWebPublishDirectory() {
    string webPublishDirectory =
        _profile.WebInstaller.WebPublishDirectory;

    if (string.IsNullOrWhiteSpace(webPublishDirectory))
      throw new InvalidOperationException("Web publish directory is empty.");

    webPublishDirectory =
        Path.GetFullPath(webPublishDirectory);

    if (!Directory.Exists(webPublishDirectory)) {
      Directory.CreateDirectory(webPublishDirectory);
      AppendLog("[OK] Web publish directory created : " + webPublishDirectory);
      return;
    }

    if (!webPublishDirectory.EndsWith(
            Path.Combine("public","softwares"),
            StringComparison.OrdinalIgnoreCase)) {
      throw new InvalidOperationException(
          "Refusing to clean unexpected web publish directory : " +
          webPublishDirectory
      );
    }

    AppendLog("[INFO] Cleaning web publish directory : " + webPublishDirectory);

    foreach (string file in Directory.GetFiles(webPublishDirectory)) {
      string fileName =
          Path.GetFileName(file);

      if (fileName.Equals(".htaccess",StringComparison.OrdinalIgnoreCase)) {
        AppendLog("[INFO] Preserved : " + file);
        continue;
      }

      File.Delete(file);
      AppendLog("[INFO] Deleted file : " + file);
    }

    foreach (string directory in Directory.GetDirectories(webPublishDirectory)) {
      Directory.Delete(directory,recursive: true);
      AppendLog("[INFO] Deleted directory : " + directory);
    }

    AppendLog("[OK] Web publish directory cleaned.");
  }

  private void CopyArtifactAndSha256ToWebPublish(
    string sourceFilePath,
    string destinationFilePath) {

    if (string.IsNullOrWhiteSpace(sourceFilePath)) {
      throw new InvalidOperationException("Source file path is empty.");
    }

    if (string.IsNullOrWhiteSpace(destinationFilePath)) {
      throw new InvalidOperationException("Destination file path is empty.");
    }

    if (!File.Exists(sourceFilePath)) {
      throw new FileNotFoundException(
          "Source artifact was not found.",
          sourceFilePath);
    }

    string? destinationDirectory =
        Path.GetDirectoryName(destinationFilePath);

    if (!string.IsNullOrWhiteSpace(destinationDirectory)) {
      Directory.CreateDirectory(destinationDirectory);
    }

    File.Copy(
        sourceFilePath,
        destinationFilePath,
        overwrite: true);

    AppendLog("[OK] Artifact copied : " + destinationFilePath);

    string sourceSha256Path =
        sourceFilePath + ".sha256.txt";

    if (!File.Exists(sourceSha256Path)) {
      AppendLog("[WARN] SHA256 sidecar file not found : " + sourceSha256Path);
      return;
    }

    string destinationSha256Path =
        destinationFilePath + ".sha256.txt";

    File.Copy(
        sourceSha256Path,
        destinationSha256Path,
        overwrite: true);

    AppendLog("[OK] SHA256 copied : " + destinationSha256Path);
  }

  private void PrepareWebPublishFolder() {
    if (!_profile.WebInstaller.BuildWebInstaller &&
        !_profile.Android.PublishApk)
      return;

    if (!_profile.WebInstaller.PrepareWebPublishFolder)
      return;

    void CopyArtifactToWebPublishWithSha256(
        string sourcePath,
        string targetDirectory,
        string artifactLabel) {

      if (string.IsNullOrWhiteSpace(sourcePath)) {
        AppendLog("[WARN] " + artifactLabel + " path is empty for web publish.");
        return;
      }

      if (!File.Exists(sourcePath)) {
        AppendLog("[WARN] " + artifactLabel + " file not found for web publish : " + sourcePath);
        return;
      }

      Directory.CreateDirectory(targetDirectory);

      string targetPath =
          Path.Combine(
              targetDirectory,
              Path.GetFileName(sourcePath)
          );

      File.Copy(
          sourcePath,
          targetPath,
          overwrite: true
      );

      AppendLog("[OK] " + artifactLabel + " copied to web project folder : " + targetPath);

      WriteSha256File(targetPath);

      string targetSha256Path =
          targetPath + ".sha256.txt";

      if (File.Exists(targetSha256Path)) {
        AppendLog("[OK] " + artifactLabel + " SHA256 generated : " + targetSha256Path);
      }
      else {
        AppendLog("[WARN] " + artifactLabel + " SHA256 file was not generated : " + targetSha256Path);
      }
    }

    CleanWebPublishDirectory();

    string publishRoot =
        Path.GetFullPath(_profile.WebInstaller.WebPublishDirectory);

    string productFolder =
        GetWebProductFolderName();

    string msiSoftwarePackagerDir =
        Path.Combine(
            publishRoot,
            "msi-software-packager"
        );

    string projectPublishDir =
        Path.Combine(
            msiSoftwarePackagerDir,
            productFolder
        );

    string targetAssetsDir =
        Path.Combine(
            publishRoot,
            "assets"
        );

    string sourceAssetsDir =
        Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Assets"
        );

    string msiPath =
        Path.Combine(
            _profile.Output.MsiOutputDirectory,
            _profile.Output.MsiFileName
        );

    string webSetupPath =
        Path.Combine(
            _profile.WebInstaller.WebOutputDirectory,
            _profile.WebInstaller.WebSetupFileName
        );

    // ==================================================
    // Prepare folder tree
    // ==================================================

    Directory.CreateDirectory(publishRoot);
    Directory.CreateDirectory(msiSoftwarePackagerDir);
    Directory.CreateDirectory(targetAssetsDir);

    if (Directory.Exists(projectPublishDir)) {
      Directory.Delete(
          projectPublishDir,
          recursive: true
      );
    }

    Directory.CreateDirectory(projectPublishDir);

    // ==================================================
    // Copy web assets
    // ==================================================

    if (Directory.Exists(sourceAssetsDir)) {
      foreach (string sourceFile in Directory.GetFiles(sourceAssetsDir)) {
        string fileName =
            Path.GetFileName(sourceFile);

        string targetFile =
            Path.Combine(
                targetAssetsDir,
                fileName
            );

        File.Copy(
            sourceFile,
            targetFile,
            overwrite: true
        );
      }

      AppendLog("[OK] Web assets copied : " + targetAssetsDir);
    }
    else {
      AppendLog("[WARN] Assets directory not found : " + sourceAssetsDir);
    }

    EnsureDefaultWebAssets(targetAssetsDir);

    // ==================================================
    // Copy package files + SHA256
    // ==================================================

    if (!_profile.Android.PublishApk) {
      CopyArtifactToWebPublishWithSha256(
          msiPath,
          projectPublishDir,
          "MSI"
      );

      if (_profile.WebInstaller.BuildWebInstaller) {
        CopyArtifactToWebPublishWithSha256(
            webSetupPath,
            projectPublishDir,
            "Web setup"
        );
      }
    }

    if (_profile.Android.PublishApk) {
      string apkPath =
          _profile.Android.ApkFilePath;

      CopyArtifactToWebPublishWithSha256(
          apkPath,
          projectPublishDir,
          "APK"
      );
    }

    // ==================================================
    // Generate update manifest
    // ==================================================

    WriteUpdateManifest(projectPublishDir);

    // ==================================================
    // Generate /softwares/index.php
    // ==================================================

    string catalogPhp =
        ApplyWebTemplateValues(
            LoadWebTemplate("catalog.index.php.template")
        );

    File.WriteAllText(
        Path.Combine(
            publishRoot,
            "index.php"
        ),
        catalogPhp,
        EncodingHelper.Utf8NoBom
    );

    // ==================================================
    // Generate /softwares/download.php
    // ==================================================

    string downloadPhp =
        ApplyWebTemplateValues(
            LoadWebTemplate("download.php.template")
        );

    File.WriteAllText(
        Path.Combine(
            publishRoot,
            "download.php"
        ),
        downloadPhp,
        EncodingHelper.Utf8NoBom
    );

    // ==================================================
    // Generate /softwares/msi-software-packager/index.php
    // ==================================================

    string msiSoftwarePackagerPhp =
        ApplyWebTemplateValues(
            LoadWebTemplate("msi-software-packager.index.php.template")
        );

    File.WriteAllText(
        Path.Combine(
            msiSoftwarePackagerDir,
            "index.php"
        ),
        msiSoftwarePackagerPhp,
        EncodingHelper.Utf8NoBom
    );

    AppendLog("[INFO] Web publish folder prepared : " + publishRoot);
  }

  private void WriteSha256File(string filePath) {
    using FileStream stream = File.OpenRead(filePath);
    byte[] hash = System.Security.Cryptography.SHA256.HashData(stream);

    string sha256 = Convert.ToHexString(hash).ToLowerInvariant();
    string shaFilePath = filePath + ".sha256.txt";

    File.WriteAllText(
        shaFilePath,
        sha256 + "  " + Path.GetFileName(filePath),
        Encoding.UTF8
    );

    AppendLog("[OK] SHA256 generated : " + shaFilePath);
  }

  private static void EnsureDefaultWebAssets(string assetsDir) {
    Directory.CreateDirectory(assetsDir);

    string logoPath =
        Path.Combine(assetsDir,"pb-bzh-logo.png");

    string softwareIconPath =
        Path.Combine(assetsDir,"software-icon.png");

    string readmePath =
        Path.Combine(assetsDir,"README.txt");

    if (!File.Exists(logoPath) ||
        !File.Exists(softwareIconPath)) {
      File.WriteAllText(
          readmePath,
          "Place web images here:" + Environment.NewLine +
          "- pb-bzh-logo.png" + Environment.NewLine +
          "- software-icon.png" + Environment.NewLine,
          EncodingHelper.Utf8Bom
      );
    }
  }

  private void ExportCurrentMsiBuildArchive(string reportPath) {
    try {
      string buildRoot =
          Path.GetDirectoryName(_profile.Output.MsiOutputDirectory)!;

      string archivesDir =
          Path.Combine(buildRoot,"Archives");

      Directory.CreateDirectory(archivesDir);

      string archivePath =
          Path.Combine(
              archivesDir,
              $"MSI_BUILD_CURRENT_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
          );

      using ZipArchive archive =
          ZipFile.Open(
              archivePath,
              ZipArchiveMode.Create
          );

      if (File.Exists(_buildLogPath)) {
        archive.CreateEntryFromFile(
            _buildLogPath,
            Path.Combine("Logs",Path.GetFileName(_buildLogPath))
        );
      }

      if (!string.IsNullOrWhiteSpace(_currentProfilePath) && File.Exists(_currentProfilePath)) {
        archive.CreateEntryFromFile(
            _currentProfilePath,
            Path.Combine(
                "Profile",
                GetVersionedProfileFileName()
            )
        );
      }


      if (_profile.Bundle.BuildBundle) {
        string bundlePath = GetBundlePath();

        if (File.Exists(bundlePath)) {
          archive.CreateEntryFromFile(
              bundlePath,
              Path.Combine(
                  "Bundle",
                  Path.GetFileName(bundlePath)
              )
          );
        }
      }

      if (_profile.WebInstaller.BuildWebInstaller) {
        string webInstallerPath =
            GetWebInstallerPathForReport();

        if (File.Exists(webInstallerPath)) {
          archive.CreateEntryFromFile(
              webInstallerPath,
              Path.Combine(
                  "Web",
                  Path.GetFileName(webInstallerPath)
              )
          );
        }
      }

      string wixDir = _profile.Output.WixOutputDirectory;

      if (Directory.Exists(wixDir)) {
        foreach (string file in Directory.GetFiles(wixDir)) {
          archive.CreateEntryFromFile(
              file,
              Path.Combine(
                  "Wix",
                  Path.GetFileName(file)
              )
          );
        }
      }

      if (File.Exists(reportPath)) {
        archive.CreateEntryFromFile(
            reportPath,
            Path.Combine("Reports",Path.GetFileName(reportPath))
        );
      }

      string msiPath =
          Path.Combine(
              _profile.Output.MsiOutputDirectory,
              _profile.Output.MsiFileName
          );

      if (File.Exists(msiPath)) {
        archive.CreateEntryFromFile(
            msiPath,
            Path.Combine("Installer",Path.GetFileName(msiPath))
        );

        if (!string.IsNullOrWhiteSpace(_lastUploadLogPath) && File.Exists(_lastUploadLogPath)) {
          archive.CreateEntryFromFile(
              _lastUploadLogPath,
              Path.Combine(
                  "Upload",
                  Path.GetFileName(_lastUploadLogPath)
              )
          );
        }
      }

      string releaseDirectory =
          Path.Combine(
              GetBuildRootDirectory(),
              "Release"
          );

      AddFileToArchiveIfExists(
          archive,
          Path.Combine(releaseDirectory,"RELEASE_CHECKLIST.txt"),
          "Release",
          "RELEASE_CHECKLIST.txt"
      );

      AddFileToArchiveIfExists(
          archive,
          Path.Combine(releaseDirectory,"RELEASE_SUMMARY.txt"),
          "Release",
          "RELEASE_SUMMARY.txt"
      );

      AddFileToArchiveIfExists(
          archive,
          Path.Combine(releaseDirectory,"update.json"),
          "Release",
          "update.json"
      );

      AddFileToArchiveIfExists(
          archive,
          Path.Combine(releaseDirectory,"UpdateSettings.json"),
          "Release",
          "UpdateSettings.json"
      );

      AppendLog("[INFO] Build archive created : " + archivePath);
    }
    catch (Exception ex) {
      AppendLog("[WARNING] Build archive export failed : " + ex.Message);
    }
  }

  private static void AddFileToArchiveIfExists(
    ZipArchive archive,
    string sourcePath,
    string archiveFolder,
    string archiveFileName) {
    if (!File.Exists(sourcePath))
      return;

    archive.CreateEntryFromFile(
        sourcePath,
        Path.Combine(
            archiveFolder,
            archiveFileName
        )
    );
  }

  private void AppendBuildLogFile(string line) {
    if (!_buildFileLoggingEnabled)
      return;

    if (string.IsNullOrWhiteSpace(_buildLogPath))
      return;

    lock (BuildLogLock) {
      File.AppendAllText(
          _buildLogPath,
          line + Environment.NewLine,
          EncodingHelper.Utf8Bom
      );
    }
  }

  private void InitializeBuildLog() {
    _buildStartTime = DateTime.Now;

    string logDir = Path.Combine(
        Path.GetDirectoryName(_profile.Output.MsiOutputDirectory)!,
        "Logs"
    );

    Directory.CreateDirectory(logDir);

    _buildLogPath = Path.Combine(
        logDir,
        $"MSI_BUILD_LOG_{_buildStartTime:yyyyMMdd_HHmmss}.txt"
    );

    StringBuilder header = new();

    header.AppendLine("==================================================");
    header.AppendLine("MsiSoftwarePackager Build Live Log");
    header.AppendLine("==================================================");
    header.AppendLine();
    header.AppendLine($"Start time   : {_buildStartTime:yyyy-MM-dd HH:mm:ss}");
    header.AppendLine($"Product      : {_profile.Product.ProductName}");
    header.AppendLine($"Version      : {_profile.Product.Version}");
    header.AppendLine($"Manufacturer : {_profile.Product.Manufacturer}");
    header.AppendLine($"Project file : {_profile.TargetProject.ProjectFile}");
    header.AppendLine();

    File.WriteAllText(
        _buildLogPath,
        header.ToString(),
        EncodingHelper.Utf8Bom
    );

    _buildFileLoggingEnabled = true;
  }

  private bool ValidateProfileBeforeBuild() {
    List<string> errors = [];

    ApplyUiToProfile();
    RefreshMsiDownloadUrl();
    ApplyUiToProfile();

    if (_profile.WebInstaller.BuildWebInstaller) {
      if (string.IsNullOrWhiteSpace(_profile.WebInstaller.WebBundleName))
        errors.Add("Web bundle name is required.");

      if (string.IsNullOrWhiteSpace(_profile.WebInstaller.WebOutputDirectory))
        errors.Add("Web output directory is required.");

      if (string.IsNullOrWhiteSpace(_profile.WebInstaller.WebSetupFileName))
        errors.Add("Web setup file name is required.");

      if (string.IsNullOrWhiteSpace(_profile.WebInstaller.MsiDownloadUrl))
        errors.Add("MSI download URL is required.");

      if (string.IsNullOrWhiteSpace(_profile.WebInstaller.WebPublishDirectory))
        errors.Add("Web publish directory is required.");

      if (!Uri.TryCreate(
              _profile.WebInstaller.MsiDownloadUrl,
              UriKind.Absolute,
              out Uri? uri) ||
          (uri.Scheme != Uri.UriSchemeHttp &&
           uri.Scheme != Uri.UriSchemeHttps)) {
        errors.Add("MSI download URL must be a valid http:// or https:// URL.");
      }

      if (_profile.WebInstaller.MsiDownloadUrl.Contains(
              "http://msi-software-packager",
              StringComparison.OrdinalIgnoreCase)) {
        errors.Add(
            "MSI download URL still points to local WampServer. Use the public HTTPS URL."
        );
      }

      if (string.IsNullOrWhiteSpace(_profile.WebInstaller.WebBundleUpgradeCode))
        errors.Add("Web bundle UpgradeCode is required.");

      if (!Guid.TryParse(_profile.WebInstaller.WebBundleUpgradeCode,out _))
        errors.Add("Web bundle UpgradeCode is not a valid GUID.");

      if (_profile.WebInstaller.WebSetupFileName.Equals(
              "setup.exe",
              StringComparison.OrdinalIgnoreCase)) {
        errors.Add(
            "Web setup file name cannot be Setup.exe. Use ProductNameWebSetup.exe instead."
        );
      }
    }

    if (_profile.Bundle.BuildBundle) {
      if (string.IsNullOrWhiteSpace(_profile.Bundle.BundleUpgradeCode))
        errors.Add("Bundle UpgradeCode is required.");

      if (!Guid.TryParse(_profile.Bundle.BundleUpgradeCode,out _))
        errors.Add("Bundle UpgradeCode is not a valid GUID.");

      if (string.IsNullOrWhiteSpace(_profile.Bundle.BundleOutputDirectory)) {
        errors.Add("Bundle output directory is required.");
      }
      else if (!Path.IsPathRooted(_profile.Bundle.BundleOutputDirectory)) {
        string? buildRoot =
            Path.GetDirectoryName(_profile.Output.MsiOutputDirectory);

        if (string.IsNullOrWhiteSpace(buildRoot)) {
          errors.Add("Unable to resolve Bundle output directory because MSI output directory is invalid.");
        }
        else {
          _profile.Bundle.BundleOutputDirectory =
              Path.GetFullPath(
                  Path.Combine(
                      buildRoot,
                      _profile.Bundle.BundleOutputDirectory
                  )
              );

          txtBundleOutputDir.Text =
              _profile.Bundle.BundleOutputDirectory;

          AppendLog(
              "[INFO] Bundle output directory resolved : " +
              _profile.Bundle.BundleOutputDirectory
          );
        }
      }

      if (string.IsNullOrWhiteSpace(_profile.Bundle.BundleFileName))
        errors.Add("Bundle file name is required.");
    }

    if (string.IsNullOrWhiteSpace(_profile.Product.ProductName))
      errors.Add("Product name is required.");

    if (string.IsNullOrWhiteSpace(_profile.Product.Version))
      errors.Add("Version is required.");
    else if (!Version.TryParse(_profile.Product.Version,out _))
      errors.Add("Version is not valid.");

    if (string.IsNullOrWhiteSpace(_profile.Product.UpgradeCode))
      errors.Add("UpgradeCode is required. Generate one and save the profile.");

    if (!Guid.TryParse(_profile.Product.UpgradeCode,out _))
      errors.Add("UpgradeCode is not a valid GUID.");

    if (string.IsNullOrWhiteSpace(_profile.TargetProject.ProjectFile) ||
        !File.Exists(_profile.TargetProject.ProjectFile)) {
      errors.Add("Project file does not exist.");
    }

    if (_profile.Msi.RequireAdministrator) {
      ValidateRequireAdministratorManifest(errors);
    }

    if (string.IsNullOrWhiteSpace(_profile.TargetProject.ExecutableName))
      errors.Add("Executable name is required.");

    if (string.IsNullOrWhiteSpace(_profile.Publish.PublishDirectory))
      errors.Add("Publish directory is required.");

    if (string.IsNullOrWhiteSpace(_profile.Output.WixOutputDirectory))
      errors.Add("WiX output directory is required.");

    if (string.IsNullOrWhiteSpace(_profile.Output.MsiOutputDirectory))
      errors.Add("MSI output directory is required.");

    if (string.IsNullOrWhiteSpace(_profile.Output.MsiFileName))
      errors.Add("MSI file name is required.");
    else if (!_profile.Output.MsiFileName.EndsWith(".msi",StringComparison.OrdinalIgnoreCase))
      errors.Add("MSI file name must end with .msi.");

    if (_profile.Upload.UploadWebFilesAfterBuild) {
      if (string.IsNullOrWhiteSpace(_profile.Upload.WinScpPath))
        errors.Add("WinSCP path is required.");
      else if (!File.Exists(_profile.Upload.WinScpPath))
        errors.Add("WinSCP executable not found : " + _profile.Upload.WinScpPath);
    }

    ValidateRequiredTemplates(errors);
    ValidateWebAssets(errors);
    ValidateUploadPassword(errors);
    ValidateMsiDownloadUrlConsistency(errors);

    if (errors.Count == 0) {
      AppendLog("[VALIDATION OK] Profile validation successful.");
      return true;
    }

    AppendLog("[VALIDATION FAILED]");

    foreach (string error in errors)
      AppendLog("[ERROR] " + error);

    MessageBox.Show(
        string.Join(Environment.NewLine,errors),
        "MSI profile validation",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning
    );

    return false;
  }

  private void ValidateRequireAdministratorManifest(List<string> errors) {
    string projectFile =
        _profile.TargetProject.ProjectFile;

    string projectXml =
        File.ReadAllText(projectFile);

    if (!projectXml.Contains(
            "<ApplicationManifest>app.manifest</ApplicationManifest>",
            StringComparison.OrdinalIgnoreCase)) {
      AppendLog(
          "Project does not reference app.manifest with <ApplicationManifest>app.manifest</ApplicationManifest>."
      );

      return;
    }

    string projectDir =
        Path.GetDirectoryName(projectFile)!;

    string manifestPath =
        Path.Combine(projectDir,"app.manifest");

    if (!File.Exists(manifestPath)) {
      errors.Add(
          "app.manifest was referenced but not found."
      );

      return;
    }

    string manifestXml =
        File.ReadAllText(manifestPath);

    if (!manifestXml.Contains(
            "requestedExecutionLevel level=\"requireAdministrator\"",
            StringComparison.OrdinalIgnoreCase)) {
      AppendLog(
          "app.manifest does not require administrator privileges."
      );
    }
  }

  private static void HighlightRegex(RichTextBox editor,string pattern,Color color) {
    foreach (Match match
             in Regex.Matches(
                 editor.Text,
                 pattern)) {
      editor.Select(
          match.Index,
          match.Length
      );

      editor.SelectionColor =
          color;
    }
  }

  private static void ApplyXmlSyntaxHighlight(RichTextBox editor) {
    int selectionStart =
        editor.SelectionStart;

    int selectionLength =
        editor.SelectionLength;

    editor.SuspendLayout();

    editor.SelectAll();
    editor.SelectionColor =
        Color.Gainsboro;

    HighlightRegex(
        editor,
        @"</?[\w\:\-\.]+",
        Color.DeepSkyBlue
    );

    HighlightRegex(
        editor,
        "\".*?\"",
        Color.LightGreen
    );

    HighlightRegex(
        editor,
        @"\b(Id|Name|Version|SourceFile|Directory|Value|KeyPath)\b",
        Color.Khaki
    );

    editor.SelectionStart =
        selectionStart;

    editor.SelectionLength =
        selectionLength;

    editor.SelectionColor =
        Color.Gainsboro;

    editor.ResumeLayout();
  }

  private void UpdateWebUploadUiState() {
    bool enabled = chkUploadWebFilesAfterBuild.Checked;

    cmbUploadProtocol.Enabled = enabled;

    txtUploadHost.Enabled = enabled;
    txtUploadPassword.Enabled = enabled;
    txtUploadRemoteDir.Enabled = enabled;
    txtUploadLocalDir.Enabled = enabled;
    txtWinScpPath.Enabled = enabled;
    txtWebRemoteSite.Enabled = enabled;

    btnBrowseWinScpPath.Enabled = enabled;
    btnBrowseUploadLocalDir.Enabled = enabled;
    btnTestUploadConnection.Enabled = enabled;
    btnUploadNow.Enabled = enabled;
  }

  private void UpdateBundleUiState() {
    bool enabled = chkBuildBundle.Checked;

    txtBundleOutputDir.Enabled = enabled;
    txtBundleFileName.Enabled = enabled;
    txtBundleUpgradeCode.Enabled = enabled;
    btnBrowseBundleDir.Enabled = enabled;
    btnGenerateBundleUpgradeCode.Enabled = enabled;
  }

  private void btnOpenMsiFolder_Click(object? sender,EventArgs e) {
    try {
      string packagerRoot =
          GetPackagerRootDirectory();

      if (!Directory.Exists(packagerRoot)) {
        Directory.CreateDirectory(packagerRoot);
      }

      Process.Start(new ProcessStartInfo {
        FileName = packagerRoot,
        UseShellExecute = true
      });
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          ex.Message,
          "Open packager folder",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );
    }
  }

  private string GetPackagerRootDirectory() {
    ApplyUiToProfile();

    List<string> directories = [];

    AddDirectoryIfValid(directories,_profile.Publish.PublishDirectory);
    AddDirectoryIfValid(directories,_profile.Output.WixOutputDirectory);
    AddDirectoryIfValid(directories,_profile.Output.MsiOutputDirectory);

    if (_profile.Bundle.BuildBundle) {
      AddDirectoryIfValid(directories,_profile.Bundle.BundleOutputDirectory);
    }

    if (directories.Count == 0) {
      throw new InvalidOperationException(
          "Unable to determine packager root directory. No output directory is defined."
      );
    }

    string commonRoot =
        GetCommonDirectoryPath(directories);

    DirectoryInfo buildRoot =
        new(commonRoot);

    DirectoryInfo? packagerRoot =
        buildRoot.Parent;

    return packagerRoot == null
      ? throw new InvalidOperationException(
          "Unable to determine packager root directory from: " + commonRoot
      )
      : packagerRoot.FullName;
  }

  private static void AddDirectoryIfValid(
    List<string> directories,
    string? path) {
    if (string.IsNullOrWhiteSpace(path))
      return;

    directories.Add(
        Path.GetFullPath(path.Trim())
    );
  }

  private static string GetCommonDirectoryPath(List<string> paths) {
    if (paths.Count == 0)
      throw new ArgumentException("No paths provided.");

    string[] firstParts =
        paths[0]
            .TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);

    int commonLength =
        firstParts.Length;

    foreach (string path in paths.Skip(1)) {
      string[] parts =
          path
              .TrimEnd(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar)
              .Split(Path.DirectorySeparatorChar,Path.AltDirectorySeparatorChar);

      commonLength =
          Math.Min(commonLength,parts.Length);

      for (int i = 0;i < commonLength;i++) {
        if (!string.Equals(
                firstParts[i],
                parts[i],
                StringComparison.OrdinalIgnoreCase)) {
          commonLength = i;
          break;
        }
      }
    }

    if (commonLength == 0) {
      throw new InvalidOperationException(
          "Unable to determine common output directory."
      );
    }

    string commonPath =
        string.Join(
            Path.DirectorySeparatorChar,
            firstParts.Take(commonLength)
        );

    if (commonPath.EndsWith(':'))
      commonPath += Path.DirectorySeparatorChar;

    return commonPath;
  }

  private void OpenEditorInNotepad(RichTextBox editor) {
    try {
      string fileName = _editorFileNames.TryGetValue(editor,out string? knownName)
              ? knownName
              : "msi-software-packager_Log.txt";

      string tempFile = Path.Combine(Path.GetTempPath(),fileName);
      File.WriteAllText(tempFile,editor.Text);
      string? notepadPlusPlus = FindNotepadPlusPlus();

      if (notepadPlusPlus == null) {
        Process.Start(new ProcessStartInfo {
          FileName = tempFile,
          UseShellExecute = true
        });
        return;
      }

      Process.Start(new ProcessStartInfo {
        FileName = notepadPlusPlus,
        Arguments = $"\"{tempFile}\"",
        UseShellExecute = true
      });
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);
    }
  }

  private void ApplyProfileToUi() {
    txtProductName.Text = _profile.Product.ProductName;
    txtManufacturer.Text = _profile.Product.Manufacturer;
    txtVersion.Text = _profile.Product.Version;

    txtProjectFile.Text = _profile.TargetProject.ProjectFile;
    txtExecutableName.Text = _profile.TargetProject.ExecutableName;

    txtPublishDir.Text = _profile.Publish.PublishDirectory;
    txtMsiOutputDir.Text = _profile.Output.MsiOutputDirectory;

    chkDesktopShortcut.Checked = _profile.Shortcuts.CreateDesktopShortcut;
    chkStartMenuShortcut.Checked = _profile.Shortcuts.CreateStartMenuShortcut;
    chkAddToPath.Checked = _profile.Environment.AddInstallFolderToPath;
    txtIconPath.Text = _profile.Product.IconPath;
    txtUpgradeCode.Text = _profile.Product.UpgradeCode;
    txtWixOutputDir.Text = _profile.Output.WixOutputDirectory;
    chkRequireAdministrator.Checked = _profile.Msi.RequireAdministrator;
    chkBuildBundle.Checked = _profile.Bundle.BuildBundle;
    txtBundleOutputDir.Text = _profile.Bundle.BundleOutputDirectory;
    txtBundleFileName.Text = _profile.Bundle.BundleFileName;
    txtBundleUpgradeCode.Text = _profile.Bundle.BundleUpgradeCode;
    txtBundleName.Text = _profile.Bundle.BundleName;
    chkBuildWebInstaller.Checked = _profile.WebInstaller.BuildWebInstaller;
    txtWebBundleName.Text = _profile.WebInstaller.WebBundleName;
    txtWebOutputDir.Text = _profile.WebInstaller.WebOutputDirectory;
    txtWebSetupFileName.Text = _profile.WebInstaller.WebSetupFileName;
    txtMsiDownloadUrl.Text = _profile.WebInstaller.MsiDownloadUrl;
    txtWebBundleUpgradeCode.Text = _profile.WebInstaller.WebBundleUpgradeCode;
    txtWebPublishDirectory.Text = _profile.WebInstaller.WebPublishDirectory;
    UpdateWebInstallerUiState();
    UpdateToolTips();
    chkUploadWebFilesAfterBuild.Checked = _profile.Upload.UploadWebFilesAfterBuild;
    cmbUploadProtocol.Text = _profile.Upload.Protocol;
    txtUploadHost.Text = _profile.Upload.Host;
    txtUploadRemoteDir.Text = _profile.Upload.RemoteDirectory;
    txtUploadLocalDir.Text = _profile.Upload.LocalDirectory;
    txtWinScpPath.Text = _profile.Upload.WinScpPath;
    txtWebRemoteSite.Text = _profile.Upload.WebRemoteSite;
    txtUploadUserName.Text = _profile.Upload.UserName;

    txtUploadPassword.Text = string.Empty;

    if (string.IsNullOrWhiteSpace(_profile.Upload.CredentialTarget)) {
      _profile.Upload.CredentialTarget = WindowsCredentialManager.BuildUploadTargetName(_profile.Upload.Protocol,_profile.Upload.Host,_profile.Upload.UserName);
    }

    if (!string.IsNullOrWhiteSpace(_profile.Upload.CredentialTarget)) {
      txtUploadPassword.Text = WindowsCredentialManager.ReadPassword(_profile.Upload.CredentialTarget);
    }

    // methode codage avec protection Windows (non utilisée car pas compatible avec les autres OS et rend le mot de passe illisible dans le JSON)
    // txtUploadPassword.Text = WindowsPasswordProtector.UnprotectSafe(_profile.Upload.Password);

    // méthode de codage simple avec BOM Unicode pour éviter les problèmes d'encodage dans le JSON (compatible avec tous les OS mais pas sécurisé)
    // txtUploadPassword.Text = PasswordCodec.DecodePasswordUnicodeBomSafe(_profile.Upload.Password);

    // Important : password volontairement non chargé depuis le JSON
    // txtUploadPassword.Clear();

    UpdateWebUploadUiState();
    txtWebProductFolder.Text = string.IsNullOrWhiteSpace(_profile.WebInstaller.WebProductFolder)
            ? UpdateSettingsHelper.GetSafeFileName(_profile.Product.ProductName)
            : _profile.WebInstaller.WebProductFolder;
    _profile.WebInstaller.WebProductFolder = txtWebProductFolder.Text;

    chkRequireLicenseAcceptance.Checked = _profile.License.RequireLicenseAcceptance;
    chkTechnicalLicenseRequired.Checked = _profile.License.TechnicalLicenseRequired;
  }

  private void ApplyUiToProfile() {
    _profile.Product.ProductName = txtProductName.Text;
    _profile.Product.Manufacturer = txtManufacturer.Text;
    _profile.Product.Version = txtVersion.Text;

    _profile.TargetProject.ProjectFile = txtProjectFile.Text;
    _profile.TargetProject.ExecutableName = txtExecutableName.Text;

    _profile.Publish.PublishDirectory = txtPublishDir.Text;
    _profile.Output.MsiOutputDirectory = txtMsiOutputDir.Text;

    _profile.Shortcuts.CreateDesktopShortcut = chkDesktopShortcut.Checked;
    _profile.Shortcuts.CreateStartMenuShortcut = chkStartMenuShortcut.Checked;
    _profile.Environment.AddInstallFolderToPath = chkAddToPath.Checked;
    _profile.Product.IconPath = txtIconPath.Text;
    _profile.Product.UpgradeCode = txtUpgradeCode.Text;
    _profile.Msi.InstallFolderName = txtProductName.Text;
    _profile.Shortcuts.ShortcutName = txtProductName.Text;

    _profile.Output.MsiFileName = UpdateSettingsHelper.GetSafeFileName(_profile.Product.ProductName) + ".msi";

    _profile.Output.WixOutputDirectory = txtWixOutputDir.Text;
    _profile.Msi.RequireAdministrator = chkRequireAdministrator.Checked;
    _profile.Bundle.BuildBundle = chkBuildBundle.Checked;
    _profile.Bundle.BundleOutputDirectory = txtBundleOutputDir.Text;
    _profile.Bundle.BundleFileName = txtBundleFileName.Text;
    _profile.Bundle.BundleUpgradeCode = txtBundleUpgradeCode.Text;
    _profile.Bundle.BundleName = txtBundleName.Text;
    if (string.IsNullOrWhiteSpace(_profile.Bundle.BundleName)) {
      _profile.Bundle.BundleName = _profile.Product.ProductName;
    }
    _profile.WebInstaller.BuildWebInstaller = chkBuildWebInstaller.Checked;
    _profile.WebInstaller.WebBundleName = txtWebBundleName.Text;
    _profile.WebInstaller.WebOutputDirectory = txtWebOutputDir.Text;
    _profile.WebInstaller.WebSetupFileName = txtWebSetupFileName.Text;
    _profile.WebInstaller.MsiDownloadUrl = txtMsiDownloadUrl.Text;
    SyncUpdateManifestFromProfile();
    _profile.WebInstaller.WebBundleUpgradeCode = txtWebBundleUpgradeCode.Text;
    _profile.WebInstaller.WebPublishDirectory = txtWebPublishDirectory.Text;
    if (string.IsNullOrWhiteSpace(_profile.WebInstaller.WebBundleName)) {
      _profile.WebInstaller.WebBundleName =
          _profile.Product.ProductName;
    }
    _profile.Upload.UploadWebFilesAfterBuild = chkUploadWebFilesAfterBuild.Checked;
    _profile.Upload.Protocol = cmbUploadProtocol.Text;
    _profile.Upload.Host = txtUploadHost.Text;
    _profile.Upload.RemoteDirectory = txtUploadRemoteDir.Text;
    _profile.Upload.LocalDirectory = txtUploadLocalDir.Text;
    _profile.Upload.WinScpPath = txtWinScpPath.Text;
    _profile.Upload.WebRemoteSite = txtWebRemoteSite.Text;
    _profile.Upload.UserName = txtUploadUserName.Text;

    //_profile.WebInstaller.WebProductFolder = txtWebProductFolder.Text.Trim();
    if (string.IsNullOrWhiteSpace(_profile.WebInstaller.WebProductFolder)) {
      _profile.WebInstaller.WebProductFolder =
          UpdateSettingsHelper.GetSafeFileName(_profile.Product.ProductName);
    }
    // methode de stockage sécurisé du mot de passe dans le gestionnaire de credentials Windows (compatible avec tous les OS mais rend le mot de passe illisible dans le JSON)
    string credentialTarget = WindowsCredentialManager.BuildUploadTargetName(_profile.Upload.Protocol,_profile.Upload.Host,_profile.Upload.UserName);
    _profile.Upload.CredentialTarget = credentialTarget;
    _profile.Upload.Password = string.Empty;
    if (!string.IsNullOrEmpty(txtUploadPassword.Text)) {
      WindowsCredentialManager.SavePassword(_profile.Upload.CredentialTarget,_profile.Upload.UserName,txtUploadPassword.Text);
    }

    // methode codage avec protection Windows (non utilisée car pas compatible avec les autres OS et rend le mot de passe illisible dans le JSON)
    //_profile.Upload.Password = WindowsPasswordProtector.Protect(txtUploadPassword.Text);

    // méthode de codage simple avec BOM Unicode pour éviter les problèmes d'encodage dans le JSON (compatible avec tous les OS mais pas sécurisé)
    // _profile.Upload.Password = PasswordCodec.EncodePasswordUnicodeBom(txtUploadPassword.Text);

    _profile.License.RequireLicenseAcceptance = chkRequireLicenseAcceptance.Checked;
    _profile.License.TechnicalLicenseRequired = chkTechnicalLicenseRequired.Checked;
  }

  private void UpdateToolTips() {
    toolTip.SetToolTip(
        txtMsiDownloadUrl,
        txtMsiDownloadUrl.Text
    );
  }

  private void UpdateWebInstallerUiState() {
    bool enabled = chkBuildWebInstaller.Checked;

    txtWebBundleName.Enabled = enabled;
    txtWebOutputDir.Enabled = enabled;
    txtWebSetupFileName.Enabled = enabled;
    txtMsiDownloadUrl.Enabled = enabled;
    txtWebBundleUpgradeCode.Enabled = enabled;
    txtWebPublishDirectory.Enabled = enabled;

    btnBrowseWebOutputDir.Enabled = enabled;
    btnGenerateWebBundleUpgradeCode.Enabled = enabled;
  }

  private void btnGenerateBundleUpgradeCode_Click(object? sender,EventArgs e) {
    if (!string.IsNullOrWhiteSpace(txtBundleUpgradeCode.Text))
      return;

    txtBundleUpgradeCode.Text = Guid.NewGuid().ToString().ToUpperInvariant();

    AutoSaveProfile();

    AppendLog("[INFO] Bundle UpgradeCode generated.");
  }

  private ContextMenuStrip CreateEditorContextMenu(RichTextBox editor) {
    ContextMenuStrip menu = new();

    ToolStripMenuItem copyItem = new("Copier");
    copyItem.Click += (_,_) => {
      editor.Copy();
    };

    ToolStripMenuItem selectAllItem =
        new("Tout sélectionner");

    selectAllItem.Click += (_,_) => {
      editor.SelectAll();
    };

    ToolStripMenuItem notepadItem =
        new("Éditer dans Notepad++");

    notepadItem.Click += (_,_) => {
      OpenEditorInNotepad(editor);
    };

    menu.Items.Add(copyItem);
    menu.Items.Add(selectAllItem);
    menu.Items.Add(new ToolStripSeparator());
    menu.Items.Add(notepadItem);

    return menu;
  }

  private void RefreshPreviewTabs() {
    ApplyUiToProfile();

    string root =
        _profile.Output.WixOutputDirectory;

    foreach (KeyValuePair<string,RichTextBox> item in _previewEditors) {
      if (item.Key == "Catalog index.php") {
        string content =
            ApplyWebTemplateValues(
                LoadWebTemplate("catalog.index.php.template")
            );


        SetPlainPreviewText(item.Value,content);
        ApplyPreviewSyntaxHighlighting(item.Value,item.Key);
        continue;
      }

      if (item.Key == "download.php") {
        string content =
            ApplyWebTemplateValues(
                LoadWebTemplate("download.php.template")
            );


        SetPlainPreviewText(item.Value,content);
        ApplyPreviewSyntaxHighlighting(item.Value,item.Key);
        continue;
      }

      if (item.Key == "msi-software-packager index.php") {
        string content =
            ApplyWebTemplateValues(
                LoadWebTemplate("msi-software-packager.index.php.template")
            );

        SetPlainPreviewText(item.Value,content);
        ApplyPreviewSyntaxHighlighting(item.Value,item.Key);
        continue;
      }

      if (item.Key == "WinSCP upload script") {
        string content =
            GenerateWinScpUploadScriptPreview();

        SetPlainPreviewText(item.Value,content);
        ApplyPreviewSyntaxHighlighting(item.Value,item.Key);
        continue;
      }

      if (item.Key == "Profile.msi.json") {
        string content = ProfileSerializer.ToJson(_profile);

        SetPlainPreviewText(item.Value,content);
        ApplyPreviewSyntaxHighlighting(item.Value,item.Key);
        continue;
      }

      if (item.Key == "Build History") {
        string content = BuildBuildHistoryText();

        SetPlainPreviewText(item.Value,content);
        ApplyPreviewSyntaxHighlighting(item.Value,item.Key);
        continue;
      }

      if (item.Key == "Environment Check") {
        string content = BuildEnvironmentCheckText();

        SetPlainPreviewText(item.Value,content);
        ApplyPreviewSyntaxHighlighting(item.Value,item.Key);
        continue;
      }

      if (item.Key == "Release Checklist") {
        string content = BuildReleaseChecklistText();

        SetPlainPreviewText(item.Value,content);
        ApplyPreviewSyntaxHighlighting(item.Value,item.Key);
        continue;
      }

      if (item.Key == "Release summary") {
        string content = BuildReleaseSummaryText();

        SetPlainPreviewText(item.Value,content);
        ApplyPreviewSyntaxHighlighting(item.Value,item.Key);
        continue;
      }

      string filePath =
          Path.Combine(root,item.Key);

      if (!File.Exists(filePath)) {
        SetPlainPreviewText(item.Value,string.Empty);
        continue;
      }

      SetPlainPreviewText(
          item.Value,
          File.ReadAllText(filePath,Encoding.UTF8)
      );
      item.Value.Text = File.ReadAllText(filePath);
      ApplyXmlSyntaxHighlight(item.Value);
    }
  }

  private string BuildReleaseSummaryText() {
    ApplyUiToProfile();
    RefreshMsiDownloadUrl();
    ApplyUiToProfile();

    StringBuilder report = new();
    string productFolder = GetWebProductFolderName();
    string webRemoteSite = _profile.Upload.WebRemoteSite
      .Trim()
      .TrimEnd('/');
    string downloadPage =
        webRemoteSite +
        "/download.php?category=" +
        Uri.EscapeDataString(WebCategoryFolderName) +
        "&product=" +
        Uri.EscapeDataString(productFolder);

    string executablePath = Path.Combine(_profile.Publish.PublishDirectory,_profile.TargetProject.ExecutableName);
    string msiPath = Path.Combine(_profile.Output.MsiOutputDirectory,_profile.Output.MsiFileName);
    string bundlePath = Path.Combine(_profile.Bundle.BundleOutputDirectory,_profile.Bundle.BundleFileName);
    string webSetupPath = Path.Combine(_profile.WebInstaller.WebOutputDirectory,_profile.WebInstaller.WebSetupFileName);
    string apkPath = _profile.Android.ApkFilePath;
    string webPublishProductDir = Path.Combine(_profile.WebInstaller.WebPublishDirectory,WebCategoryFolderName,productFolder);
    string webApkPath = string.IsNullOrWhiteSpace(apkPath)
      ? string.Empty
      : Path.Combine(webPublishProductDir,Path.GetFileName(apkPath));
    string webApkSha256Path = string.IsNullOrWhiteSpace(webApkPath)
      ? string.Empty
      : webApkPath + ".sha256.txt";
    string updateManifestPath = Path.Combine(webPublishProductDir,"update.json");

    bool isAndroidOnly =
    _profile.Android.PublishApk &&
    !_profile.Bundle.BuildBundle &&
    !_profile.WebInstaller.BuildWebInstaller;

    report.AppendLine("==================================================");
    report.AppendLine("Release Summary");
    report.AppendLine("==================================================");
    report.AppendLine();

    report.AppendLine("Product      : " + _profile.Product.ProductName);
    report.AppendLine("Version      : " + _profile.Product.Version);
    report.AppendLine("Publisher    : " + _profile.Product.Manufacturer);
    report.AppendLine();

    report.AppendLine("Artifacts");
    if (!isAndroidOnly) {
      report.AppendLine("Executable   : " + GetArtifactStatus(executablePath,_profile.Signing.SignArtifacts && _profile.Signing.SignExecutable));
      report.AppendLine("MSI          : " + GetArtifactStatus(msiPath,_profile.Signing.SignArtifacts && _profile.Signing.SignMsi));

      if (_profile.Bundle.BuildBundle) {
        report.AppendLine("Bundle       : " + GetArtifactStatus(bundlePath,_profile.Signing.SignArtifacts && _profile.Signing.SignBundle));
      }
      else {
        report.AppendLine("Bundle       : disabled");
      }

      if (_profile.WebInstaller.BuildWebInstaller) {
        report.AppendLine("WebSetup     : " + GetArtifactStatus(webSetupPath,_profile.Signing.SignArtifacts && _profile.Signing.SignWebSetup));
      }
      else {
        report.AppendLine("WebSetup     : disabled");
      }
    }
    else {
      report.AppendLine("Android APK  : " + GetSimpleFileStatus(apkPath));
      report.AppendLine("APK web file : " + GetSimpleFileStatus(webApkPath));
      report.AppendLine("APK SHA256   : " + GetSimpleFileStatus(webApkSha256Path));
    }

    report.AppendLine("update.json  : " + (File.Exists(updateManifestPath) ? "OK" : "missing"));

    if (_profile.Upload.UploadWebFilesAfterBuild)
      report.AppendLine("Upload       : enabled");
    else
      report.AppendLine("Upload       : disabled");

    report.AppendLine();

    report.AppendLine("URLs");
    report.AppendLine("Download URL : " + downloadPage);
    if (!isAndroidOnly) {
      report.AppendLine("MSI URL      : " + _profile.WebInstaller.MsiDownloadUrl);
    }
    else {
      if (_profile.Android.PublishApk) {
        report.AppendLine("APK URL      : " + BuildApkUrl());
      }
    }

    if (_profile.WebInstaller.BuildWebInstaller) {
      string webSetupUrl =
          webRemoteSite +
          "/" +
          Uri.EscapeDataString(WebCategoryFolderName) +
          "/" +
          Uri.EscapeDataString(productFolder) +
          "/" +
          Uri.EscapeDataString(_profile.WebInstaller.WebSetupFileName);

      report.AppendLine("WebSetup URL : " + webSetupUrl);
    }

    report.AppendLine();

    report.AppendLine("Output files");
    if (!isAndroidOnly) {
      report.AppendLine("Executable   : " + executablePath);
      report.AppendLine("MSI          : " + msiPath);

      if (_profile.Bundle.BuildBundle)
        report.AppendLine("Bundle       : " + bundlePath);

      if (_profile.WebInstaller.BuildWebInstaller)
        report.AppendLine("WebSetup     : " + webSetupPath);
    }
    else {
      report.AppendLine("APK source   : " + apkPath);
      report.AppendLine("APK web      : " + webApkPath);
      report.AppendLine("APK SHA256   : " + webApkSha256Path);

    }
    report.AppendLine("Web publish  : " + webPublishProductDir);

    return report.ToString();
  }

  private static string GetSimpleFileStatus(string filePath) {
    if (string.IsNullOrWhiteSpace(filePath))
      return "not configured";

    return File.Exists(filePath)
        ? "OK"
        : "missing";
  }

  private string GetArtifactStatus(
      string filePath,
      bool shouldBeSigned) {
    if (!File.Exists(filePath))
      return "missing";

    if (!shouldBeSigned)
      return "OK - signing disabled";

    FileSignatureStatus status =
        GetFileSignatureStatus(filePath);

    return status switch {
      FileSignatureStatus.SignedAndTrusted =>
          "OK - signed and trusted",

      FileSignatureStatus.SignedButNotTrusted =>
          "OK - signed, not trusted",

      FileSignatureStatus.NotSigned =>
          "OK - not signed",

      FileSignatureStatus.Missing =>
          "missing",

      _ =>
          "signature verification error"
    };
  }

  private void CheckCurrentProfileEnvironment(
    StringBuilder report,
    ref bool hasError,
    ref bool hasWarning) {
    report.AppendLine();
    report.AppendLine("Current profile");

    CheckFile(
        report,
        ref hasError,
        "Project file",
        _profile.TargetProject.ProjectFile,
        required: true
    );

    CheckDirectory(
        report,
        ref hasWarning,
        "Publish directory",
        _profile.Publish.PublishDirectory,
        required: false
    );

    CheckDirectory(
        report,
        ref hasWarning,
        "WiX output directory",
        _profile.Output.WixOutputDirectory,
        required: false
    );

    CheckDirectory(
        report,
        ref hasWarning,
        "MSI output directory",
        _profile.Output.MsiOutputDirectory,
        required: false
    );

    CheckDirectory(
        report,
        ref hasWarning,
        "Web publish directory",
        _profile.WebInstaller.WebPublishDirectory,
        required: false
    );

    CheckDirectory(
        report,
        ref hasWarning,
        "Upload local directory",
        _profile.Upload.LocalDirectory,
        required: false
    );

    if (_profile.Upload.UploadWebFilesAfterBuild) {
      CheckFile(
          report,
          ref hasError,
          "WinSCP",
          _profile.Upload.WinScpPath,
          required: true
      );
    }
    else {
      CheckFile(
          report,
          ref hasWarning,
          "WinSCP",
          _profile.Upload.WinScpPath,
          required: false
      );
    }

    if (_profile.Signing.SignArtifacts) {
      CheckFile(
          report,
          ref hasError,
          "SignTool",
          _profile.Signing.SignToolPath,
          required: true
      );
    }
    else {
      CheckFile(
          report,
          ref hasWarning,
          "SignTool",
          _profile.Signing.SignToolPath,
          required: false
      );
    }
  }

  private string BuildEnvironmentCheckText() {
    ApplyUiToProfile();

    StringBuilder report = new();

    report.AppendLine("==================================================");
    report.AppendLine("Environment Check");
    report.AppendLine("==================================================");
    report.AppendLine();

    report.AppendLine("Product : " + _profile.Product.ProductName);
    report.AppendLine("Version : " + _profile.Product.Version);
    report.AppendLine();

    bool hasError = false;
    bool hasWarning = false;

    CheckGlobalSettings(
        report,
        ref hasWarning
    );

    CheckCurrentProfileEnvironment(
        report,
        ref hasError,
        ref hasWarning
    );

    CheckSigningCertificate(
        report,
        ref hasWarning
    );

    CheckUploadCredential(
        report,
        ref hasWarning
    );

    CheckTemplates(
        report,
        ref hasError
    );

    CheckAssets(
        report,
        ref hasError
    );

    report.AppendLine();
    report.AppendLine("==================================================");
    report.AppendLine("Final status");
    report.AppendLine("==================================================");

    if (hasError)
      report.AppendLine("FAILED");
    else if (hasWarning)
      report.AppendLine("OK WITH WARNINGS");
    else
      report.AppendLine("OK");

    return report.ToString();
  }

  private string BuildBuildHistoryText() {
    ApplyUiToProfile();

    string buildRoot =
        Path.GetDirectoryName(_profile.Output.MsiOutputDirectory)
        ?? string.Empty;

    if (string.IsNullOrWhiteSpace(buildRoot))
      return "Build root is not defined.";

    string reportsDir =
        Path.Combine(buildRoot,"Reports");

    string logsDir =
        Path.Combine(buildRoot,"Logs");

    string archivesDir =
        Path.Combine(buildRoot,"Archives");

    StringBuilder sb = new();

    sb.AppendLine("==================================================");
    sb.AppendLine("Build History");
    sb.AppendLine("==================================================");
    sb.AppendLine();

    sb.AppendLine("Product : " + _profile.Product.ProductName);
    sb.AppendLine("Version : " + _profile.Product.Version);
    sb.AppendLine("Root    : " + buildRoot);
    sb.AppendLine();

    AppendHistorySection(
        sb,
        "REPORTS",
        reportsDir,
        "MSI_BUILD_REPORT_*.txt"
    );

    AppendHistorySection(
        sb,
        "LOGS",
        logsDir,
        "MSI_BUILD_LOG_*.txt"
    );

    AppendHistorySection(
        sb,
        "ARCHIVES",
        archivesDir,
        "*.zip"
    );

    return sb.ToString();
  }

  private static void AppendHistorySection(
      StringBuilder sb,
      string title,
      string directory,
      string searchPattern) {
    const int maxFilesToShow = 30;

    sb.AppendLine("==================================================");
    sb.AppendLine(title);
    sb.AppendLine("==================================================");

    if (!Directory.Exists(directory)) {
      sb.AppendLine("Directory not found : " + directory);
      sb.AppendLine();
      return;
    }

    FileInfo[] allFiles =
        [.. new DirectoryInfo(directory)
            .GetFiles(searchPattern)
            .OrderByDescending(f => f.LastWriteTime)];

    if (allFiles.Length == 0) {
      sb.AppendLine("No file found.");
      sb.AppendLine();
      return;
    }

    FileInfo[] files =
        [.. allFiles.Take(maxFilesToShow)];

    if (allFiles.Length > maxFilesToShow) {
      sb.AppendLine(
          $"Showing latest {maxFilesToShow} files of {allFiles.Length}."
      );
      sb.AppendLine();
    }

    foreach (FileInfo file in files) {
      sb.AppendLine(
          $"{file.LastWriteTime:yyyy-MM-dd HH:mm:ss} | " +
          $"{FormatFileSize(file.Length),10} | " +
          file.Name
      );
    }

    sb.AppendLine();
  }

  private static void ApplyPreviewSyntaxHighlighting(
    RichTextBox editor,
    string previewName) {
    string name =
        previewName.ToLowerInvariant();

    if (name.EndsWith(".php")) {
      ApplyPhpSyntaxHighlighting(editor);
      return;
    }

    if (name.Contains("winscp")) {
      ApplyWinScpSyntaxHighlighting(editor);
      return;
    }

    if (name.EndsWith(".json")) {
      ApplyJsonSyntaxHighlighting(editor);
      return;
    }

    if (name.EndsWith(".wxs") ||
        name.EndsWith(".wxl") ||
        name.EndsWith(".xml")) {
      ApplyXmlSyntaxHighlight(editor);
      return;
    }
  }

  private static void ApplyJsonSyntaxHighlighting(RichTextBox editor) {
    int selectionStart = editor.SelectionStart;
    int selectionLength = editor.SelectionLength;

    editor.SuspendLayout();

    editor.SelectAll();
    editor.SelectionColor = Color.Gainsboro;

    HighlightRegex(
        editor,
        "\"(?:\\\\.|[^\"])*\"\\s*:",
        Color.DeepSkyBlue
    );

    HighlightRegex(
        editor,
        ":\\s*\"(?:\\\\.|[^\"])*\"",
        Color.LightGreen
    );

    HighlightRegex(
        editor,
        "\\b(true|false|null)\\b",
        Color.Orange,
        RegexOptions.IgnoreCase
    );

    HighlightRegex(
        editor,
        "\\b\\d+(?:\\.\\d+)?\\b",
        Color.Khaki
    );

    editor.SelectionStart = Math.Min(selectionStart,editor.TextLength);
    editor.SelectionLength = Math.Min(selectionLength,editor.TextLength - editor.SelectionStart);
    editor.SelectionColor = Color.Gainsboro;

    editor.ResumeLayout();
  }

  private void ApplyProfileJsonPreviewToUi() {
    if (!TryGetSelectedPreviewEditor(out RichTextBox editor)) {
      MessageBox.Show(
          "Aucune vue sélectionnée.",
          "Profile JSON",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );
      return;
    }

    if (tabPreview.SelectedTab == null ||
        !tabPreview.SelectedTab.Text.Equals("Profile.msi.json",StringComparison.OrdinalIgnoreCase)) {
      MessageBox.Show(
          "La vue sélectionnée n'est pas le profil JSON.",
          "Profile JSON",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );
      return;
    }

    if (string.IsNullOrWhiteSpace(editor.Text)) {
      MessageBox.Show(
          "Le profil JSON est vide.",
          "Profile JSON",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );
      return;
    }

    try {
      _isLoadingProfile = true;

      MsiPackageProfile loadedProfile =
          ProfileSerializer.FromJson(editor.Text);

      _profile = loadedProfile;

      ApplyProfileToUi();
    }
    catch (Exception ex) {
      MessageBox.Show(
          this,
          "Le JSON du profil est invalide." +
          Environment.NewLine +
          Environment.NewLine +
          ex.Message,
          "Profile JSON",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );

      AppendLog("[ERROR] Invalid profile JSON: " + ex.Message);
      return;
    }
    finally {
      _isLoadingProfile = false;
    }

    ApplyUiToProfile();
    RefreshMsiDownloadUrl();

    UpdateBundleUiState();
    UpdateWebInstallerUiState();
    UpdateWebUploadUiState();
    UpdateToolTips();
    RefreshPreviewTabs();

    AppendLog("[OK] Profile JSON applied to UI.");
  }

  private static void ApplyPhpSyntaxHighlighting(RichTextBox editor) {
    int selectionStart = editor.SelectionStart;
    int selectionLength = editor.SelectionLength;

    editor.SuspendLayout();

    editor.SelectAll();
    editor.SelectionColor = Color.Gainsboro;

    HighlightRegex(
        editor,
        @"<\?php|\?>",
        Color.DeepSkyBlue
    );

    HighlightRegex(
        editor,
        @"\b(declare|strict_types|foreach|if|else|elseif|endif|endforeach|as|function|return|continue|static|array|true|false|null|count|scandir|in_array|is_dir|rawurlencode|htmlspecialchars|ENT_QUOTES|UTF-8|glob|basename|usort|strcasecmp)\b",
        Color.DeepSkyBlue
    );

    HighlightRegex(
        editor,
        @"\$[a-zA-Z_][a-zA-Z0-9_]*",
        Color.LightGreen
    );

    HighlightRegex(
        editor,
        @"""[^""]*""|'[^']*'",
        Color.Orange
    );

    HighlightRegex(
        editor,
        @"//.*?$|#.*?$",
        Color.Gray,
        RegexOptions.Multiline
    );

    HighlightRegex(
        editor,
        @"<[^>]+>",
        Color.LightSkyBlue
    );

    editor.SelectionStart = Math.Min(selectionStart,editor.TextLength);
    editor.SelectionLength = Math.Min(selectionLength,editor.TextLength - editor.SelectionStart);
    editor.SelectionColor = Color.Gainsboro;

    editor.ResumeLayout();
  }

  private static void ApplyWinScpSyntaxHighlighting(RichTextBox editor) {
    int selectionStart = editor.SelectionStart;
    int selectionLength = editor.SelectionLength;

    editor.SuspendLayout();

    editor.SelectAll();
    editor.SelectionColor = Color.Gainsboro;

    HighlightRegex(
        editor,
        @"^\s*(option|open|cd|lcd|put|mkdir|exit)\b",
        Color.DeepSkyBlue,
        RegexOptions.Multiline | RegexOptions.IgnoreCase
    );

    HighlightRegex(
        editor,
        @"""[^""]*""",
        Color.Orange
    );

    HighlightRegex(
        editor,
        @"#.*?$",
        Color.Gray,
        RegexOptions.Multiline
    );

    HighlightRegex(
        editor,
        @"\b(sftp|ftp|ftpes)://[^\s""]+",
        Color.LightGreen
    );

    HighlightRegex(
        editor,
        @"\*\.png|\*\.exe|\*\.msi|\*\.php",
        Color.Khaki
    );

    editor.SelectionStart = Math.Min(selectionStart,editor.TextLength);
    editor.SelectionLength = Math.Min(selectionLength,editor.TextLength - editor.SelectionStart);
    editor.SelectionColor = Color.Gainsboro;

    editor.ResumeLayout();
  }

  private static void HighlightRegex(
    RichTextBox editor,
    string pattern,
    Color color,
    RegexOptions options = RegexOptions.None) {
    foreach (Match match in Regex.Matches(editor.Text,pattern,options)) {
      editor.Select(match.Index,match.Length);
      editor.SelectionColor = color;
    }
  }

  private void CreatePreviewTab(string fileName) {
    TabPage page = new() {
      Text = fileName
    };

    RichTextBox editor = new() {
      Dock = DockStyle.Fill,
      Font = new System.Drawing.Font("Consolas",10),
      WordWrap = false,
      AcceptsTab = true
    };

    editor.ContextMenuStrip = CreateEditorContextMenu(editor);

    page.Controls.Add(editor);

    tabPreview.TabPages.Add(page);

    _previewEditors[fileName] = editor;
    _editorFileNames[editor] = fileName;
  }

  private void InitializePreviewTabs() {
    CreatePreviewTab("Package.wxs");
    CreatePreviewTab("Folders.wxs");
    CreatePreviewTab("Components.wxs");
    CreatePreviewTab("Package.fr-fr.wxl");
    CreatePreviewTab("Bundle.wxs");
    CreatePreviewTab("WebBundle.wxs");
    CreatePreviewTab("WinSCP upload script");

    CreatePreviewTab("Catalog index.php");
    CreatePreviewTab("download.php");
    CreatePreviewTab("msi-software-packager index.php");

    CreatePreviewTab("Profile.msi.json");
    CreatePreviewTab("Build History");
    CreatePreviewTab("Environment Check");
    CreatePreviewTab("Release Checklist");
    CreatePreviewTab("Release Summary");
  }

  private static void SetPlainPreviewText(
    RichTextBox editor,
    string content) {
    editor.SuspendLayout();

    editor.Clear();
    editor.Text = content;
    editor.SelectionStart = 0;
    editor.SelectionLength = 0;

    editor.ResumeLayout();
  }

  private string GenerateWinScpUploadScriptPreview() {
    ApplyUiToProfile();

    string protocol =
        GetWinScpProtocol(_profile.Upload.Protocol);

    string host = _profile.Upload.Host.Trim();

    string userName = _profile.Upload.UserName.Trim();

    string openCommand = $"{protocol}://{userName}:********@{host}/";

    string localSiteRoot =
        Path.GetFullPath(
            _profile.Upload.LocalDirectory
        ).TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar
        );

    string remoteSiteRoot =
        _profile.Upload.RemoteDirectory
            .Trim()
            .Replace("\\","/")
            .TrimEnd('/');

    if (string.IsNullOrWhiteSpace(remoteSiteRoot)) {
      remoteSiteRoot = "/";
    }

    if (!remoteSiteRoot.StartsWith('/')) {
      remoteSiteRoot = "/" + remoteSiteRoot;
    }

    string productFolder =
        GetWebProductFolderName();

    string localPublicDir =
        Path.Combine(
            localSiteRoot,
            "public"
        );

    string localSoftwaresDir =
        Path.Combine(
            localPublicDir,
            "softwares"
        );

    string localAssetsDir =
        Path.Combine(
            localSoftwaresDir,
            WebAssetsFolderName
        );

    string localMsiSoftwarePackagerDir =
        Path.Combine(
            localSoftwaresDir,
            "msi-software-packager"
        );

    string localProjectDir =
        Path.Combine(
            localMsiSoftwarePackagerDir,
            productFolder
        );

    string remotePublicDir =
        CombineRemotePath(
            remoteSiteRoot,
            "public"
        );

    string remoteSoftwaresDir =
        CombineRemotePath(
            remotePublicDir,
            "softwares"
        );

    string remoteAssetsDir =
        CombineRemotePath(
            remoteSoftwaresDir,
            WebAssetsFolderName
        );

    string remoteMsiSoftwarePackagerDir =
        CombineRemotePath(
            remoteSoftwaresDir,
            "msi-software-packager"
        );

    string remoteProjectDir =
        CombineRemotePath(
            remoteMsiSoftwarePackagerDir,
            productFolder
        );
    StringBuilder script = new();

    script.AppendLine("option batch abort");
    script.AppendLine("option confirm off");
    script.AppendLine();

    script.AppendLine($"open \"{openCommand}\"");
    script.AppendLine();

    script.AppendLine("# ==================================================");
    script.AppendLine("# Bootstrap remote tree");
    script.AppendLine("# Creates missing remote directories without mkdir/call");
    script.AppendLine("# ==================================================");

    script.AppendLine($"synchronize remote \"{localPublicDir}\" \"{remotePublicDir}\"");
    script.AppendLine();

    script.AppendLine("# ==================================================");
    script.AppendLine("# Upload /public/softwares/index.php");
    script.AppendLine("# ==================================================");
    script.AppendLine($"cd \"{remoteSoftwaresDir}\"");
    script.AppendLine($"lcd \"{localSoftwaresDir}\"");
    script.AppendLine($"put \"{WebCatalogFileName}\"");
    script.AppendLine();

    script.AppendLine("# ==================================================");
    script.AppendLine("# Synchronize assets");
    script.AppendLine("# ==================================================");
    script.AppendLine(
        $"synchronize remote -delete \"{localAssetsDir}\" \"{remoteAssetsDir}\""
    );
    script.AppendLine();

    script.AppendLine("# ==================================================");
    script.AppendLine("# Upload /public/softwares/msi-software-packager/index.php");
    script.AppendLine("# ==================================================");
    script.AppendLine($"cd \"{remoteMsiSoftwarePackagerDir}\"");
    script.AppendLine($"lcd \"{localMsiSoftwarePackagerDir}\"");
    script.AppendLine($"put \"{WebCatalogFileName}\"");
    script.AppendLine();

    script.AppendLine("# ==================================================");
    script.AppendLine("# Synchronize current project files");
    script.AppendLine("# ==================================================");
    script.AppendLine(
        $"synchronize remote -delete \"{localProjectDir}\" \"{remoteProjectDir}\""
    );
    script.AppendLine();

    script.AppendLine("exit");

    return script.ToString();
  }

  private void btnBrowseWixOutputDir_Click(object? sender,EventArgs e) {
    using FolderBrowserDialog dialog = new();

    dialog.Description = "Select WiX output directory";
    dialog.ShowNewFolderButton = true;

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    txtWixOutputDir.Text = dialog.SelectedPath;
  }

  private void btnBrowseIconPath_Click(object? sender,EventArgs e) {
    using OpenFileDialog dialog = new();

    dialog.Title = "Select application icon";
    dialog.Filter = "Icon files (*.ico)|*.ico|All files (*.*)|*.*";

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    txtIconPath.Text = dialog.FileName;
  }

  private string GetVersionedProfileFileName() {
    string productName =
        string.IsNullOrWhiteSpace(_profile.Product.ProductName)
            ? "Profile"
            : _profile.Product.ProductName;

    string version =
        string.IsNullOrWhiteSpace(_profile.Product.Version)
            ? "0.0.0"
            : _profile.Product.Version;

    foreach (char c in Path.GetInvalidFileNameChars()) {
      productName = productName.Replace(c,'_');
      version = version.Replace(c,'_');
    }

    return $"{productName}_v{version}.msi.json";
  }

  private void btnGenerateWix_Click(object? sender,EventArgs e) {
    if (!ValidateProfileBeforeBuild())
      return;
    AutoSaveProfile();

    string templateRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Templates");

    WixProjectGenerator.Generate(_profile,templateRoot,_profile.Output.WixOutputDirectory);

    AppendLog("WiX files generated.");
    RefreshPreviewTabs();
  }

  public object BuildLogLock => _buildLogLock;

  private void ArchiveAndCleanPreviousBuildTraces() {
    string buildRoot =
        Path.GetDirectoryName(_profile.Output.MsiOutputDirectory)!;

    string logsDir =
        Path.Combine(buildRoot,"Logs");

    string reportsDir =
        Path.Combine(buildRoot,"Reports");

    string archivesDir =
        Path.Combine(buildRoot,"Archives");

    Directory.CreateDirectory(logsDir);
    Directory.CreateDirectory(reportsDir);
    Directory.CreateDirectory(archivesDir);

    bool hasOldFiles =
        Directory.GetFiles(logsDir,"*.txt").Length > 0 ||
        Directory.GetFiles(reportsDir,"*.txt").Length > 0;

    if (hasOldFiles) {
      string archivePath =
          Path.Combine(
              archivesDir,
              $"MSI_BUILD_ARCHIVE_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
          );

      using ZipArchive archive =
          ZipFile.Open(
              archivePath,
              ZipArchiveMode.Create
          );

      foreach (string file in Directory.GetFiles(logsDir,"*.txt")) {
        archive.CreateEntryFromFile(
            file,
            Path.Combine("Logs",Path.GetFileName(file))
        );
      }

      foreach (string file in Directory.GetFiles(reportsDir,"*.txt")) {
        archive.CreateEntryFromFile(
            file,
            Path.Combine("Reports",Path.GetFileName(file))
        );
      }
    }

    foreach (string file in Directory.GetFiles(logsDir,"*.txt"))
      File.Delete(file);

    foreach (string file in Directory.GetFiles(reportsDir,"*.txt"))
      File.Delete(file);
  }

  private void ShowBuildCompletedMessage() {
    string message =
        "Build completed successfully." +
        Environment.NewLine +
        Environment.NewLine +
        "MSI, bundle, web installer, reports and archives have been generated.";

    if (_profile.Upload.UploadWebFilesAfterBuild) {
      message +=
          Environment.NewLine +
          Environment.NewLine +
          (_lastUploadSuccess
              ? "Web upload completed successfully."
              : "Web upload was enabled but did not complete successfully.");
    }

    MessageBox.Show(
        this,
        message,
        "msi-software-packager",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information
    );
  }

  private async void btnBuildMsi_Click(object? sender,EventArgs e) {
    ApplyUiToProfile();
    RefreshMsiDownloadUrl();
    SyncUpdateManifestFromProfile();
    ApplyUiToProfile();

    if (!ValidateProfileBeforeBuild())
      return;

    AutoSaveProfile();

    ArchiveAndCleanPreviousBuildTraces();
    InitializeBuildLog();

    CleanBuildArtifacts();

    string templateRoot =
        Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Templates"
        );

    MsiBuildService service = new();

    service.LogChanged += AppendLog;

    btnBuildMsi.Enabled = false;
    btnGenerateWix.Enabled = false;

    try {
      await Task.Run(() => {
        service.Build(_profile,templateRoot);
      });

      PrepareWebPublishFolder();

      RunWinScpUpload();

      AppendLog("[SUCCESS] MSI build completed.");

      SaveMsiBuildReport(
          success: true,
          message: "MSI generated successfully."
      );

      ShowBuildCompletedMessage();

      SaveReleaseReports();
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);
      AppendLog("[FAILED] MSI build failed.");

      SaveMsiBuildReport(
          success: false,
          message: ex.Message
      );

      MessageBox.Show(
          this,
          "Build failed." +
          Environment.NewLine +
          Environment.NewLine +
          ex.Message,
          "msi-software-packager",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
    finally {
      _buildFileLoggingEnabled = false;

      btnBuildMsi.Enabled = true;
      btnGenerateWix.Enabled = true;
    }
  }

  private void CleanBuildArtifacts() {
    AppendLog("[INFO] Cleaning previous build artifacts...");

    string[] directoriesToClean =
    [
        _profile.Output.WixOutputDirectory,
        _profile.Output.MsiOutputDirectory,
        _profile.Publish.PublishDirectory,
        _profile.Bundle.BundleOutputDirectory,
        _profile.WebInstaller.WebOutputDirectory
    ];

    foreach (string directory in directoriesToClean) {
      if (string.IsNullOrWhiteSpace(directory))
        continue;

      string fullPath = Path.GetFullPath(directory);

      if (!Directory.Exists(fullPath))
        continue;

      foreach (string file in Directory.GetFiles(fullPath)) {
        File.Delete(file);
      }

      foreach (string subDir in Directory.GetDirectories(fullPath)) {
        Directory.Delete(subDir,recursive: true);
      }

      AppendLog("[INFO] Cleaned : " + fullPath);
    }

    AppendLog("[INFO] Build artifacts cleaned.");
  }

  private void AppendLog(string line) {
    if (InvokeRequired) {
      BeginInvoke(new Action<string>(AppendLog),line);
      return;
    }

    if (string.IsNullOrWhiteSpace(line)) {
      txtLog.AppendText(Environment.NewLine);
      AppendBuildLogFile(string.Empty);
      return;
    }

    string stampedLine =
            TimestampPrefixRegex().IsMatch(line)
                ? line
                : $"[{DateTime.Now:HH:mm:ss}] {line}";

    txtLog.AppendText(
        stampedLine + Environment.NewLine
    );

    txtLog.SelectionStart = txtLog.TextLength;
    txtLog.ScrollToCaret();

    AppendBuildLogFile(stampedLine);
  }

  private void btnLoadProfile_Click(object? sender,EventArgs e) {
    using OpenFileDialog dialog = new();

    dialog.Title = "Load MSI profile";
    dialog.InitialDirectory = @"E:\msi-software-packager\Profiles";
    dialog.Filter =
        "MSI profile (*.msi.json)|*.msi.json|JSON files (*.json)|*.json|All files (*.*)|*.*";

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    try {
      _isLoadingProfile = true;

      _profile = ProfileSerializer.Load(dialog.FileName);
      _currentProfilePath = dialog.FileName;

      ApplyProfileToUi();
      _profileHasUnsavedChanges = false;
    }
    catch (Exception ex) {
      MessageBox.Show(
          this,
          ex.Message,
          "Load profile",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );

      AppendLog("[ERROR] " + ex.Message);
      return;
    }
    finally {
      _isLoadingProfile = false;
    }

    ApplyUiToProfile();
    RefreshMsiDownloadUrl();

    UpdateBundleUiState();
    UpdateWebInstallerUiState();
    UpdateWebUploadUiState();
    UpdateToolTips();
    RefreshPreviewTabs();

    AppendLog("Profile loaded : " + dialog.FileName);
  }

  private void btnSaveProfile_Click(object? sender,EventArgs e) {
    SaveCurrentProfile();
  }

  private static MsiPackageProfile CreateDefaultProfile() {
    return new MsiPackageProfile {
      Product =
        {
            ProductName = "NewProduct",
            Manufacturer = "PB BZH Concept",
            Version = "1.0.0",
            UpgradeCode = Guid.NewGuid().ToString().ToUpperInvariant()
        },

      Output =
        {
            MsiFileName = "NewProduct.msi",
            MsiOutputDirectory = @"E:\msi-software-packager\Build\Installer",
            WixOutputDirectory = @"E:\msi-software-packager\Build\Wix"
        },

      WebInstaller =
        {
            WebBundleName = "NewProduct",
            WebSetupFileName = "NewProductWebSetup.exe",
            WebPublishDirectory = @"C:\wamp64\www\pb-bzh-concept.fr\public\softwares"
        },

      Upload =
        {
            Protocol = "SFTP",
            LocalDirectory = @"C:\wamp64\www\pb-bzh-concept.fr",
            RemoteDirectory = "/",
            WebRemoteSite = "https://www.pb-bzh-concept.fr/softwares/",
            WinScpPath = @"C:\Program Files (x86)\WinSCP\WinSCP.com"
        }
    };
  }

  private void AutoSaveProfile() {
    if (_isLoadingProfile)
      return;

    if (string.IsNullOrWhiteSpace(_currentProfilePath))
      return;

    ApplyUiToProfile();

    ProfileSerializer.Save(
        _currentProfilePath,
        _profile
    );

    _profileHasUnsavedChanges = false;
  }

  private string GenerateWinScpConnectionTestScript() {
    string buildRoot =
        Path.GetDirectoryName(_profile.Output.MsiOutputDirectory)!;

    string uploadDir =
        Path.Combine(buildRoot,"Upload");

    Directory.CreateDirectory(uploadDir);

    string scriptPath =
        Path.Combine(uploadDir,"winscp-test.txt");

    string protocol =
        _profile.Upload.Protocol.Trim().ToLowerInvariant();

    string host =
        _profile.Upload.Host.Trim();

    string userName =
        _profile.Upload.UserName.Trim();

    string password =
        txtUploadPassword.Text;

    string remoteDir =
        _profile.Upload.RemoteDirectory.Trim().Replace("\\","/");

    string escapedPassword =
        Uri.EscapeDataString(password);

    string openCommand =
        $"{protocol}://{userName}:{escapedPassword}@{host}/";

    StringBuilder script = new();

    script.AppendLine("option batch abort");
    script.AppendLine("option confirm off");
    script.AppendLine();
    script.AppendLine($"open \"{openCommand}\"");
    script.AppendLine($"cd \"{remoteDir}\"");
    script.AppendLine("pwd");
    script.AppendLine("exit");

    File.WriteAllText(
        scriptPath,
        script.ToString(),
        EncodingHelper.Utf8Bom
    );

    return scriptPath;
  }

  private bool ConfirmDiscardOrSaveCurrentProfile() {
    if (!_profileHasUnsavedChanges)
      return true;

    DialogResult result =
        MessageBox.Show(
            this,
            "Do you want to save the current profile before creating a new one?",
            "New profile",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question
        );

    if (result == DialogResult.Cancel)
      return false;

    if (result == DialogResult.Yes)
      return SaveCurrentProfile();

    return true;
  }

  private bool SaveCurrentProfile() {
    ApplyUiToProfile();
    SyncUpdateManifestFromProfile();

    if (string.IsNullOrWhiteSpace(_currentProfilePath)) {
      using SaveFileDialog dialog = new();

      dialog.Title = "Save profile";
      dialog.Filter = "msi-software-packager profile (*.json)|*.json|All files (*.*)|*.*";
      dialog.InitialDirectory = @"E:\msi-software-packager\Profiles";
      dialog.FileName =
          $"{_profile.Output.MsiFileName}.json";

      if (dialog.ShowDialog(this) != DialogResult.OK)
        return false;

      _currentProfilePath = dialog.FileName;
    }

    ProfileSerializer.Save(
        _currentProfilePath,
        _profile
    );
    _profileHasUnsavedChanges = false;
    AppendLog("[INFO] Profile saved : " + _currentProfilePath);

    return true;
  }

  private void btnGenerateUpgradeCode_Click(object? sender,EventArgs e) {
    txtUpgradeCode.Text = Guid.NewGuid().ToString().ToUpperInvariant();
    AppendLog("UpgradeCode generated.");
    AutoSaveProfile();
  }

  private void btnBrowseProjectFile_Click(object? sender,EventArgs e) {
    using OpenFileDialog dialog = new();

    dialog.Title = "Select .csproj";
    dialog.Filter =
        "C# Project (*.csproj)|*.csproj|All files (*.*)|*.*";

    dialog.InitialDirectory =
        Directory.Exists(@"G:\OneDrive")
            ? @"G:\OneDrive"
            : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    txtProjectFile.Text = dialog.FileName;

    txtExecutableName.Text = Path.GetFileNameWithoutExtension(dialog.FileName) + ".exe";

    string projectName = Path.GetFileNameWithoutExtension(dialog.FileName);

    txtProductName.Text = projectName;
    txtExecutableName.Text = projectName + ".exe";
    txtPublishDir.Text = Path.Combine(Path.GetDirectoryName(dialog.FileName)!,"publish");

    txtMsiOutputDir.Text = Path.Combine(Path.GetDirectoryName(dialog.FileName)!,"installer");
  }

  private void btnBrowsePublishDir_Click(object? sender,EventArgs e) {
    using FolderBrowserDialog dialog = new();

    dialog.Description = "Select publish directory";

    dialog.ShowNewFolderButton = true;

    if (dialog.ShowDialog(this)
        != DialogResult.OK)
      return;

    txtPublishDir.Text =
        dialog.SelectedPath;
  }

  private void btnBrowseMsiOutputDir_Click(object? sender,EventArgs e) {
    using FolderBrowserDialog dialog = new();

    dialog.Description = "Select MSI\noutput directory";

    dialog.ShowNewFolderButton = true;

    if (dialog.ShowDialog(this)
        != DialogResult.OK)
      return;

    txtMsiOutputDir.Text = dialog.SelectedPath;
  }

  private void btnBrowseBundleDir_Click(object sender,EventArgs e) {
    using FolderBrowserDialog dialog = new();

    dialog.Description = "Select Bundle\noutput directory";

    dialog.ShowNewFolderButton = true;

    if (dialog.ShowDialog(this)
        != DialogResult.OK)
      return;

    txtBundleOutputDir.Text = dialog.SelectedPath;
  }

  private void btnBrowseWebOutputDir_Click(object? sender,EventArgs e) {
    using FolderBrowserDialog dialog = new();
    dialog.Description = "Select web installer\noutput directory";
    dialog.ShowNewFolderButton = true;
    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;
    txtWebOutputDir.Text = dialog.SelectedPath;
    AutoSaveProfile();
  }

  private void btnGenerateWebBundleUpgradeCode_Click(object? sender,EventArgs e) {
    if (!string.IsNullOrWhiteSpace(txtWebBundleUpgradeCode.Text))
      return;

    txtWebBundleUpgradeCode.Text =
        Guid.NewGuid().ToString().ToUpperInvariant();

    AutoSaveProfile();

    AppendLog("[INFO] Web bundle UpgradeCode generated.");
  }

  private void btnUploadNow_Click(object? sender,EventArgs e) {
    try {
      RunWinScpUpload();
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          ex.Message,
          "Upload",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void btnTestUploadConnection_Click(object? sender,EventArgs e) {
    try {
      if (!ValidateUploadOptions())
        return;

      string scriptPath = GenerateWinScpConnectionTestScript();

      AppendLog("[INFO] Testing upload connection...");

      ProcessStartInfo psi = new() {
        FileName = _profile.Upload.WinScpPath,
        Arguments = $"/script=\"{scriptPath}\"",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8
      };

      using Process process = new() {
        StartInfo = psi
      };

      process.OutputDataReceived += (_,ev) => {
        if (!string.IsNullOrWhiteSpace(ev.Data))
          AppendLog("[WinSCP] " + ev.Data);
      };

      process.ErrorDataReceived += (_,ev) => {
        if (!string.IsNullOrWhiteSpace(ev.Data))
          AppendLog("[WinSCP ERROR] " + ev.Data);
      };

      process.Start();
      process.BeginOutputReadLine();
      process.BeginErrorReadLine();
      process.WaitForExit();

      if (process.ExitCode != 0)
        throw new InvalidOperationException(
            "Connection test failed. Exit code: " +
            process.ExitCode
        );

      AppendLog("[SUCCESS] Connection test successful.");
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          ex.Message,
          "Connection test",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void btnBrowseUploadLocalDir_Click(object sender,EventArgs e) {
    using FolderBrowserDialog dialog = new();

    dialog.Description = "Select upload\nlocal directory";

    dialog.ShowNewFolderButton = true;

    if (dialog.ShowDialog(this)
        != DialogResult.OK)
      return;

    txtUploadLocalDir.Text = dialog.SelectedPath;
  }

  private void btnBrowseWinScpPath_Click(object sender,EventArgs e) {
    using OpenFileDialog dialog = new();

    dialog.Title = "Select WinSCP executable";
    dialog.Filter =
        "WinSCP console executable (WinSCP.com)|WinSCP.com|" +
        "WinSCP executable (WinSCP.exe)|WinSCP.exe|" +
        "Executable files (*.exe;*.com)|*.exe;*.com|" +
        "All files (*.*)|*.*";

    dialog.CheckFileExists = true;
    dialog.CheckPathExists = true;

    string defaultPath =
        @"C:\Program Files (x86)\WinSCP";

    if (Directory.Exists(defaultPath)) {
      dialog.InitialDirectory = defaultPath;
    }
    else if (!string.IsNullOrWhiteSpace(txtWinScpPath.Text)) {
      string? currentDir =
          Path.GetDirectoryName(txtWinScpPath.Text);

      if (!string.IsNullOrWhiteSpace(currentDir) &&
          Directory.Exists(currentDir)) {
        dialog.InitialDirectory = currentDir;
      }
    }

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    txtWinScpPath.Text = dialog.FileName;

    AutoSaveProfile();
  }

  private void btnWebPublisDir_Click(object sender,EventArgs e) {
    using FolderBrowserDialog dialog = new();

    dialog.Description = "Select web publish directory";

    dialog.ShowNewFolderButton = true;

    if (dialog.ShowDialog(this)
        != DialogResult.OK)
      return;

    txtWebPublishDirectory.Text = dialog.SelectedPath;
  }

  private bool SaveCurrentProfileAs() {
    ApplyUiToProfile();
    SyncUpdateManifestFromProfile();

    using SaveFileDialog dialog = new();

    dialog.Title = "Save profile as";
    dialog.Filter =
        "MsiPackager profile (*.json)|*.json|All files (*.*)|*.*";

    string profilesDir =
        @"E:\MsiPackager\Profiles";

    if (Directory.Exists(profilesDir)) {
      dialog.InitialDirectory = profilesDir;
    }

    string suggestedFileName =
        BuildProfileFileName();

    dialog.FileName = suggestedFileName;

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return false;

    string selectedPath =
        dialog.FileName;

    ProfileSerializer.Save(
        selectedPath,
        _profile
    );

    _currentProfilePath =
        selectedPath;

    AppendLog(
        "[INFO] Profile saved as : " +
        selectedPath
    );

    return true;
  }

  private string BuildProfileFileName() {
    string productName =
        string.IsNullOrWhiteSpace(_profile.Product.ProductName)
            ? "Product"
            : _profile.Product.ProductName.Trim();

    string version =
        string.IsNullOrWhiteSpace(_profile.Product.Version)
            ? "1.0.0"
            : _profile.Product.Version.Trim();

    string fileName =
        $"{productName}_v{version}.msi.json";

    foreach (char c in Path.GetInvalidFileNameChars()) {
      fileName =
          fileName.Replace(c,'_');
    }

    return fileName;
  }

  private void btnSaveProfileAs_Click(object? sender,EventArgs e) {
    try {
      SaveCurrentProfileAs();
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Save profile as",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void btnNewProfile_Click(object? sender,EventArgs e) {
    try {
      if (!ConfirmDiscardOrSaveCurrentProfile())
        return;

      _isLoadingProfile = true;

      _profile = CreateDefaultProfile();
      _profileHasUnsavedChanges = false;
      ApplyGlobalSettingsToProfileDefaults();
      AppendLog("[INFO] Global settings applied to new profile.");

      _currentProfilePath = null;

      ApplyProfileToUi();
      _profileHasUnsavedChanges = false;
      AskConfigureSigningForNewProfile();

      UpdateBundleUiState();
      UpdateWebInstallerUiState();
      UpdateWebUploadUiState();
      UpdateToolTips();
      RefreshPreviewTabs();

      AppendLog("[INFO] New profile created.");
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "New profile",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
    finally {
      _isLoadingProfile = false;
      _profileHasUnsavedChanges = false;
    }
  }

  private void mnuPrintSelectedView_Click(object sender,EventArgs e) {
    if (!TryGetSelectedPreviewEditor(out RichTextBox textBox)) {
      MessageBox.Show(
          "Aucune vue sélectionnée.",
          "Impression",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );
      return;
    }

    if (string.IsNullOrWhiteSpace(textBox.Text)) {
      MessageBox.Show(
          "Aucun contenu à imprimer.",
          "Impression",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );
      return;
    }

    _printLines = textBox.Text.Replace("\r\n","\n").Split('\n');
    _currentPrintLine = 0;

    _printDocument = new PrintDocument {
      DocumentName = tabPreview.SelectedTab?.Text ?? "preview"
    };

    _printDocument.PrintPage += PrintDocument_PrintPage;

    using PrintDialog dialog = new() {
      Document = _printDocument,
      UseEXDialog = true
    };

    if (dialog.ShowDialog() == DialogResult.OK)
      _printDocument.Print();
  }

  private async Task OpenEditorAndReloadIfNeededAsync(RichTextBox editor) {
    try {
      string fileName =
          _editorFileNames.TryGetValue(editor,out string? knownName)
              ? knownName
              : "Preview.txt";

      string tempFile = Path.Combine(
          Path.GetTempPath(),
          fileName
      );

      File.WriteAllText(tempFile,editor.Text,EncodingHelper.Utf8NoBom);

      string? notepadPlusPlus = FindNotepadPlusPlus();

      ProcessStartInfo startInfo =
          notepadPlusPlus != null
              ? new ProcessStartInfo {
                FileName = notepadPlusPlus,
                Arguments = $"\"{tempFile}\"",
                UseShellExecute = true
              }
              : new ProcessStartInfo {
                FileName = tempFile,
                UseShellExecute = true
              };

      Process? process = Process.Start(startInfo);

      if (process == null)
        return;

      await Task.Run(() => process.WaitForExit());

      string editedText = File.ReadAllText(tempFile);

      SetPlainPreviewText(editor,editedText);
      ApplyPreviewSyntaxHighlighting(editor,fileName);

      if (fileName.Equals("Profile.msi.json",StringComparison.OrdinalIgnoreCase)) {
        ApplyProfileJsonTextToUi(editedText);
      }
      else {
        AppendLog("[OK] Preview file reloaded from editor: " + fileName);
      }
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Edit selected view",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void ApplyProfileJsonTextToUi(string json) {
    if (string.IsNullOrWhiteSpace(json)) {
      MessageBox.Show(
          "Le profil JSON est vide.",
          "Profile JSON",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );
      return;
    }

    try {
      _isLoadingProfile = true;

      _profile = ProfileSerializer.FromJson(json);

      ApplyProfileToUi();
    }
    catch (Exception ex) {
      MessageBox.Show(
          this,
          "Le JSON du profil est invalide." +
          Environment.NewLine +
          Environment.NewLine +
          ex.Message,
          "Profile JSON",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );

      AppendLog("[ERROR] Invalid profile JSON: " + ex.Message);
      return;
    }
    finally {
      _isLoadingProfile = false;
    }

    ApplyUiToProfile();
    RefreshMsiDownloadUrl();

    UpdateBundleUiState();
    UpdateWebInstallerUiState();
    UpdateWebUploadUiState();
    UpdateToolTips();
    RefreshPreviewTabs();

    AppendLog("[OK] Profile JSON reloaded from Notepad++.");
  }

  private void ShowValidationResult(ProfileValidationResult result) {
    StringBuilder sb = new();

    if (result.Errors.Count > 0) {
      sb.AppendLine("ERRORS");
      sb.AppendLine("------");

      foreach (string error in result.Errors)
        sb.AppendLine("[ERROR] " + error);

      sb.AppendLine();
    }

    if (result.Warnings.Count > 0) {
      sb.AppendLine("WARNINGS");
      sb.AppendLine("--------");

      foreach (string warning in result.Warnings)
        sb.AppendLine("[WARN] " + warning);

      sb.AppendLine();
    }

    if (result.Infos.Count > 0) {
      sb.AppendLine("INFO");
      sb.AppendLine("----");

      foreach (string info in result.Infos)
        sb.AppendLine("[INFO] " + info);

      sb.AppendLine();
    }

    if (sb.Length == 0)
      sb.AppendLine("[OK] Profile validation successful.");

    AppendLog(sb.ToString());

    MessageBoxIcon icon =
        result.HasErrors
            ? MessageBoxIcon.Error
            : result.HasWarnings
                ? MessageBoxIcon.Warning
                : MessageBoxIcon.Information;

    MessageBox.Show(
        this,
        sb.ToString(),
        "Validate package profile",
        MessageBoxButtons.OK,
        icon
    );
  }

  private async void mnuEditSelectedViewNoteapad_Click(object sender,EventArgs e) {
    if (!TryGetSelectedPreviewEditor(out RichTextBox textBox)) {
      MessageBox.Show(
          "Aucune vue sélectionnée.",
          "Edition",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );
      return;
    }

    if (string.IsNullOrWhiteSpace(textBox.Text)) {
      MessageBox.Show(
          "Aucun contenu à éditer.",
          "Edition",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );
      return;
    }

    await OpenEditorAndReloadIfNeededAsync(textBox);
  }

  private void mnuApplyProfileJsonToUi_Click(object sender,EventArgs e) {
    ApplyProfileJsonPreviewToUi();
  }

  private void mnuExit_Click(object sender,EventArgs e) {
    DialogResult result = MessageBox.Show(
        this,
        "Voulez-vous sauvegarder le profil actuel avant de quitter ?",
        "Quitter",
        MessageBoxButtons.YesNoCancel,
        MessageBoxIcon.Question
    );

    if (result == DialogResult.Cancel)
      return;

    if (result == DialogResult.Yes) {
      ApplyUiToProfile();
      RefreshMsiDownloadUrl();
      ApplyUiToProfile();

      AutoSaveProfile();
    }

    Close();
  }

  private void mnuNewProfileFromCsproj_Click(object? sender,EventArgs e) {
    using OpenFileDialog dialog = new();

    dialog.Title = "Select C# project file";
    dialog.Filter = "C# project (*.csproj)|*.csproj|All files (*.*)|*.*";

    dialog.InitialDirectory = Directory.Exists(@"G:\OneDrive")
      ? @"G:\OneDrive"
      : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    try {
      _isLoadingProfile = true;

      _profile = CsprojProfileFactory.CreateFromCsproj(dialog.FileName);

      DialogResult applyGlobals = MessageBox.Show(
          this,
          "Do you want to apply Global Settings to this new profile?" +
          Environment.NewLine +
          Environment.NewLine +
          "This will apply common build paths, upload settings, WinSCP path, SignTool path and signing defaults.",
          "New profile from .csproj",
          MessageBoxButtons.YesNo,
          MessageBoxIcon.Question
      );

      if (applyGlobals == DialogResult.Yes) {
        ApplyGlobalSettingsToProfileDefaults();
        AppendLog("[INFO] Global settings applied to new profile from .csproj.");
      }
      else {
        AppendLog("[INFO] Global settings not applied to new profile from .csproj.");
      }

      _currentProfilePath = null;

      ApplyProfileToUi();

      AskConfigureSigningForNewProfile();
    }
    catch (Exception ex) {
      MessageBox.Show(
          this,
          ex.Message,
          "New profile from .csproj",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );

      AppendLog("[ERROR] " + ex.Message);
      return;
    }
    finally {
      _isLoadingProfile = false;
    }

    ApplyUiToProfile();
    RefreshMsiDownloadUrl();
    ApplyUiToProfile();

    UpdateBundleUiState();
    UpdateWebInstallerUiState();
    UpdateWebUploadUiState();
    UpdateToolTips();
    RefreshPreviewTabs();

    AppendLog("[INFO] Global settings applied to new profile from .csproj.");
    AppendLog("[OK] New profile created from .csproj : " + dialog.FileName);
  }

  private void mnuValidateProfile_Click(object? sender,EventArgs e) {
    ApplyUiToProfile();
    RefreshMsiDownloadUrl();
    ApplyUiToProfile();

    string templateRoot = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Templates"
    );

    ProfileValidationResult result =
        ProfileValidationService.Validate(_profile,templateRoot);

    ShowValidationResult(result);
  }

  private void mnuUpdateCsprojFromProfile_Click(object? sender,EventArgs e) {
    ApplyGlobalSettingsToProfileDefaults();

    ApplyUiToProfile();
    RefreshMsiDownloadUrl();
    ApplyUiToProfile();

    if (string.IsNullOrWhiteSpace(_profile.TargetProject.ProjectFile)) {
      MessageBox.Show(
          this,
          "Aucun fichier .csproj n'est défini dans le profil.",
          "Update .csproj",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );

      return;
    }

    DialogResult result = MessageBox.Show(
        this,
        "Voulez-vous reporter les informations du profil actuel dans le fichier .csproj ?" +
        Environment.NewLine +
        Environment.NewLine +
        _profile.TargetProject.ProjectFile,
        "Update .csproj from profile",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    );

    if (result != DialogResult.Yes)
      return;

    try {
      string backupPath = CsprojProfileFactory.UpdateCsprojFromProfile(_profile);

      AppendLog("[OK] .csproj updated from current profile : " +
                _profile.TargetProject.ProjectFile);

      AppendLog("[INFO] .csproj backup created : " + backupPath);

      MessageBox.Show(
          this,
          "Le fichier .csproj a été mis à jour." +
          Environment.NewLine +
          Environment.NewLine +
          "Sauvegarde créée :" +
          Environment.NewLine +
          backupPath,
          "Update .csproj",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
      );
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Update .csproj",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void mnuSaveUploadCredential_Click(object? sender,EventArgs e) {
    try {
      SaveUploadPasswordToCredentialManager();

      MessageBox.Show(
          this,
          "Le mot de passe a été enregistré dans le Gestionnaire d'identifiants Windows.",
          "Windows Credential Manager",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
      );
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Windows Credential Manager",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void mnuRestoreDefaultWinScpPath_Click(object? sender,EventArgs e) {
    RestoreDefaultWinScpPath();
  }

  private void mnuGenerateWebSiteOnly_Click(object? sender,EventArgs e) {
    try {
      ApplyUiToProfile();
      RefreshMsiDownloadUrl();
      ApplyUiToProfile();

      if (!ValidateWebOnlyBeforeAction())
        return;

      AppendLog("[INFO] Generate web site only started.");

      if (_profile.Android.PublishApk) {
        AndroidBuildService androidService = new();
        androidService.BuildApk(_profile,AppendLog);
      }

      PrepareWebPublishFolder();

      RefreshPreviewTabs();

      AppendLog("[SUCCESS] Web site generated successfully.");

      MessageBox.Show(
          this,
          "Web site generated successfully.",
          "Generate web site only",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
      );
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);
      AppendLog("[FAILED] Web site generation failed.");

      MessageBox.Show(
          this,
          "Web site generation failed." +
          Environment.NewLine +
          Environment.NewLine +
          ex.Message,
          "Generate web site only",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void mnuUploadWebSiteOnly_Click(object? sender,EventArgs e) {
    try {
      ApplyUiToProfile();
      RefreshMsiDownloadUrl();
      ApplyUiToProfile();

      if (!ValidateWebOnlyBeforeAction())
        return;

      AppendLog("[INFO] Upload web site only started.");

      RunWinScpUpload(forceUpload: true);

      AppendLog("[SUCCESS] Web site uploaded successfully.");

      MessageBox.Show(
          this,
          "Web site uploaded successfully.",
          "Upload web site only",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
      );
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);
      AppendLog("[FAILED] Web site upload failed.");

      MessageBox.Show(
          this,
          "Web site upload failed." +
          Environment.NewLine +
          Environment.NewLine +
          ex.Message,
          "Upload web site only",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void mnuOpenLocalSiteWeb_Click(object? sender,EventArgs e) {
    try {
      ApplyUiToProfile();

      string localDirectory = _profile.Upload.LocalDirectory;

      if (string.IsNullOrWhiteSpace(localDirectory)) {
        MessageBox.Show(
            this,
            "Le répertoire local du site web n'est pas défini.",
            "Open local web site folder",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );
        return;
      }

      string fullPath = Path.GetFullPath(localDirectory);

      if (!Directory.Exists(fullPath)) {
        DialogResult result = MessageBox.Show(
            this,
            "Le répertoire local du site web n'existe pas :" +
            Environment.NewLine +
            Environment.NewLine +
            fullPath +
            Environment.NewLine +
            Environment.NewLine +
            "Voulez-vous le créer ?",
            "Open local web site folder",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
          return;

        Directory.CreateDirectory(fullPath);
      }

      Process.Start(new ProcessStartInfo {
        FileName = "explorer.exe",
        Arguments = $"\"{fullPath}\"",
        UseShellExecute = true
      });

      AppendLog("[INFO] Local web site folder opened : " + fullPath);
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Open local web site folder",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void mnuRefreshBuildHistory_Click(object? sender,EventArgs e) {
    try {
      ApplyUiToProfile();

      if (!_previewEditors.TryGetValue("Build History",out RichTextBox? editor)) {
        MessageBox.Show(
            this,
            "Build History view was not found.",
            "Refresh build history",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );

        return;
      }

      string content = BuildBuildHistoryText();

      SetPlainPreviewText(editor,content);
      ApplyPreviewSyntaxHighlighting(editor,"Build History");

      foreach (TabPage tab in tabPreview.TabPages) {
        if (tab.Text.Equals("Build History",StringComparison.OrdinalIgnoreCase)) {
          tabPreview.SelectedTab = tab;
          break;
        }
      }

      AppendLog("[INFO] Build history refreshed.");
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Refresh build history",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void mnuOpenBuildArchiveFolder_Click(object? sender,EventArgs e) {
    try {
      ApplyUiToProfile();

      string buildRoot =
          Path.GetDirectoryName(_profile.Output.MsiOutputDirectory)
          ?? string.Empty;

      if (string.IsNullOrWhiteSpace(buildRoot)) {
        MessageBox.Show(
            this,
            "Unable to determine build root directory.",
            "Open build archive folder",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );

        return;
      }

      string archivesDir =
          Path.Combine(buildRoot,"Archives");

      if (!Directory.Exists(archivesDir)) {
        DialogResult result = MessageBox.Show(
            this,
            "Build archive folder does not exist:" +
            Environment.NewLine +
            Environment.NewLine +
            archivesDir +
            Environment.NewLine +
            Environment.NewLine +
            "Do you want to create it?",
            "Open build archive folder",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.Yes)
          return;

        Directory.CreateDirectory(archivesDir);
      }

      Process.Start(new ProcessStartInfo {
        FileName = "explorer.exe",
        Arguments = $"\"{archivesDir}\"",
        UseShellExecute = true
      });

      AppendLog("[INFO] Build archive folder opened : " + archivesDir);
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Open build archive folder",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private async void mnuCheckForUpdates_Click(object? sender,EventArgs e) {
    HelpHelper.mnuCheckForUpdates(this,_profile);
  }

  private void mnuAbout_Click(object? sender,EventArgs e) {
    HelpHelper.mnuAbout(this,_profile);
  }

  [GeneratedRegex(@"(open\s+""sftp://[^:""]+:)([^@""\r\n]+)(@[^""]+"")",RegexOptions.IgnoreCase,"fr-FR")]
  private static partial Regex OpenSftpPasswordRegex();
  [GeneratedRegex(@"^\[\d{2}:\d{2}:\d{2}\]")]
  private static partial Regex TimestampPrefixRegex();

  private static void CheckFile(
    StringBuilder report,
    ref bool issueFlag,
    string label,
    string filePath,
    bool required) {
    if (string.IsNullOrWhiteSpace(filePath)) {
      if (required) {
        report.AppendLine("[ERROR] " + label + " path is empty.");
        issueFlag = true;
      }
      else {
        report.AppendLine("[INFO] " + label + " path is empty.");
      }

      return;
    }

    string fullPath;

    try {
      fullPath = Path.GetFullPath(filePath);
    }
    catch {
      report.AppendLine("[ERROR] " + label + " path is invalid : " + filePath);
      issueFlag = true;
      return;
    }

    if (File.Exists(fullPath)) {
      report.AppendLine("[OK] " + label + " : " + fullPath);
      return;
    }

    if (required) {
      report.AppendLine("[ERROR] " + label + " not found : " + fullPath);
    }
    else {
      report.AppendLine("[WARN] " + label + " not found : " + fullPath);
    }

    issueFlag = true;
  }

  private static void CheckDirectory(
    StringBuilder report,
    ref bool issueFlag,
    string label,
    string directoryPath,
    bool required) {
    if (string.IsNullOrWhiteSpace(directoryPath)) {
      if (required) {
        report.AppendLine("[ERROR] " + label + " path is empty.");
        issueFlag = true;
      }
      else {
        report.AppendLine("[INFO] " + label + " path is empty.");
      }

      return;
    }

    string fullPath;

    try {
      fullPath = Path.GetFullPath(directoryPath);
    }
    catch {
      report.AppendLine("[ERROR] " + label + " path is invalid : " + directoryPath);
      issueFlag = true;
      return;
    }

    if (Directory.Exists(fullPath)) {
      report.AppendLine("[OK] " + label + " : " + fullPath);
      return;
    }

    if (required) {
      report.AppendLine("[ERROR] " + label + " not found : " + fullPath);
    }
    else {
      report.AppendLine("[WARN] " + label + " not found : " + fullPath);
    }

    issueFlag = true;
  }

  private static void CheckTemplates(
    StringBuilder report,
    ref bool hasError) {
    report.AppendLine();
    report.AppendLine("Templates");

    string templateRoot =
        Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Templates"
        );

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

      if (File.Exists(fullPath)) {
        report.AppendLine("[OK] Template : " + relativePath);
      }
      else {
        report.AppendLine("[ERROR] Template missing : " + fullPath);
        hasError = true;
      }
    }
  }

  private static void CheckAssets(
    StringBuilder report,
    ref bool hasError) {
    report.AppendLine();
    report.AppendLine("Assets");

    string assetsRoot =
        Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Assets"
        );

    string[] requiredAssets =
    [
        "pb-bzh-logo.png",
        "software-icon.png"
    ];

    foreach (string asset in requiredAssets) {
      string fullPath =
          Path.Combine(assetsRoot,asset);

      if (File.Exists(fullPath)) {
        report.AppendLine("[OK] Asset : " + asset);
      }
      else {
        report.AppendLine("[ERROR] Asset missing : " + fullPath);
        hasError = true;
      }
    }
  }

  private void CheckUploadCredential(
      StringBuilder report,
      ref bool hasWarning) {
    report.AppendLine();
    report.AppendLine("Upload credential");

    if (!_profile.Upload.UploadWebFilesAfterBuild) {
      report.AppendLine("[INFO] Upload is disabled.");
      return;
    }

    if (string.IsNullOrWhiteSpace(_profile.Upload.CredentialTarget)) {
      report.AppendLine("[WARN] Profile credential target is empty.");
      hasWarning = true;
      return;
    }

    report.AppendLine(
        "[OK] Profile credential target : " +
        _profile.Upload.CredentialTarget
    );

    string password =
        WindowsCredentialManager.ReadPassword(
            _profile.Upload.CredentialTarget
        );

    if (string.IsNullOrWhiteSpace(password)) {
      report.AppendLine(
          "[WARN] Profile upload password was not found in Windows Credential Manager."
      );

      hasWarning = true;
      return;
    }

    report.AppendLine(
        "[OK] Profile upload password found in Windows Credential Manager."
    );
  }

  private static X509Certificate2? FindCertificateByThumbprintInStore(
      string thumbprint,
      StoreLocation storeLocation,
      out string storeLocationText) {
    storeLocationText = storeLocation + @"\My";

    using X509Store store =
        new(StoreName.My,storeLocation);

    store.Open(OpenFlags.ReadOnly);

    foreach (X509Certificate2 certificate in store.Certificates) {
      string currentThumbprint =
          certificate.Thumbprint?
              .Replace(" ","")
              .Trim()
              .ToUpperInvariant()
          ?? "";

      if (currentThumbprint == thumbprint)
        return new X509Certificate2(certificate);
    }

    return null;
  }

  private static X509Certificate2? FindCertificateByThumbprint(
      string thumbprint,
      out string storeLocationText) {
    storeLocationText = "";

    if (string.IsNullOrWhiteSpace(thumbprint))
      return null;

    string normalizedThumbprint =
        thumbprint
            .Replace(" ","")
            .Trim()
            .ToUpperInvariant();

    X509Certificate2? certificate =
        FindCertificateByThumbprintInStore(
            normalizedThumbprint,
            StoreLocation.CurrentUser,
            out storeLocationText
        );

    if (certificate != null)
      return certificate;

    certificate =
        FindCertificateByThumbprintInStore(
            normalizedThumbprint,
            StoreLocation.LocalMachine,
            out storeLocationText
        );

    return certificate;
  }
  private void CheckSigningCertificate(
      StringBuilder report,
      ref bool hasWarning) {
    report.AppendLine();
    report.AppendLine("Code signing");

    if (!_profile.Signing.SignArtifacts) {
      report.AppendLine("[INFO] Code signing is disabled.");
    }

    AppendCertificateStatus(
        report,
        ref hasWarning,
        "Profile",
        _profile.Signing.CertificateThumbprint
    );

    if (string.IsNullOrWhiteSpace(_profile.Signing.TimestampUrl)) {
      report.AppendLine("[WARN] Timestamp URL is empty.");
      hasWarning = true;
    }
    else {
      report.AppendLine(
          "[OK] Timestamp URL : " +
          _profile.Signing.TimestampUrl
      );
    }
  }

  private void RunEnvironmentCheck() {
    try {
      string report =
          BuildEnvironmentCheckText();

      AppendLog(report);

      if (_previewEditors.TryGetValue("Environment Check",out RichTextBox? editor)) {
        SetPlainPreviewText(editor,report);
        ApplyPreviewSyntaxHighlighting(editor,"Environment Check");

        foreach (TabPage tab in tabPreview.TabPages) {
          if (tab.Text.Equals("Environment Check",StringComparison.OrdinalIgnoreCase)) {
            tabPreview.SelectedTab = tab;
            break;
          }
        }
      }

      MessageBox.Show(
          this,
          "Environment check completed.",
          "Environment check",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
      );
    }
    catch (Exception ex) {
      AppendLog("[ERROR] Environment check failed : " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Environment check",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private string GetBuildRootDirectory() {
    if (string.IsNullOrWhiteSpace(_profile.Output.MsiOutputDirectory))
      throw new InvalidOperationException("MSI output directory is empty.");

    string msiOutputDirectory =
        Path.GetFullPath(_profile.Output.MsiOutputDirectory);

    DirectoryInfo? buildRoot =
        Directory.GetParent(msiOutputDirectory);

    return buildRoot == null
      ? throw new InvalidOperationException("Unable to determine build root directory from MSI output directory : " + msiOutputDirectory)
      : buildRoot.FullName;
  }

  private void SaveReleaseReports() {
    ApplyUiToProfile();
    RefreshMsiDownloadUrl();
    SyncUpdateManifestFromProfile();
    ApplyUiToProfile();

    string releaseDirectory =
        Path.Combine(
            GetBuildRootDirectory(),
            "Release"
        );

    Directory.CreateDirectory(releaseDirectory);

    string checklistPath =
        Path.Combine(
            releaseDirectory,
            "RELEASE_CHECKLIST.txt"
        );

    string summaryPath =
        Path.Combine(
            releaseDirectory,
            "RELEASE_SUMMARY.txt"
        );

    File.WriteAllText(
        checklistPath,
        BuildReleaseChecklistText(),
        EncodingHelper.Utf8NoBom
    );

    File.WriteAllText(
        summaryPath,
        BuildReleaseSummaryText(),
        EncodingHelper.Utf8NoBom
    );

    AppendLog("[OK] Release checklist saved : " + checklistPath);
    AppendLog("[OK] Release summary saved : " + summaryPath);

    string webProductFolder =
        GetWebProductFolderName();

    string publishedUpdateJsonPath =
        Path.Combine(
            _profile.WebInstaller.WebPublishDirectory,
            WebCategoryFolderName,
            webProductFolder,
            "update.json"
        );

    string releaseUpdateJsonPath =
        Path.Combine(
            releaseDirectory,
            "update.json"
        );

    if (File.Exists(publishedUpdateJsonPath)) {
      File.Copy(
          publishedUpdateJsonPath,
          releaseUpdateJsonPath,
          overwrite: true
      );

      AppendLog("[OK] Release update manifest saved : " + releaseUpdateJsonPath);
    }
    else {
      AppendLog("[WARN] Published update.json not found : " + publishedUpdateJsonPath);
    }

    string publishedUpdateSettingsPath =
        Path.Combine(
            _profile.Publish.PublishDirectory,
            "UpdateSettings.json"
        );

    string releaseUpdateSettingsPath =
        Path.Combine(
            releaseDirectory,
            "UpdateSettings.json"
        );

    if (File.Exists(publishedUpdateSettingsPath)) {
      File.Copy(
          publishedUpdateSettingsPath,
          releaseUpdateSettingsPath,
          overwrite: true
      );

      AppendLog("[OK] Release update settings saved : " + releaseUpdateSettingsPath);
    }
    else {
      AppendLog("[WARN] UpdateSettings.json not found : " + publishedUpdateSettingsPath);
    }
  }

  private string BuildReleaseChecklistText() {
    ApplyUiToProfile();
    RefreshMsiDownloadUrl();
    ApplyUiToProfile();

    StringBuilder report = new();

    bool hasError = false;
    bool hasWarning = false;

    report.AppendLine("==================================================");
    report.AppendLine("Release Checklist");
    report.AppendLine("==================================================");
    report.AppendLine();

    report.AppendLine("Product : " + _profile.Product.ProductName);
    report.AppendLine("Version : " + _profile.Product.Version);
    report.AppendLine();

    string executablePath =
    Path.Combine(
        _profile.Publish.PublishDirectory,
        _profile.TargetProject.ExecutableName
    );

    AppendChecklistFile(
        report,
        ref hasError,
        "Published executable",
        executablePath,
        required: true
    );

    AppendChecklistItem(
        report,
        ref hasError,
        "Product name",
        !string.IsNullOrWhiteSpace(_profile.Product.ProductName),
        "Product name is empty."
    );

    AppendChecklistItem(
        report,
        ref hasError,
        "Product version",
        Version.TryParse(_profile.Product.Version,out _),
        "Product version is invalid."
    );

    AppendChecklistItem(
        report,
        ref hasError,
        "UpgradeCode",
        Guid.TryParse(_profile.Product.UpgradeCode,out _),
        "Product UpgradeCode is missing or invalid."
    );

    AppendChecklistFile(
        report,
        ref hasError,
        "Project file",
        _profile.TargetProject.ProjectFile,
        required: true
    );

    AppendChecklistFile(
        report,
        ref hasWarning,
        "Application icon",
        _profile.Product.IconPath,
        required: false
    );

    string msiPath =
        Path.Combine(
            _profile.Output.MsiOutputDirectory,
            _profile.Output.MsiFileName
        );

    AppendChecklistFile(
        report,
        ref hasError,
        "MSI package",
        msiPath,
        required: true
    );

    if (_profile.Bundle.BuildBundle) {
      string bundlePath =
          Path.Combine(
              _profile.Bundle.BundleOutputDirectory,
              _profile.Bundle.BundleFileName
          );

      AppendChecklistFile(
          report,
          ref hasError,
          "Setup.exe bundle",
          bundlePath,
          required: true
      );
    }
    else {
      report.AppendLine("[INFO] Setup.exe bundle is disabled.");
    }

    if (_profile.WebInstaller.BuildWebInstaller) {
      string webSetupPath =
          Path.Combine(
              _profile.WebInstaller.WebOutputDirectory,
              _profile.WebInstaller.WebSetupFileName
          );

      AppendChecklistFile(
          report,
          ref hasError,
          "Web setup.exe",
          webSetupPath,
          required: true
      );

      AppendChecklistItem(
          report,
          ref hasError,
          "MSI download URL",
          Uri.TryCreate(_profile.WebInstaller.MsiDownloadUrl,UriKind.Absolute,out Uri? msiUri) &&
          (msiUri.Scheme == Uri.UriSchemeHttp || msiUri.Scheme == Uri.UriSchemeHttps),
          "MSI download URL is invalid."
      );
    }
    else {
      report.AppendLine("[INFO] Web installer is disabled.");
    }

    if (_profile.Signing.SignArtifacts) {
      report.AppendLine();
      report.AppendLine("Code signing");

      AppendChecklistFile(
          report,
          ref hasError,
          "SignTool",
          _profile.Signing.SignToolPath,
          required: true
      );

      AppendChecklistItem(
          report,
          ref hasError,
          "Certificate thumbprint",
          !string.IsNullOrWhiteSpace(_profile.Signing.CertificateThumbprint),
          "Certificate thumbprint is empty."
      );

      AppendChecklistItem(
          report,
          ref hasError,
          "Timestamp URL",
          !string.IsNullOrWhiteSpace(_profile.Signing.TimestampUrl),
          "Timestamp URL is empty."
      );
    }
    else {
      report.AppendLine("[INFO] Code signing is disabled.");
    }

    if (_profile.WebInstaller.PrepareWebPublishFolder) {
      report.AppendLine();
      report.AppendLine("Web publish");

      string productFolder =
          GetWebProductFolderName();

      string projectPublishDir =
          Path.Combine(
              _profile.WebInstaller.WebPublishDirectory,
              WebCategoryFolderName,
              productFolder
          );

      string updateManifestPath =
          Path.Combine(
              projectPublishDir,
              "update.json"
          );

      AppendChecklistDirectory(
          report,
          ref hasError,
          "Web publish directory",
          _profile.WebInstaller.WebPublishDirectory,
          required: true
      );

      AppendChecklistDirectory(
          report,
          ref hasError,
          "Published product directory",
          projectPublishDir,
          required: true
      );

      AppendChecklistFile(
          report,
          ref hasError,
          "update.json",
          updateManifestPath,
          required: true
      );
    }

    if (_profile.Upload.UploadWebFilesAfterBuild) {
      report.AppendLine();
      report.AppendLine("Upload");

      AppendChecklistFile(
          report,
          ref hasError,
          "WinSCP",
          _profile.Upload.WinScpPath,
          required: true
      );

      AppendChecklistDirectory(
          report,
          ref hasError,
          "Upload local directory",
          _profile.Upload.LocalDirectory,
          required: true
      );

      AppendChecklistItem(
          report,
          ref hasError,
          "Upload credential",
          !string.IsNullOrWhiteSpace(GetUploadPassword()),
          "Upload password cannot be retrieved."
      );
    }
    else {
      report.AppendLine("[INFO] Upload after build is disabled.");
    }

    report.AppendLine();
    report.AppendLine("==================================================");
    report.AppendLine("Final status");
    report.AppendLine("==================================================");

    if (hasError)
      report.AppendLine("FAILED");
    else if (hasWarning)
      report.AppendLine("OK WITH WARNINGS");
    else
      report.AppendLine("OK");

    return report.ToString();
  }

  private static void AppendChecklistItem(
    StringBuilder report,
    ref bool issueFlag,
    string label,
    bool condition,
    string errorMessage) {
    if (condition) {
      report.AppendLine("[OK] " + label);
      return;
    }

    report.AppendLine("[ERROR] " + label + " : " + errorMessage);
    issueFlag = true;
  }

  private static void AppendChecklistFile(
    StringBuilder report,
    ref bool issueFlag,
    string label,
    string filePath,
    bool required) {
    if (string.IsNullOrWhiteSpace(filePath)) {
      if (required) {
        report.AppendLine("[ERROR] " + label + " path is empty.");
        issueFlag = true;
      }
      else {
        report.AppendLine("[WARN] " + label + " path is empty.");
        issueFlag = true;
      }

      return;
    }

    string fullPath;

    try {
      fullPath = Path.GetFullPath(filePath);
    }
    catch {
      report.AppendLine("[ERROR] " + label + " path is invalid : " + filePath);
      issueFlag = true;
      return;
    }

    if (File.Exists(fullPath)) {
      report.AppendLine("[OK] " + label + " : " + fullPath);
      return;
    }

    if (required)
      report.AppendLine("[ERROR] " + label + " not found : " + fullPath);
    else
      report.AppendLine("[WARN] " + label + " not found : " + fullPath);

    issueFlag = true;
  }

  private static void AppendChecklistDirectory(
    StringBuilder report,
    ref bool issueFlag,
    string label,
    string directoryPath,
    bool required) {
    if (string.IsNullOrWhiteSpace(directoryPath)) {
      if (required) {
        report.AppendLine("[ERROR] " + label + " path is empty.");
        issueFlag = true;
      }
      else {
        report.AppendLine("[WARN] " + label + " path is empty.");
        issueFlag = true;
      }

      return;
    }

    string fullPath;

    try {
      fullPath = Path.GetFullPath(directoryPath);
    }
    catch {
      report.AppendLine("[ERROR] " + label + " path is invalid : " + directoryPath);
      issueFlag = true;
      return;
    }

    if (Directory.Exists(fullPath)) {
      report.AppendLine("[OK] " + label + " : " + fullPath);
      return;
    }

    if (required)
      report.AppendLine("[ERROR] " + label + " not found : " + fullPath);
    else
      report.AppendLine("[WARN] " + label + " not found : " + fullPath);

    issueFlag = true;
  }

  private void RunReleaseChecklist() {
    try {
      string report =
          BuildReleaseChecklistText();

      AppendLog(report);

      if (_previewEditors.TryGetValue("Release Checklist",out RichTextBox? editor)) {
        SetPlainPreviewText(editor,report);
        ApplyPreviewSyntaxHighlighting(editor,"Release Checklist");

        foreach (TabPage tab in tabPreview.TabPages) {
          if (tab.Text.Equals("Release Checklist",StringComparison.OrdinalIgnoreCase)) {
            tabPreview.SelectedTab = tab;
            break;
          }
        }
      }

      MessageBox.Show(
          this,
          "Release checklist completed.",
          "Release checklist",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
      );
    }
    catch (Exception ex) {
      AppendLog("[ERROR] Release checklist failed : " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Release checklist",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void mnuCodeSigningSettings_Click(object? sender,EventArgs e) {
    ApplyUiToProfile();

    using CodeSigningSettingsForm dialog = new(_profile.Signing);

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;
    _profile.Signing = dialog.Signing;
    _profileHasUnsavedChanges = true;
    AutoSaveProfile();
    RefreshPreviewTabs();
    AppendLog("[INFO] Code signing settings updated.");
  }

  private void mnuEnvironmentCheck_Click(object? sender,EventArgs e) {
    RunEnvironmentCheck();
  }

  private void mnuReleaseChecklist_Click(object? sender,EventArgs e) {
    RunReleaseChecklist();
  }

  private void mnuGlobalSettings_Click(object? sender,EventArgs e) {
    using GlobalSettingsForm dialog =
        new(_globalSettings);

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    _globalSettings =
        dialog.Settings;

    GlobalSettingsService.Save(_globalSettings);

    AppendLog(
        "[INFO] Global settings saved : " +
        GlobalSettingsService.GetSettingsPath()
    );

    DialogResult result = MessageBox.Show(
        this,
        "Global settings saved." +
        Environment.NewLine +
        Environment.NewLine +
        "Do you want to apply them to the current profile?",
        "Global settings",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question
    );

    if (result != DialogResult.Yes)
      return;

    ApplyGlobalSettingsToProfileDefaults();

    ApplyProfileToUi();

    UpdateBundleUiState();
    UpdateWebInstallerUiState();
    UpdateWebUploadUiState();
    UpdateToolTips();

    RefreshMsiDownloadUrl();
    ApplyUiToProfile();

    RefreshPreviewTabs();

    _profileHasUnsavedChanges = true;

    AutoSaveProfile();

    AppendLog("[INFO] Global settings applied to current profile.");
  }

  private void mnuOpenGlobalSettingsFolder_Click(object? sender,EventArgs e) {
    try {
      string settingsDirectory =
          GlobalSettingsService.GetSettingsDirectory();

      if (!Directory.Exists(settingsDirectory)) {
        MessageBox.Show(
            this,
            "Global settings folder was not found." +
            Environment.NewLine +
            Environment.NewLine +
            settingsDirectory,
            "Open global settings folder",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );

        return;
      }

      Process.Start(new ProcessStartInfo {
        FileName = "explorer.exe",
        Arguments = $"\"{settingsDirectory}\"",
        UseShellExecute = true
      });

      AppendLog("[INFO] Global settings folder opened : " + settingsDirectory);
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Open global settings folder",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void mnuOpenGlobalSettingsFile_Click(object? sender,EventArgs e) {
    try {
      string settingsPath =
          GlobalSettingsService.GetSettingsPath();

      if (!File.Exists(settingsPath)) {
        MessageBox.Show(
            this,
            "Global settings file was not found." +
            Environment.NewLine +
            Environment.NewLine +
            settingsPath,
            "Open global settings file",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning
        );

        return;
      }

      Process.Start(new ProcessStartInfo {
        FileName = settingsPath,
        UseShellExecute = true
      });

      AppendLog("[INFO] Global settings file opened : " + settingsPath);
    }
    catch (Exception ex) {
      AppendLog("[ERROR] " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Open global settings file",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void mnuReleaseSummary_Click(object? sender,EventArgs e) {
    ShowReleaseSummary();
  }

  private void mnuLicense_Click(object? sender,EventArgs e) {
    LicenseHelper.AfficherLicence(this,_licenseService,_profile);
  }

  private void mnuImportLicense_Click(object? sender,EventArgs e) {
    LicenseHelper.ImporterLicence(this,_licenseService);
  }

  private void btnAndroidPackageSettings_Click(object sender,EventArgs e) {
    using AndroidPackageSettingsForm form = new(_profile.Android);

    if (form.ShowDialog(this) != DialogResult.OK)
      return;

    _profile.Android = form.AndroidOptions;

    AutoSaveProfile();
    RefreshPreviewTabs();

    AppendLog("[INFO] Android APK settings updated.");
  }

  private async void mnuBuildApk_Click(object? sender,EventArgs e) {
    ApplyUiToProfile();
    AutoSaveProfile();

    if (!_profile.Android.PublishApk) {
      MessageBox.Show(
          this,
          "Android APK publishing is disabled in APK settings.",
          "Build APK",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
      );
      return;
    }

    try {
      mnuBuildApk.Enabled = false;
      btnBuildMsi.Enabled = false;
      btnGenerateWix.Enabled = false;
      btnBuildApk.Enabled = false;

      AndroidBuildService service = new();

      string apkPath = await Task.Run(() =>
          service.BuildApk(_profile,AppendLog)
      );

      AutoSaveProfile();
      RefreshPreviewTabs();

      MessageBox.Show(
          this,
          "Android APK generated successfully." +
          Environment.NewLine +
          Environment.NewLine +
          apkPath,
          "Build APK",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
      );
    }
    catch (Exception ex) {
      AppendLog("[ERROR] Android APK build failed : " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Build APK",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
    finally {
      mnuBuildApk.Enabled = true;
      btnBuildMsi.Enabled = true;
      btnGenerateWix.Enabled = true;
      btnBuildApk.Enabled = true;
    }
  }

  private async void btnBuildApk_Click(object? sender,EventArgs e) {
    ApplyUiToProfile();

    using AndroidPackageSettingsForm form = new(_profile.Android);

    if (form.ShowDialog(this) != DialogResult.OK)
      return;

    _profile.Android = form.AndroidOptions;

    if (!_profile.Android.PublishApk) {
      MessageBox.Show(
          this,
          "Publish Android APK must be checked.",
          "Build APK",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
      );
      return;
    }

    AutoSaveProfile();

    try {
      btnBuildApk.Enabled = false;
      btnBuildMsi.Enabled = false;
      btnGenerateWix.Enabled = false;

      AndroidBuildService service = new();

      string apkPath = await Task.Run(() =>
          service.BuildApk(_profile,AppendLog)
      );

      AppendLog("[OK] Android APK generated : " + apkPath);

      if (_profile.Upload.UploadWebFilesAfterBuild) {
        PrepareWebPublishFolder();
        RunWinScpUpload(forceUpload: true);
      }

      AutoSaveProfile();
      RefreshPreviewTabs();

      string uploadMessage =
        !_profile.Upload.UploadWebFilesAfterBuild
        ? "Web upload was not requested."
        : _lastUploadSuccess
            ? "Web upload completed successfully."
            : "Web upload was requested but did not complete successfully.";

      MessageBox.Show(
        this,
        "Android APK build completed successfully." +
        Environment.NewLine +
        Environment.NewLine +
        "APK file:" +
        Environment.NewLine +
        apkPath +
        Environment.NewLine +
        Environment.NewLine +
        uploadMessage,
        "Build APK",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information
      );

    }
    catch (Exception ex) {
      AppendLog("[ERROR] Android APK build failed : " + ex.Message);

      MessageBox.Show(
          this,
          ex.Message,
          "Build APK",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
    finally {
      btnBuildApk.Enabled = true;
      btnBuildMsi.Enabled = true;
      btnGenerateWix.Enabled = true;
    }
  }
}
