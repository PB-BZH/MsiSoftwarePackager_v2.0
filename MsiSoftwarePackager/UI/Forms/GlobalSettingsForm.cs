using MsiSoftwarePackager.Core.Models;
using MsiSoftwarePackager.Core.Services;
using PB.BZH.Help.Library.UI.Theming;

namespace MsiSoftwarePackager.UI.Forms;

public partial class GlobalSettingsForm: Form {
  public GlobalSettings Settings { get; private set; }

  public GlobalSettingsForm(GlobalSettings settings) {
    InitializeComponent();
    Settings = new GlobalSettings {
      Upload = new GlobalUploadSettings {
        Protocol = settings.Upload.Protocol,
        Host = settings.Upload.Host,
        UserName = settings.Upload.UserName,
        RemoteDirectory = settings.Upload.RemoteDirectory,
        LocalDirectory = settings.Upload.LocalDirectory,
        WebRemoteSite = settings.Upload.WebRemoteSite
      },
      Tools = new GlobalToolsSettings {
        WinScpPath = settings.Tools.WinScpPath,
        NotepadPlusPlusPath = settings.Tools.NotepadPlusPlusPath
      },
      Signing = new GlobalSigningSettings {
        SignToolPath = settings.Signing.SignToolPath,
        CertificateThumbprint = settings.Signing.CertificateThumbprint,
        TimestampUrl = settings.Signing.TimestampUrl
      }
    };

    ThemeManager.ApplyDarkTheme(this);
    ThemeManager.StyleDarkButtons(this);
    ThemeManager.ApplyDarkDialogBorder(this,pnlMain);

    ApplySettingsToUi();
  }

  private void SaveUploadPasswordIfProvided() {
    string password =
        txtUploadPassword.Text;

    if (string.IsNullOrWhiteSpace(password))
      return;

    string credentialTarget =
        WindowsCredentialManager.BuildTargetName(
            Settings.Upload.Protocol,
            Settings.Upload.Host,
            Settings.Upload.UserName
        );

    WindowsCredentialManager.SavePassword(
        credentialTarget,
        Settings.Upload.UserName,
        password
    );

    Settings.Upload.CredentialTarget =
        credentialTarget;

    txtUploadPassword.Clear();
  }

  private void ApplySettingsToUi() {
    cmbProtocol.Text = Settings.Upload.Protocol;
    txtHost.Text = Settings.Upload.Host;
    txtUserName.Text = Settings.Upload.UserName;
    txtRemoteDirectory.Text = Settings.Upload.RemoteDirectory;
    txtLocalWebSite.Text = Settings.Upload.LocalDirectory;
    txtWebRemoteSite.Text = Settings.Upload.WebRemoteSite;

    txtWinScpPath.Text = Settings.Tools.WinScpPath;
    txtNotepadppPath.Text = Settings.Tools.NotepadPlusPlusPath;

    txtSignToolPath.Text = Settings.Signing.SignToolPath;
    txtCertificateThumbprint.Text = Settings.Signing.CertificateThumbprint;
    txtTimestampUrl.Text = Settings.Signing.TimestampUrl;
    txtBuildRoot.Text = Settings.BuildPaths.BuildRoot;
  }

  private void ApplyUiToSettings() {
    Settings.Upload.Protocol = cmbProtocol.Text;
    Settings.Upload.Host = txtHost.Text;
    Settings.Upload.UserName = txtUserName.Text;
    Settings.Upload.RemoteDirectory = txtRemoteDirectory.Text;
    Settings.Upload.LocalDirectory = txtLocalWebSite.Text;
    Settings.Upload.WebRemoteSite = txtWebRemoteSite.Text;

    Settings.Tools.WinScpPath = txtWinScpPath.Text;
    Settings.Tools.NotepadPlusPlusPath = txtNotepadppPath.Text;
    Settings.Signing.SignToolPath = ToolPathResolver.ResolveSignToolPath(txtSignToolPath.Text);
    Settings.Signing.CertificateThumbprint = txtCertificateThumbprint.Text;
    Settings.Signing.TimestampUrl = txtTimestampUrl.Text;
    Settings.BuildPaths.BuildRoot = txtBuildRoot.Text;
    Settings.Upload.CredentialTarget =
    WindowsCredentialManager.BuildTargetName(
        Settings.Upload.Protocol,
        Settings.Upload.Host,
        Settings.Upload.UserName
    );
  }

  private void btnOk_Click(object? sender,EventArgs e) {
    ApplyUiToSettings();
    SaveUploadPasswordIfProvided();

    DialogResult = DialogResult.OK;
    Close();
  }

  private void btnCancel_Click(object? sender,EventArgs e) {
    DialogResult = DialogResult.Cancel;
    Close();
  }

  private void btnBrowseUploadLocalDirectory_Click(object? sender,EventArgs e) {
    using FolderBrowserDialog dialog = new();

    dialog.Description = "Select local web site directory";

    if (!string.IsNullOrWhiteSpace(txtLocalWebSite.Text) &&
        Directory.Exists(txtLocalWebSite.Text)) {
      dialog.SelectedPath = txtLocalWebSite.Text;
    }

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    txtLocalWebSite.Text = dialog.SelectedPath;
  }

  private void btnBrowseWinScpPath_Click(object? sender,EventArgs e) {
    BrowseExecutable(
        txtWinScpPath,
        "Select WinSCP.com",
        "WinSCP|WinSCP.com|Executable files|*.exe;*.com|All files|*.*"
    );
  }

  private void btnBrowseNotepadPlusPlusPath_Click(object? sender,EventArgs e) {
    BrowseExecutable(
        txtNotepadppPath,
        "Select Notepad++",
        "Notepad++|notepad++.exe|Executable files|*.exe|All files|*.*"
    );
  }

  private void btnBrowseSignToolPath_Click(object? sender,EventArgs e) {
    BrowseExecutable(
        txtSignToolPath,
        "Select SignTool.exe",
        "SignTool|signtool.exe|Executable files|*.exe|All files|*.*"
    );
  }

  private void BrowseExecutable(
      TextBox targetTextBox,
      string title,
      string filter) {
    using OpenFileDialog dialog = new();

    dialog.Title = title;
    dialog.Filter = filter;

    if (!string.IsNullOrWhiteSpace(targetTextBox.Text)) {
      string? directory = Path.GetDirectoryName(targetTextBox.Text);

      if (!string.IsNullOrWhiteSpace(directory) &&
          Directory.Exists(directory)) {
        dialog.InitialDirectory = directory;
      }
    }

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    targetTextBox.Text = dialog.FileName;
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
  private void btnBrowseBuildRoot_Click(object? sender,EventArgs e) {
    using FolderBrowserDialog dialog = new();

    dialog.Description = "Select build root directory";

    if (!string.IsNullOrWhiteSpace(txtBuildRoot.Text) &&
        Directory.Exists(txtBuildRoot.Text)) {
      dialog.SelectedPath = txtBuildRoot.Text;
    }

    if (dialog.ShowDialog(this) != DialogResult.OK)
      return;

    txtBuildRoot.Text = dialog.SelectedPath;
  }
}