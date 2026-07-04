using MsiSoftwarePackager.Core.Models;
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
  }

  private void ApplyOptionsApkToUi() {
    chkPublishAndroidApk.Checked = AndroidOptions.PublishApk;
    txtApkFile.Text = AndroidOptions.ApkFilePath;
    cmbConfiguration.Text = AndroidOptions.Configuration;
    cmbTargetFramework.Text = AndroidOptions.TargetFramework;
    txtKeystoreFile.Text = AndroidOptions.KeystoreFile;
    txtAlias.Text = AndroidOptions.KeyAlias;
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
