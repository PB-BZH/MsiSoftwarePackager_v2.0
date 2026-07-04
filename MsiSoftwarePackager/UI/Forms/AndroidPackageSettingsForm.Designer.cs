namespace MsiSoftwarePackager.UI.Forms;

partial class AndroidPackageSettingsForm {
  /// <summary>
  /// Required designer variable.
  /// </summary>
  private System.ComponentModel.IContainer components = null;

  /// <summary>
  /// Clean up any resources being used.
  /// </summary>
  /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
  protected override void Dispose(bool disposing) {
    if (disposing && (components != null)) {
      components.Dispose();
    }
    base.Dispose(disposing);
  }

  #region Windows Form Designer generated code

  /// <summary>
  /// Required method for Designer support - do not modify
  /// the contents of this method with the code editor.
  /// </summary>
  private void InitializeComponent() {
    pnlMain = new Panel();
    grpKeystoreOptions = new GroupBox();
    label3 = new Label();
    label2 = new Label();
    label1 = new Label();
    lblKeystoreFile = new Label();
    btnBrowseKeystoreFile = new Button();
    txtKeyPassword = new TextBox();
    txtKeystorePassword = new TextBox();
    txtAlias = new TextBox();
    txtKeystoreFile = new TextBox();
    grpApk = new GroupBox();
    cmbTargetFramework = new ComboBox();
    cmbConfiguration = new ComboBox();
    btnBrowseApkFile = new Button();
    txtApkFile = new TextBox();
    lblTargetFramework = new Label();
    lblConfiguration = new Label();
    LblApkPath = new Label();
    chkPublishAndroidApk = new CheckBox();
    btnOk = new Button();
    btnCancel = new Button();
    pnlMain.SuspendLayout();
    grpKeystoreOptions.SuspendLayout();
    grpApk.SuspendLayout();
    SuspendLayout();
    // 
    // pnlMain
    // 
    pnlMain.Controls.Add(grpKeystoreOptions);
    pnlMain.Controls.Add(grpApk);
    pnlMain.Controls.Add(btnOk);
    pnlMain.Controls.Add(btnCancel);
    pnlMain.Dock = DockStyle.Fill;
    pnlMain.Location = new Point(2,2);
    pnlMain.Margin = new Padding(0);
    pnlMain.Name = "pnlMain";
    pnlMain.Size = new Size(771,197);
    pnlMain.TabIndex = 0;
    // 
    // grpKeystoreOptions
    // 
    grpKeystoreOptions.Controls.Add(label3);
    grpKeystoreOptions.Controls.Add(label2);
    grpKeystoreOptions.Controls.Add(label1);
    grpKeystoreOptions.Controls.Add(lblKeystoreFile);
    grpKeystoreOptions.Controls.Add(btnBrowseKeystoreFile);
    grpKeystoreOptions.Controls.Add(txtKeyPassword);
    grpKeystoreOptions.Controls.Add(txtKeystorePassword);
    grpKeystoreOptions.Controls.Add(txtAlias);
    grpKeystoreOptions.Controls.Add(txtKeystoreFile);
    grpKeystoreOptions.Location = new Point(382,10);
    grpKeystoreOptions.Name = "grpKeystoreOptions";
    grpKeystoreOptions.Size = new Size(369,148);
    grpKeystoreOptions.TabIndex = 8;
    grpKeystoreOptions.TabStop = false;
    grpKeystoreOptions.Text = "Keystore configuration";
    // 
    // label3
    // 
    label3.AutoSize = true;
    label3.Location = new Point(17,114);
    label3.Name = "label3";
    label3.Size = new Size(109,15);
    label3.TabIndex = 7;
    label3.Text = "Key password         :";
    // 
    // label2
    // 
    label2.AutoSize = true;
    label2.Location = new Point(17,85);
    label2.Name = "label2";
    label2.Size = new Size(111,15);
    label2.TabIndex = 7;
    label2.Text = "Keystore password :";
    // 
    // label1
    // 
    label1.AutoSize = true;
    label1.Location = new Point(17,56);
    label1.Name = "label1";
    label1.Size = new Size(77,15);
    label1.TabIndex = 7;
    label1.Text = "Alias              :";
    // 
    // lblKeystoreFile
    // 
    lblKeystoreFile.AutoSize = true;
    lblKeystoreFile.Location = new Point(17,24);
    lblKeystoreFile.Name = "lblKeystoreFile";
    lblKeystoreFile.Size = new Size(77,15);
    lblKeystoreFile.TabIndex = 7;
    lblKeystoreFile.Text = "Keystore file :";
    // 
    // btnBrowseKeystoreFile
    // 
    btnBrowseKeystoreFile.Location = new Point(331,22);
    btnBrowseKeystoreFile.Name = "btnBrowseKeystoreFile";
    btnBrowseKeystoreFile.Size = new Size(27,23);
    btnBrowseKeystoreFile.TabIndex = 3;
    btnBrowseKeystoreFile.Text = "...";
    btnBrowseKeystoreFile.UseVisualStyleBackColor = true;
    btnBrowseKeystoreFile.Click += btnBrowseKeystoreFile_Click;
    // 
    // txtKeyPassword
    // 
    txtKeyPassword.Location = new Point(137,111);
    txtKeyPassword.Name = "txtKeyPassword";
    txtKeyPassword.PasswordChar = '*';
    txtKeyPassword.Size = new Size(188,23);
    txtKeyPassword.TabIndex = 2;
    txtKeyPassword.UseSystemPasswordChar = true;
    // 
    // txtKeystorePassword
    // 
    txtKeystorePassword.Location = new Point(137,82);
    txtKeystorePassword.Name = "txtKeystorePassword";
    txtKeystorePassword.PasswordChar = '*';
    txtKeystorePassword.Size = new Size(188,23);
    txtKeystorePassword.TabIndex = 2;
    txtKeystorePassword.UseSystemPasswordChar = true;
    // 
    // txtAlias
    // 
    txtAlias.Location = new Point(100,53);
    txtAlias.Name = "txtAlias";
    txtAlias.Size = new Size(225,23);
    txtAlias.TabIndex = 2;
    // 
    // txtKeystoreFile
    // 
    txtKeystoreFile.Location = new Point(100,21);
    txtKeystoreFile.Name = "txtKeystoreFile";
    txtKeystoreFile.Size = new Size(225,23);
    txtKeystoreFile.TabIndex = 2;
    // 
    // grpApk
    // 
    grpApk.Controls.Add(cmbTargetFramework);
    grpApk.Controls.Add(cmbConfiguration);
    grpApk.Controls.Add(btnBrowseApkFile);
    grpApk.Controls.Add(txtApkFile);
    grpApk.Controls.Add(lblTargetFramework);
    grpApk.Controls.Add(lblConfiguration);
    grpApk.Controls.Add(LblApkPath);
    grpApk.Controls.Add(chkPublishAndroidApk);
    grpApk.Location = new Point(8,6);
    grpApk.Name = "grpApk";
    grpApk.Size = new Size(362,152);
    grpApk.TabIndex = 6;
    grpApk.TabStop = false;
    grpApk.Text = "APK package";
    // 
    // cmbTargetFramework
    // 
    cmbTargetFramework.FormattingEnabled = true;
    cmbTargetFramework.Items.AddRange(new object[] { "net10.0-android" });
    cmbTargetFramework.Location = new Point(130,110);
    cmbTargetFramework.Name = "cmbTargetFramework";
    cmbTargetFramework.Size = new Size(212,23);
    cmbTargetFramework.TabIndex = 5;
    // 
    // cmbConfiguration
    // 
    cmbConfiguration.FormattingEnabled = true;
    cmbConfiguration.Items.AddRange(new object[] { "Release","Debug" });
    cmbConfiguration.Location = new Point(130,81);
    cmbConfiguration.Name = "cmbConfiguration";
    cmbConfiguration.Size = new Size(212,23);
    cmbConfiguration.TabIndex = 5;
    // 
    // btnBrowseApkFile
    // 
    btnBrowseApkFile.Location = new Point(315,52);
    btnBrowseApkFile.Name = "btnBrowseApkFile";
    btnBrowseApkFile.Size = new Size(27,23);
    btnBrowseApkFile.TabIndex = 3;
    btnBrowseApkFile.Text = "...";
    btnBrowseApkFile.UseVisualStyleBackColor = true;
    btnBrowseApkFile.Click += btnBrowseApkFile_Click;
    // 
    // txtApkFile
    // 
    txtApkFile.Location = new Point(84,52);
    txtApkFile.Name = "txtApkFile";
    txtApkFile.Size = new Size(225,23);
    txtApkFile.TabIndex = 2;
    // 
    // lblTargetFramework
    // 
    lblTargetFramework.AutoSize = true;
    lblTargetFramework.Location = new Point(15,113);
    lblTargetFramework.Name = "lblTargetFramework";
    lblTargetFramework.Size = new Size(109,15);
    lblTargetFramework.TabIndex = 1;
    lblTargetFramework.Text = "Target framework : ";
    // 
    // lblConfiguration
    // 
    lblConfiguration.AutoSize = true;
    lblConfiguration.Location = new Point(15,84);
    lblConfiguration.Name = "lblConfiguration";
    lblConfiguration.Size = new Size(108,15);
    lblConfiguration.TabIndex = 1;
    lblConfiguration.Text = "Configuration       : ";
    // 
    // LblApkPath
    // 
    LblApkPath.AutoSize = true;
    LblApkPath.Location = new Point(15,55);
    LblApkPath.Name = "LblApkPath";
    LblApkPath.Size = new Size(54,15);
    LblApkPath.TabIndex = 1;
    LblApkPath.Text = "APK file :";
    // 
    // chkPublishAndroidApk
    // 
    chkPublishAndroidApk.AutoSize = true;
    chkPublishAndroidApk.Location = new Point(15,22);
    chkPublishAndroidApk.Name = "chkPublishAndroidApk";
    chkPublishAndroidApk.Size = new Size(136,19);
    chkPublishAndroidApk.TabIndex = 0;
    chkPublishAndroidApk.Text = "Publish Android APK";
    chkPublishAndroidApk.UseVisualStyleBackColor = true;
    // 
    // btnOk
    // 
    btnOk.Location = new Point(675,164);
    btnOk.Name = "btnOk";
    btnOk.Size = new Size(76,23);
    btnOk.TabIndex = 4;
    btnOk.Text = "OK";
    btnOk.UseVisualStyleBackColor = true;
    btnOk.Click += btnOk_Click;
    // 
    // btnCancel
    // 
    btnCancel.Location = new Point(593,164);
    btnCancel.Name = "btnCancel";
    btnCancel.Size = new Size(76,23);
    btnCancel.TabIndex = 4;
    btnCancel.Text = "Cancel";
    btnCancel.UseVisualStyleBackColor = true;
    btnCancel.Click += btnCancel_Click;
    // 
    // AndroidPackageSettingsForm
    // 
    AcceptButton = btnOk;
    AutoScaleDimensions = new SizeF(7F,15F);
    AutoScaleMode = AutoScaleMode.Font;
    CancelButton = btnCancel;
    ClientSize = new Size(775,201);
    Controls.Add(pnlMain);
    MaximizeBox = false;
    MinimizeBox = false;
    Name = "AndroidPackageSettingsForm";
    Padding = new Padding(2);
    StartPosition = FormStartPosition.CenterParent;
    Text = "APK Settings";
    pnlMain.ResumeLayout(false);
    grpKeystoreOptions.ResumeLayout(false);
    grpKeystoreOptions.PerformLayout();
    grpApk.ResumeLayout(false);
    grpApk.PerformLayout();
    ResumeLayout(false);
  }

  #endregion

  private Panel pnlMain;
  private CheckBox chkPublishAndroidApk;
  private Button btnBrowseApkFile;
  private TextBox txtApkFile;
  private Label LblApkPath;
  private Button btnOk;
  private Button btnCancel;
  private Label lblConfiguration;
  private ComboBox cmbTargetFramework;
  private ComboBox cmbConfiguration;
  private Label lblTargetFramework;
  private GroupBox grpApk;
  private Label lblKeystoreFile;
  private Button btnBrowseKeystoreFile;
  private TextBox txtKeystoreFile;
  private Label label3;
  private Label label2;
  private Label label1;
  private TextBox txtKeyPassword;
  private TextBox txtKeystorePassword;
  private TextBox txtAlias;
  private GroupBox grpKeystoreOptions;
}