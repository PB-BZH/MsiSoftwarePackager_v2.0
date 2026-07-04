namespace MsiSoftwarePackager.UI.Forms;

partial class CodeSigningSettingsForm {
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
    chkSignArtifactsAfterBuild = new CheckBox();
    lblSignToolPath = new Label();
    txtSignToolPath = new TextBox();
    btnFindSignTool = new Button();
    btnOk = new Button();
    btnCancel = new Button();
    btnGenerateSelfSignedCertificate = new Button();
    txtCertificateThumbprint = new TextBox();
    lblTimestampURL = new Label();
    txtTimestampUrl = new TextBox();
    chkSignMsi = new CheckBox();
    chkSignBundle = new CheckBox();
    chkSignWebSetup = new CheckBox();
    grpArtifacts = new GroupBox();
    chkSignExecutable = new CheckBox();
    pnlMain = new Panel();
    cmbSigningProvider = new ComboBox();
    btnBrowseCodeSignTool = new Button();
    txtPasswordSsl = new TextBox();
    txtLoginSsl = new TextBox();
    lblPasswordSsl = new Label();
    txtSslCodeSignTool = new TextBox();
    txtCredentialId = new TextBox();
    lblCredentialId = new Label();
    lblLogin = new Label();
    txtCodeSignToolPath = new TextBox();
    lblSslCodeSignTool = new Label();
    lblCodeSignToolPath = new Label();
    lblSigningProvider = new Label();
    grpArtifacts.SuspendLayout();
    pnlMain.SuspendLayout();
    SuspendLayout();
    // 
    // chkSignArtifactsAfterBuild
    // 
    chkSignArtifactsAfterBuild.AutoSize = true;
    chkSignArtifactsAfterBuild.Location = new Point(19,24);
    chkSignArtifactsAfterBuild.Name = "chkSignArtifactsAfterBuild";
    chkSignArtifactsAfterBuild.Size = new Size(151,19);
    chkSignArtifactsAfterBuild.TabIndex = 0;
    chkSignArtifactsAfterBuild.Text = "Sign artifacts after build";
    chkSignArtifactsAfterBuild.UseVisualStyleBackColor = true;
    // 
    // lblSignToolPath
    // 
    lblSignToolPath.AutoSize = true;
    lblSignToolPath.Location = new Point(8,70);
    lblSignToolPath.Name = "lblSignToolPath";
    lblSignToolPath.Size = new Size(83,15);
    lblSignToolPath.TabIndex = 1;
    lblSignToolPath.Text = "SignTool path:";
    // 
    // txtSignToolPath
    // 
    txtSignToolPath.Location = new Point(128,66);
    txtSignToolPath.Name = "txtSignToolPath";
    txtSignToolPath.Size = new Size(425,23);
    txtSignToolPath.TabIndex = 2;
    // 
    // btnFindSignTool
    // 
    btnFindSignTool.Location = new Point(558,66);
    btnFindSignTool.Margin = new Padding(0);
    btnFindSignTool.Name = "btnFindSignTool";
    btnFindSignTool.Size = new Size(94,23);
    btnFindSignTool.TabIndex = 3;
    btnFindSignTool.Text = "Find SignTool";
    btnFindSignTool.UseVisualStyleBackColor = true;
    btnFindSignTool.Click += btnFindSignTool_Click;
    // 
    // btnOk
    // 
    btnOk.Location = new Point(238,361);
    btnOk.Name = "btnOk";
    btnOk.Size = new Size(89,26);
    btnOk.TabIndex = 10;
    btnOk.Text = "Ok";
    btnOk.UseVisualStyleBackColor = true;
    btnOk.Click += btnOk_Click;
    // 
    // btnCancel
    // 
    btnCancel.Location = new Point(333,361);
    btnCancel.Name = "btnCancel";
    btnCancel.Size = new Size(89,26);
    btnCancel.TabIndex = 11;
    btnCancel.Text = "Cancel";
    btnCancel.UseVisualStyleBackColor = true;
    btnCancel.Click += btnCancel_Click;
    // 
    // btnGenerateSelfSignedCertificate
    // 
    btnGenerateSelfSignedCertificate.AutoSize = true;
    btnGenerateSelfSignedCertificate.Location = new Point(20,91);
    btnGenerateSelfSignedCertificate.Name = "btnGenerateSelfSignedCertificate";
    btnGenerateSelfSignedCertificate.Size = new Size(202,28);
    btnGenerateSelfSignedCertificate.TabIndex = 12;
    btnGenerateSelfSignedCertificate.Text = "Generate self-signed test certificate";
    btnGenerateSelfSignedCertificate.UseVisualStyleBackColor = true;
    btnGenerateSelfSignedCertificate.Click += btnGenerateSelfSignedCertificate_Click;
    // 
    // txtCertificateThumbprint
    // 
    txtCertificateThumbprint.Location = new Point(228,95);
    txtCertificateThumbprint.Name = "txtCertificateThumbprint";
    txtCertificateThumbprint.Size = new Size(424,23);
    txtCertificateThumbprint.TabIndex = 2;
    // 
    // lblTimestampURL
    // 
    lblTimestampURL.AutoSize = true;
    lblTimestampURL.Location = new Point(8,215);
    lblTimestampURL.Name = "lblTimestampURL";
    lblTimestampURL.Size = new Size(94,15);
    lblTimestampURL.TabIndex = 1;
    lblTimestampURL.Text = "Timestamp URL:";
    // 
    // txtTimestampUrl
    // 
    txtTimestampUrl.Location = new Point(128,211);
    txtTimestampUrl.Name = "txtTimestampUrl";
    txtTimestampUrl.Size = new Size(524,23);
    txtTimestampUrl.TabIndex = 2;
    // 
    // chkSignMsi
    // 
    chkSignMsi.AutoSize = true;
    chkSignMsi.Location = new Point(199,22);
    chkSignMsi.Name = "chkSignMsi";
    chkSignMsi.Size = new Size(71,19);
    chkSignMsi.TabIndex = 0;
    chkSignMsi.Text = "Sign Msi";
    chkSignMsi.UseVisualStyleBackColor = true;
    // 
    // chkSignBundle
    // 
    chkSignBundle.AutoSize = true;
    chkSignBundle.Location = new Point(306,22);
    chkSignBundle.Name = "chkSignBundle";
    chkSignBundle.Size = new Size(141,19);
    chkSignBundle.TabIndex = 0;
    chkSignBundle.Text = "Sign setup.exe bundle";
    chkSignBundle.UseVisualStyleBackColor = true;
    // 
    // chkSignWebSetup
    // 
    chkSignWebSetup.AutoSize = true;
    chkSignWebSetup.Location = new Point(473,22);
    chkSignWebSetup.Name = "chkSignWebSetup";
    chkSignWebSetup.Size = new Size(126,19);
    chkSignWebSetup.TabIndex = 0;
    chkSignWebSetup.Text = "Sign web setup.exe";
    chkSignWebSetup.UseVisualStyleBackColor = true;
    // 
    // grpArtifacts
    // 
    grpArtifacts.Controls.Add(chkSignWebSetup);
    grpArtifacts.Controls.Add(chkSignBundle);
    grpArtifacts.Controls.Add(chkSignExecutable);
    grpArtifacts.Controls.Add(chkSignMsi);
    grpArtifacts.Location = new Point(20,289);
    grpArtifacts.Name = "grpArtifacts";
    grpArtifacts.Size = new Size(633,53);
    grpArtifacts.TabIndex = 14;
    grpArtifacts.TabStop = false;
    grpArtifacts.Text = "Artifacts";
    // 
    // chkSignExecutable
    // 
    chkSignExecutable.AutoSize = true;
    chkSignExecutable.Location = new Point(18,22);
    chkSignExecutable.Name = "chkSignExecutable";
    chkSignExecutable.Size = new Size(163,19);
    chkSignExecutable.TabIndex = 0;
    chkSignExecutable.Text = "Sign published executable";
    chkSignExecutable.UseVisualStyleBackColor = true;
    // 
    // pnlMain
    // 
    pnlMain.BackColor = SystemColors.Control;
    pnlMain.Controls.Add(cmbSigningProvider);
    pnlMain.Controls.Add(grpArtifacts);
    pnlMain.Controls.Add(btnGenerateSelfSignedCertificate);
    pnlMain.Controls.Add(btnCancel);
    pnlMain.Controls.Add(btnOk);
    pnlMain.Controls.Add(btnBrowseCodeSignTool);
    pnlMain.Controls.Add(btnFindSignTool);
    pnlMain.Controls.Add(txtCertificateThumbprint);
    pnlMain.Controls.Add(txtPasswordSsl);
    pnlMain.Controls.Add(txtLoginSsl);
    pnlMain.Controls.Add(lblPasswordSsl);
    pnlMain.Controls.Add(txtSslCodeSignTool);
    pnlMain.Controls.Add(txtCredentialId);
    pnlMain.Controls.Add(txtTimestampUrl);
    pnlMain.Controls.Add(lblCredentialId);
    pnlMain.Controls.Add(lblLogin);
    pnlMain.Controls.Add(txtCodeSignToolPath);
    pnlMain.Controls.Add(lblSslCodeSignTool);
    pnlMain.Controls.Add(txtSignToolPath);
    pnlMain.Controls.Add(lblTimestampURL);
    pnlMain.Controls.Add(lblCodeSignToolPath);
    pnlMain.Controls.Add(lblSigningProvider);
    pnlMain.Controls.Add(lblSignToolPath);
    pnlMain.Controls.Add(chkSignArtifactsAfterBuild);
    pnlMain.Dock = DockStyle.Fill;
    pnlMain.ForeColor = SystemColors.ActiveCaptionText;
    pnlMain.Location = new Point(1,1);
    pnlMain.Margin = new Padding(0);
    pnlMain.Name = "pnlMain";
    pnlMain.Size = new Size(676,401);
    pnlMain.TabIndex = 15;
    // 
    // cmbSigningProvider
    // 
    cmbSigningProvider.FormattingEnabled = true;
    cmbSigningProvider.Items.AddRange(new object[] { "SignToolCka","SslComCodeSignTool" });
    cmbSigningProvider.Location = new Point(352,22);
    cmbSigningProvider.Name = "cmbSigningProvider";
    cmbSigningProvider.Size = new Size(201,23);
    cmbSigningProvider.TabIndex = 15;
    // 
    // btnBrowseCodeSignTool
    // 
    btnBrowseCodeSignTool.Location = new Point(622,153);
    btnBrowseCodeSignTool.Margin = new Padding(0);
    btnBrowseCodeSignTool.Name = "btnBrowseCodeSignTool";
    btnBrowseCodeSignTool.Size = new Size(30,23);
    btnBrowseCodeSignTool.TabIndex = 3;
    btnBrowseCodeSignTool.Text = "...";
    btnBrowseCodeSignTool.UseVisualStyleBackColor = true;
    btnBrowseCodeSignTool.Click += btnBrowseCodeSignTool_Click;
    // 
    // txtPasswordSsl
    // 
    txtPasswordSsl.Location = new Point(417,245);
    txtPasswordSsl.Name = "txtPasswordSsl";
    txtPasswordSsl.PasswordChar = '●';
    txtPasswordSsl.Size = new Size(163,23);
    txtPasswordSsl.TabIndex = 2;
    // 
    // txtLoginSsl
    // 
    txtLoginSsl.Location = new Point(148,245);
    txtLoginSsl.Name = "txtLoginSsl";
    txtLoginSsl.Size = new Size(163,23);
    txtLoginSsl.TabIndex = 2;
    // 
    // lblPasswordSsl
    // 
    lblPasswordSsl.AutoSize = true;
    lblPasswordSsl.Location = new Point(327,248);
    lblPasswordSsl.Name = "lblPasswordSsl";
    lblPasswordSsl.Size = new Size(84,15);
    lblPasswordSsl.TabIndex = 1;
    lblPasswordSsl.Text = "Password SSL :";
    // 
    // txtSslCodeSignTool
    // 
    txtSslCodeSignTool.Location = new Point(128,182);
    txtSslCodeSignTool.Name = "txtSslCodeSignTool";
    txtSslCodeSignTool.PasswordChar = '●';
    txtSslCodeSignTool.Size = new Size(524,23);
    txtSslCodeSignTool.TabIndex = 2;
    // 
    // txtCredentialId
    // 
    txtCredentialId.Location = new Point(128,124);
    txtCredentialId.Name = "txtCredentialId";
    txtCredentialId.Size = new Size(524,23);
    txtCredentialId.TabIndex = 2;
    // 
    // lblCredentialId
    // 
    lblCredentialId.AutoSize = true;
    lblCredentialId.Location = new Point(8,128);
    lblCredentialId.Name = "lblCredentialId";
    lblCredentialId.Size = new Size(81,15);
    lblCredentialId.TabIndex = 1;
    lblCredentialId.Text = "Credential ID :";
    // 
    // lblLogin
    // 
    lblLogin.AutoSize = true;
    lblLogin.Location = new Point(72,248);
    lblLogin.Name = "lblLogin";
    lblLogin.Size = new Size(64,15);
    lblLogin.TabIndex = 1;
    lblLogin.Text = "Login SSL :";
    // 
    // txtCodeSignToolPath
    // 
    txtCodeSignToolPath.Location = new Point(128,153);
    txtCodeSignToolPath.Name = "txtCodeSignToolPath";
    txtCodeSignToolPath.Size = new Size(491,23);
    txtCodeSignToolPath.TabIndex = 2;
    // 
    // lblSslCodeSignTool
    // 
    lblSslCodeSignTool.AutoSize = true;
    lblSslCodeSignTool.Location = new Point(8,186);
    lblSslCodeSignTool.Name = "lblSslCodeSignTool";
    lblSslCodeSignTool.Size = new Size(109,15);
    lblSslCodeSignTool.TabIndex = 1;
    lblSslCodeSignTool.Text = "SSL code SignTool :";
    // 
    // lblCodeSignToolPath
    // 
    lblCodeSignToolPath.AutoSize = true;
    lblCodeSignToolPath.Location = new Point(8,157);
    lblCodeSignToolPath.Name = "lblCodeSignToolPath";
    lblCodeSignToolPath.Size = new Size(114,15);
    lblCodeSignToolPath.TabIndex = 1;
    lblCodeSignToolPath.Text = "CodeSignTool path :";
    // 
    // lblSigningProvider
    // 
    lblSigningProvider.AutoSize = true;
    lblSigningProvider.Location = new Point(238,25);
    lblSigningProvider.Name = "lblSigningProvider";
    lblSigningProvider.Size = new Size(100,15);
    lblSigningProvider.TabIndex = 1;
    lblSigningProvider.Text = "Signing provider :";
    // 
    // CodeSigningSettingsForm
    // 
    AutoScaleDimensions = new SizeF(7F,15F);
    AutoScaleMode = AutoScaleMode.Font;
    BackColor = Color.DimGray;
    ClientSize = new Size(678,403);
    Controls.Add(pnlMain);
    ForeColor = Color.White;
    FormBorderStyle = FormBorderStyle.None;
    Name = "CodeSigningSettingsForm";
    Padding = new Padding(1);
    StartPosition = FormStartPosition.CenterScreen;
    Text = "CodeSigning_Settings";
    grpArtifacts.ResumeLayout(false);
    grpArtifacts.PerformLayout();
    pnlMain.ResumeLayout(false);
    pnlMain.PerformLayout();
    ResumeLayout(false);
  }

  #endregion

  private CheckBox chkSignArtifactsAfterBuild;
  private Label lblSignToolPath;
  private TextBox txtSignToolPath;
  private Button btnFindSignTool;
  private Button btnOk;
  private Button btnCancel;
  private Button btnGenerateSelfSignedCertificate;
  private TextBox txtCertificateThumbprint;
  private Label lblTimestampURL;
  private TextBox txtTimestampUrl;
  private CheckBox chkSignMsi;
  private CheckBox chkSignBundle;
  private CheckBox chkSignWebSetup;
  private GroupBox grpArtifacts;
  private Panel pnlMain;
  private CheckBox chkSignExecutable;
  private TextBox txtPasswordSsl;
  private TextBox txtLoginSsl;
  private Label lblPasswordSsl;
  private Label lblLogin;
  private TextBox txtSslCodeSignTool;
  private TextBox txtCredentialId;
  private Label lblCredentialId;
  private Label lblSslCodeSignTool;
  private ComboBox cmbSigningProvider;
  private Label lblSigningProvider;
  private Button btnBrowseCodeSignTool;
  private TextBox txtCodeSignToolPath;
  private Label lblCodeSignToolPath;
}