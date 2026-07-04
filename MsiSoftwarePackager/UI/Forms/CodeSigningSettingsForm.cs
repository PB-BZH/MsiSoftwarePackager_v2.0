using MsiSoftwarePackager.Core.Models;
using MsiSoftwarePackager.Core.Services;
using PB.BZH.Help.Library.UI.Theming;

namespace MsiSoftwarePackager.UI.Forms;

public partial class CodeSigningSettingsForm: Form {
  private bool _isLoadingSigningUi;
  public SigningOptions Signing { get; private set; }

  private static string FindLatestSignTool() {
    string baseDir = @"C:\Program Files (x86)\Windows Kits\10\bin";
    if (!Directory.Exists(baseDir))
      return string.Empty;

    string[] candidates = [.. Directory
            .GetFiles(baseDir,"signtool.exe",SearchOption.AllDirectories)
            .Where(path =>
                path.Contains(@"\x64\",StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(path => path)
            ];

    return candidates.Length > 0
        ? candidates[0]
        : string.Empty;
  }

  public CodeSigningSettingsForm(SigningOptions signing) {
    InitializeComponent();

    if (signing == null)
      throw new ArgumentNullException(nameof(signing));

    chkSignArtifactsAfterBuild.CheckedChanged += (_,_) => UpdateSigningUiState();

    Signing = new SigningOptions {
      SignArtifacts = signing.SignArtifacts,
      SignToolPath = signing.SignToolPath,
      CertificateThumbprint = signing.CertificateThumbprint,
      TimestampUrl = signing.TimestampUrl,

      SignMsi = signing.SignMsi,
      SignBundle = signing.SignBundle,
      SignWebSetup = signing.SignWebSetup,
      SignExecutable = signing.SignExecutable,

      SigningProvider = signing.SigningProvider,
      SslComCodeSignToolPath = signing.SslComCodeSignToolPath,
      SslComCredentialId = signing.SslComCredentialId,
      SslComLogin = signing.SslComLogin,
      SslComCredentialTarget = signing.SslComCredentialTarget,
      SslComTotpSecretCredentialTarget = signing.SslComTotpSecretCredentialTarget
    };

    ThemeManager.ApplyDarkTheme(this);
    ThemeManager.StyleDarkButtons(this);
    ThemeManager.ApplyDarkDialogBorder(this,pnlMain);

    ApplySigningToUi();
    UpdateSigningUiState();
  }

  private void UpdateSigningUiState() {
    bool enabled = chkSignArtifactsAfterBuild.Checked;
    txtSslCodeSignTool.Enabled = enabled;
    txtCredentialId.Enabled = enabled;
    cmbSigningProvider.Enabled = enabled;
    txtLoginSsl.Enabled = enabled;
    txtPasswordSsl.Enabled = enabled;
    txtSignToolPath.Enabled = enabled;
    btnFindSignTool.Enabled = enabled;
    btnGenerateSelfSignedCertificate.Enabled = enabled;
    txtCertificateThumbprint.Enabled = enabled;
    txtTimestampUrl.Enabled = enabled;
    chkSignExecutable.Enabled = enabled;
    chkSignMsi.Enabled = enabled;
    chkSignBundle.Enabled = enabled;
    chkSignWebSetup.Enabled = enabled;
  }

  private void ApplySigningToUi() {
    _isLoadingSigningUi = true;

    try {
      // --------------------------------------------------
      // Global signing options
      // --------------------------------------------------
      chkSignArtifactsAfterBuild.Checked = Signing.SignArtifacts;
      cmbSigningProvider.SelectedItem = string.IsNullOrWhiteSpace(Signing.SigningProvider)
              ? "SignToolCka"
              : Signing.SigningProvider;

      // --------------------------------------------------
      // Microsoft SignTool / CKA options
      // --------------------------------------------------
      txtSignToolPath.Text = Signing.SignToolPath ?? string.Empty;
      txtCertificateThumbprint.Text = Signing.CertificateThumbprint ?? string.Empty;
      txtTimestampUrl.Text = Signing.TimestampUrl ?? string.Empty;

      // --------------------------------------------------
      // SSL.com CodeSignTool options
      // --------------------------------------------------
      txtCodeSignToolPath.Text = Signing.SslComCodeSignToolPath ?? string.Empty;
      txtCredentialId.Text = Signing.SslComCredentialId ?? string.Empty;
      txtLoginSsl.Text = Signing.SslComLogin ?? string.Empty;

      // --------------------------------------------------
      // Artifact signing options
      // --------------------------------------------------
      chkSignExecutable.Checked = Signing.SignExecutable;
      chkSignMsi.Checked = Signing.SignMsi;
      chkSignBundle.Checked = Signing.SignBundle;
      chkSignWebSetup.Checked = Signing.SignWebSetup;

      // --------------------------------------------------
      // SSL.com password from Windows Credential Manager
      // --------------------------------------------------
      string passwordTarget =
          Signing.SslComCredentialTarget ?? string.Empty;
      if (string.IsNullOrWhiteSpace(passwordTarget) && !string.IsNullOrWhiteSpace(Signing.SslComLogin)) {
        passwordTarget = WindowsCredentialManager.BuildSslComSigningTargetName(Signing.SslComLogin);
      }
      txtPasswordSsl.Text = WindowsCredentialManager.ReadPassword(passwordTarget);

      // --------------------------------------------------
      // SSL.com TOTP secret from Windows Credential Manager
      // --------------------------------------------------
      string totpTarget = Signing.SslComTotpSecretCredentialTarget ?? string.Empty;
      if (string.IsNullOrWhiteSpace(totpTarget) && !string.IsNullOrWhiteSpace(Signing.SslComLogin)) {
        totpTarget = WindowsCredentialManager.BuildSslComTotpSecretTargetName(Signing.SslComLogin);
      }
      txtSslCodeSignTool.Text = WindowsCredentialManager.ReadPassword(totpTarget);
    }
    finally {
      _isLoadingSigningUi = false;
    }
  }

  private void ApplyUiToSigning() {
    if (_isLoadingSigningUi)
      return;

    // --------------------------------------------------
    // Global signing options
    // --------------------------------------------------
    Signing.SignArtifacts = chkSignArtifactsAfterBuild.Checked;
    Signing.SigningProvider = cmbSigningProvider.SelectedItem?.ToString() ?? "SignToolCka";

    // --------------------------------------------------
    // Microsoft SignTool / CKA options
    // --------------------------------------------------
    Signing.SignToolPath = ToolPathResolver.ResolveSignToolPath(txtSignToolPath.Text);
    Signing.CertificateThumbprint = txtCertificateThumbprint.Text.Trim();
    Signing.TimestampUrl = txtTimestampUrl.Text.Trim();

    // --------------------------------------------------
    // SSL.com CodeSignTool options
    // --------------------------------------------------
    Signing.SslComCodeSignToolPath = txtCodeSignToolPath.Text.Trim();
    Signing.SslComCredentialId = txtCredentialId.Text.Trim();
    Signing.SslComLogin = txtLoginSsl.Text.Trim();

    // --------------------------------------------------
    // Artifact signing options
    // --------------------------------------------------
    Signing.SignExecutable = chkSignExecutable.Checked;
    Signing.SignMsi = chkSignMsi.Checked;
    Signing.SignBundle = chkSignBundle.Checked;
    Signing.SignWebSetup = chkSignWebSetup.Checked;

    // --------------------------------------------------
    // SSL.com credentials stored in Windows Credential Manager
    // --------------------------------------------------
    if (!string.IsNullOrWhiteSpace(Signing.SslComLogin)) {
      Signing.SslComCredentialTarget = WindowsCredentialManager.BuildSslComSigningTargetName(Signing.SslComLogin);
      Signing.SslComTotpSecretCredentialTarget = WindowsCredentialManager.BuildSslComTotpSecretTargetName(Signing.SslComLogin);
      if (!string.IsNullOrEmpty(txtPasswordSsl.Text)) {
        WindowsCredentialManager.SavePassword(Signing.SslComCredentialTarget,Signing.SslComLogin,txtPasswordSsl.Text);
      }
      if (!string.IsNullOrEmpty(txtSslCodeSignTool.Text)) {
        WindowsCredentialManager.SavePassword(Signing.SslComTotpSecretCredentialTarget,Signing.SslComLogin,txtSslCodeSignTool.Text);
      }
    }
    else {
      Signing.SslComCredentialTarget = string.Empty;
      Signing.SslComTotpSecretCredentialTarget = string.Empty;
    }
  }

  private void btnOk_Click(object? sender,EventArgs e) {
    ApplyUiToSigning();

    DialogResult = DialogResult.OK;
    Close();
  }

  private void btnCancel_Click(object? sender,EventArgs e) {
    DialogResult = DialogResult.Cancel;
    Close();
  }

  private void btnGenerateSelfSignedCertificate_Click(object? sender,EventArgs e) {
    try {
      string publisherName = "PB BZH Concept";
      txtCertificateThumbprint.Text = CodeSigningCertificateHelper.GenerateSelfSignedCodeSigningCertificate(publisherName);
      MessageBox.Show(
          this,
          "Self-signed code signing certificate generated." +
          Environment.NewLine +
          Environment.NewLine +
          "Thumbprint :" +
          Environment.NewLine +
          txtCertificateThumbprint.Text,
          "Code signing certificate",
          MessageBoxButtons.OK,
          MessageBoxIcon.Information
      );
    }
    catch (Exception ex) {
      MessageBox.Show(
          this,
          ex.Message,
          "Code signing certificate",
          MessageBoxButtons.OK,
          MessageBoxIcon.Error
      );
    }
  }

  private void btnFindSignTool_Click(object? sender,EventArgs e) {
    string signToolPath = FindLatestSignTool();

    if (string.IsNullOrWhiteSpace(signToolPath)) {
      MessageBox.Show(
          this,
          "SignTool.exe was not found.",
          "Find SignTool",
          MessageBoxButtons.OK,
          MessageBoxIcon.Warning
      );
      return;
    }

    txtSignToolPath.Text = signToolPath;
  }

  private void btnBrowseCodeSignTool_Click(object? sender,EventArgs e) {
    using OpenFileDialog dialog = new() {
      Title = "Select SSL.com CodeSignTool",
      Filter =
          "SSL.com CodeSignTool|CodeSignTool.bat;CodeSignTool.exe|" +
          "Batch files (*.bat)|*.bat|" +
          "Executable files (*.exe)|*.exe|" +
          "All files (*.*)|*.*",
      CheckFileExists = true,
      CheckPathExists = true,
      Multiselect = false
    };

    if (!string.IsNullOrWhiteSpace(txtCodeSignToolPath.Text)) {
      string currentDirectory = Path.GetDirectoryName(txtCodeSignToolPath.Text) ?? string.Empty;

      if (Directory.Exists(currentDirectory))
        dialog.InitialDirectory = currentDirectory;
    }

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    txtCodeSignToolPath.Text = dialog.FileName;
  }
}