using MsiSoftwarePackager.Core.Models;
using MsiSoftwarePackager.Core.Services;
using PB.BZH.Help.Library.UI.Theming;

namespace MsiSoftwarePackager.UI.Forms;

public partial class AndroidPackageSettingsForm: Form {
  public AndroidPackageOptions AndroidOptions { get; private set; }

  public AndroidPackageSettingsForm(AndroidPackageOptions androidOptions) {
    InitializeComponent();
    AndroidOptions = androidOptions;
    ThemeManager.ApplyDarkDialogBorder(this,pnlMain);
    ThemeManager.ApplyDarkTitleBar(this);
    ThemeManager.StyleDarkButtons(this);
    ThemeManager.ApplyDarkTheme(this);
    ApplyOptionsApkToUi();
  }

  private void ApplyUitoApkOptions() {
    AndroidOptions.PublishApk = chkPublishAndroidApk.Checked;
    AndroidOptions.ApkFilePath = txtApkFile.Text;
    AndroidOptions.Configuration = cmbConfiguration.Text;
    AndroidOptions.TargetFramework = cmbTargetFramework.Text;
    AndroidOptions.KeystoreFile = txtKeystoreFile.Text;
    AndroidOptions.KeyAlias = txtAlias.Text;

    // --------------------------------------------------
    // keystore credentials stored in Windows Credential Manager
    // --------------------------------------------------
    AndroidOptions.KeystorePasswordCredentialTarget = WindowsCredentialManager.BuildKeystoreTargetName(AndroidOptions.KeyAlias);
    if (!string.IsNullOrEmpty(txtKeystorePassword.Text)) {
      WindowsCredentialManager.SavePassword(AndroidOptions.KeystorePasswordCredentialTarget,AndroidOptions.KeyAlias,txtKeystorePassword.Text);
    }

    // --------------------------------------------------
    // key credentials stored in Windows Credential Manager
    // --------------------------------------------------
    AndroidOptions.KeyPasswordCredentialTarget = WindowsCredentialManager.BuildKeyTargetName(AndroidOptions.KeyAlias);
    if (!string.IsNullOrEmpty(txtKeyPassword.Text)) {
      WindowsCredentialManager.SavePassword(AndroidOptions.KeyPasswordCredentialTarget,AndroidOptions.KeyAlias,txtKeyPassword.Text);
    }
  }


  private void ApplyOptionsApkToUi() {
    chkPublishAndroidApk.Checked = AndroidOptions.PublishApk;
    txtApkFile.Text = AndroidOptions.ApkFilePath;
    cmbConfiguration.Text = AndroidOptions.Configuration;
    cmbTargetFramework.Text = AndroidOptions.TargetFramework;
    txtKeystoreFile.Text = AndroidOptions.KeystoreFile;
    txtAlias.Text = AndroidOptions.KeyAlias;
    // --------------------------------------------------
    // Keystore password from Windows Credential Manager
    // --------------------------------------------------
    string keystorePasswordTarget = AndroidOptions.KeystorePasswordCredentialTarget ?? string.Empty;
    if (string.IsNullOrWhiteSpace(keystorePasswordTarget) && !string.IsNullOrWhiteSpace(AndroidOptions.KeystorePasswordCredentialTarget)) {
      keystorePasswordTarget = WindowsCredentialManager.BuildKeystoreTargetName(AndroidOptions.KeystorePasswordCredentialTarget);
    }
    txtKeystorePassword.Text = WindowsCredentialManager.ReadPassword(keystorePasswordTarget);

    // --------------------------------------------------
    // Key password from Windows Credential Manager
    // --------------------------------------------------
    string keyPasswordTarget =
        AndroidOptions.KeyPasswordCredentialTarget ?? string.Empty;
    if (string.IsNullOrWhiteSpace(keyPasswordTarget) && !string.IsNullOrWhiteSpace(AndroidOptions.KeyPasswordCredentialTarget)) {
      keyPasswordTarget = WindowsCredentialManager.BuildKeyTargetName(AndroidOptions.KeyPasswordCredentialTarget);
    }
    txtKeyPassword.Text = WindowsCredentialManager.ReadPassword(keyPasswordTarget);
  }

  private void btnOk_Click(object sender,EventArgs e) {
    ApplyUitoApkOptions();
    DialogResult = DialogResult.OK;
    Close();
  }

  private void btnCancel_Click(object sender,EventArgs e) {
    this.Close();
  }

  private void btnBrowseApkFile_Click(object sender,EventArgs e) {
    using OpenFileDialog dialog = new();

    dialog.Title = "Select Android APK file";
    dialog.Filter = "Android APK (*.apk)|*.apk|All files (*.*)|*.*";
    dialog.CheckFileExists = true;
    dialog.CheckPathExists = true;

    if (!string.IsNullOrWhiteSpace(txtApkFile.Text)) {
      string? directory = Path.GetDirectoryName(txtApkFile.Text);

      if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        dialog.InitialDirectory = directory;
    }

    if (dialog.ShowDialog(this) == DialogResult.OK)
      txtApkFile.Text = dialog.FileName;
  }
  private void btnBrowseKeystoreFile_Click(object sender,EventArgs e) {
    using OpenFileDialog dialog = new();

    dialog.Title = "Select Android keystore file";
    dialog.Filter =
        "Android keystore (*.keystore;*.jks)|*.keystore;*.jks|All files (*.*)|*.*";
    dialog.CheckFileExists = true;
    dialog.CheckPathExists = true;

    if (!string.IsNullOrWhiteSpace(txtKeystoreFile.Text)) {
      string? directory = Path.GetDirectoryName(txtKeystoreFile.Text);

      if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        dialog.InitialDirectory = directory;
    }

    if (dialog.ShowDialog(this) == DialogResult.OK)
      txtKeystoreFile.Text = dialog.FileName;
  }
}
