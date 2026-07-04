namespace MsiSoftwarePackager.UI.Forms;

partial class GlobalSettingsForm {
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
    lblProtocol = new Label();
    cmbProtocol = new ComboBox();
    lblHost = new Label();
    txtHost = new TextBox();
    lblUserName = new Label();
    txtUserName = new TextBox();
    lblRemoteDirectory = new Label();
    txtRemoteDirectory = new TextBox();
    lbLocalWebSiteDir = new Label();
    txtLocalWebSite = new TextBox();
    btnBrowseLocalWebSiteDir = new Button();
    lblWebRemoteSite = new Label();
    txtWebRemoteSite = new TextBox();
    grpUplaodSettings = new GroupBox();
    lblWinScpPath = new Label();
    txtWinScpPath = new TextBox();
    txtBrowseWinScpPath = new Button();
    lblNotepadppPath = new Label();
    txtNotepadppPath = new TextBox();
    btnBrowseNotepadppPath = new Button();
    grpTools = new GroupBox();
    grpCodeSigning = new GroupBox();
    btnGenerateTestCertificate = new Button();
    btnSignToolPath = new Button();
    lblSignToolPath = new Label();
    txtSignToolPath = new TextBox();
    txtTimestampUrl = new TextBox();
    lblTimestampUrl = new Label();
    txtCertificateThumbprint = new TextBox();
    lblCertificateThumbprint = new Label();
    btnOk = new Button();
    btnCancel = new Button();
    pnlMain = new Panel();
    grpBuildPath = new GroupBox();
    btnBrowseBuildRoot = new Button();
    txtBuildRoot = new TextBox();
    lblBuildRoot = new Label();
    txtUploadPassword = new TextBox();
    lblPassword = new Label();
    grpUplaodSettings.SuspendLayout();
    grpTools.SuspendLayout();
    grpCodeSigning.SuspendLayout();
    pnlMain.SuspendLayout();
    grpBuildPath.SuspendLayout();
    SuspendLayout();
    // 
    // lblProtocol
    // 
    lblProtocol.AutoSize = true;
    lblProtocol.Location = new Point(13,23);
    lblProtocol.Name = "lblProtocol";
    lblProtocol.Size = new Size(52,15);
    lblProtocol.TabIndex = 0;
    lblProtocol.Text = "Protocol";
    // 
    // cmbProtocol
    // 
    cmbProtocol.FormattingEnabled = true;
    cmbProtocol.Items.AddRange(new object[] { "FTP","SFTP","FTPS" });
    cmbProtocol.Location = new Point(119,20);
    cmbProtocol.Name = "cmbProtocol";
    cmbProtocol.Size = new Size(78,23);
    cmbProtocol.TabIndex = 1;
    // 
    // lblHost
    // 
    lblHost.AutoSize = true;
    lblHost.Location = new Point(13,52);
    lblHost.Name = "lblHost";
    lblHost.Size = new Size(32,15);
    lblHost.TabIndex = 0;
    lblHost.Text = "Host";
    // 
    // txtHost
    // 
    txtHost.Location = new Point(119,49);
    txtHost.Name = "txtHost";
    txtHost.Size = new Size(307,23);
    txtHost.TabIndex = 2;
    // 
    // lblUserName
    // 
    lblUserName.AutoSize = true;
    lblUserName.Location = new Point(13,81);
    lblUserName.Name = "lblUserName";
    lblUserName.Size = new Size(63,15);
    lblUserName.TabIndex = 0;
    lblUserName.Text = "User name";
    // 
    // txtUserName
    // 
    txtUserName.Location = new Point(119,78);
    txtUserName.Name = "txtUserName";
    txtUserName.Size = new Size(307,23);
    txtUserName.TabIndex = 2;
    // 
    // lblRemoteDirectory
    // 
    lblRemoteDirectory.AutoSize = true;
    lblRemoteDirectory.Location = new Point(13,110);
    lblRemoteDirectory.Name = "lblRemoteDirectory";
    lblRemoteDirectory.Size = new Size(98,15);
    lblRemoteDirectory.TabIndex = 0;
    lblRemoteDirectory.Text = "Remote directory";
    // 
    // txtRemoteDirectory
    // 
    txtRemoteDirectory.Location = new Point(119,107);
    txtRemoteDirectory.Name = "txtRemoteDirectory";
    txtRemoteDirectory.Size = new Size(307,23);
    txtRemoteDirectory.TabIndex = 2;
    // 
    // lbLocalWebSiteDir
    // 
    lbLocalWebSiteDir.AutoSize = true;
    lbLocalWebSiteDir.Location = new Point(13,139);
    lbLocalWebSiteDir.Name = "lbLocalWebSiteDir";
    lbLocalWebSiteDir.Size = new Size(98,15);
    lbLocalWebSiteDir.TabIndex = 0;
    lbLocalWebSiteDir.Text = "Local web site dir";
    // 
    // txtLocalWebSite
    // 
    txtLocalWebSite.Location = new Point(119,136);
    txtLocalWebSite.Name = "txtLocalWebSite";
    txtLocalWebSite.Size = new Size(277,23);
    txtLocalWebSite.TabIndex = 2;
    // 
    // btnBrowseLocalWebSiteDir
    // 
    btnBrowseLocalWebSiteDir.Location = new Point(402,134);
    btnBrowseLocalWebSiteDir.Name = "btnBrowseLocalWebSiteDir";
    btnBrowseLocalWebSiteDir.Size = new Size(24,25);
    btnBrowseLocalWebSiteDir.TabIndex = 3;
    btnBrowseLocalWebSiteDir.Text = "...";
    btnBrowseLocalWebSiteDir.UseVisualStyleBackColor = true;
    btnBrowseLocalWebSiteDir.Click += btnBrowseUploadLocalDirectory_Click;
    // 
    // lblWebRemoteSite
    // 
    lblWebRemoteSite.AutoSize = true;
    lblWebRemoteSite.Location = new Point(13,169);
    lblWebRemoteSite.Name = "lblWebRemoteSite";
    lblWebRemoteSite.Size = new Size(93,15);
    lblWebRemoteSite.TabIndex = 0;
    lblWebRemoteSite.Text = "Web remote site";
    // 
    // txtWebRemoteSite
    // 
    txtWebRemoteSite.Location = new Point(119,165);
    txtWebRemoteSite.Name = "txtWebRemoteSite";
    txtWebRemoteSite.Size = new Size(307,23);
    txtWebRemoteSite.TabIndex = 2;
    // 
    // grpUplaodSettings
    // 
    grpUplaodSettings.Controls.Add(txtUploadPassword);
    grpUplaodSettings.Controls.Add(btnBrowseLocalWebSiteDir);
    grpUplaodSettings.Controls.Add(txtLocalWebSite);
    grpUplaodSettings.Controls.Add(txtWebRemoteSite);
    grpUplaodSettings.Controls.Add(txtRemoteDirectory);
    grpUplaodSettings.Controls.Add(lbLocalWebSiteDir);
    grpUplaodSettings.Controls.Add(lblWebRemoteSite);
    grpUplaodSettings.Controls.Add(txtUserName);
    grpUplaodSettings.Controls.Add(lblRemoteDirectory);
    grpUplaodSettings.Controls.Add(txtHost);
    grpUplaodSettings.Controls.Add(lblUserName);
    grpUplaodSettings.Controls.Add(cmbProtocol);
    grpUplaodSettings.Controls.Add(lblHost);
    grpUplaodSettings.Controls.Add(lblPassword);
    grpUplaodSettings.Controls.Add(lblProtocol);
    grpUplaodSettings.Location = new Point(11,11);
    grpUplaodSettings.Name = "grpUplaodSettings";
    grpUplaodSettings.Size = new Size(432,205);
    grpUplaodSettings.TabIndex = 4;
    grpUplaodSettings.TabStop = false;
    grpUplaodSettings.Text = "Uplaod settings";
    // 
    // lblWinScpPath
    // 
    lblWinScpPath.AutoSize = true;
    lblWinScpPath.Location = new Point(6,23);
    lblWinScpPath.Name = "lblWinScpPath";
    lblWinScpPath.Size = new Size(76,15);
    lblWinScpPath.TabIndex = 0;
    lblWinScpPath.Text = "WinSCP path";
    // 
    // txtWinScpPath
    // 
    txtWinScpPath.Location = new Point(121,20);
    txtWinScpPath.Name = "txtWinScpPath";
    txtWinScpPath.Size = new Size(328,23);
    txtWinScpPath.TabIndex = 2;
    // 
    // txtBrowseWinScpPath
    // 
    txtBrowseWinScpPath.Location = new Point(455,18);
    txtBrowseWinScpPath.Name = "txtBrowseWinScpPath";
    txtBrowseWinScpPath.Size = new Size(24,25);
    txtBrowseWinScpPath.TabIndex = 3;
    txtBrowseWinScpPath.Text = "...";
    txtBrowseWinScpPath.UseVisualStyleBackColor = true;
    txtBrowseWinScpPath.Click += btnBrowseWinScpPath_Click;
    // 
    // lblNotepadppPath
    // 
    lblNotepadppPath.AutoSize = true;
    lblNotepadppPath.Location = new Point(6,52);
    lblNotepadppPath.Name = "lblNotepadppPath";
    lblNotepadppPath.Size = new Size(96,15);
    lblNotepadppPath.TabIndex = 0;
    lblNotepadppPath.Text = "Notepad++ path";
    // 
    // txtNotepadppPath
    // 
    txtNotepadppPath.Location = new Point(121,49);
    txtNotepadppPath.Name = "txtNotepadppPath";
    txtNotepadppPath.Size = new Size(328,23);
    txtNotepadppPath.TabIndex = 2;
    // 
    // btnBrowseNotepadppPath
    // 
    btnBrowseNotepadppPath.Location = new Point(455,47);
    btnBrowseNotepadppPath.Name = "btnBrowseNotepadppPath";
    btnBrowseNotepadppPath.Size = new Size(24,25);
    btnBrowseNotepadppPath.TabIndex = 3;
    btnBrowseNotepadppPath.Text = "...";
    btnBrowseNotepadppPath.UseVisualStyleBackColor = true;
    btnBrowseNotepadppPath.Click += btnBrowseNotepadPlusPlusPath_Click;
    // 
    // grpTools
    // 
    grpTools.Controls.Add(btnBrowseNotepadppPath);
    grpTools.Controls.Add(txtNotepadppPath);
    grpTools.Controls.Add(txtBrowseWinScpPath);
    grpTools.Controls.Add(txtWinScpPath);
    grpTools.Controls.Add(lblNotepadppPath);
    grpTools.Controls.Add(lblWinScpPath);
    grpTools.Location = new Point(449,11);
    grpTools.Name = "grpTools";
    grpTools.Size = new Size(501,85);
    grpTools.TabIndex = 5;
    grpTools.TabStop = false;
    grpTools.Text = "Tools";
    // 
    // grpCodeSigning
    // 
    grpCodeSigning.Controls.Add(btnGenerateTestCertificate);
    grpCodeSigning.Controls.Add(btnSignToolPath);
    grpCodeSigning.Controls.Add(lblSignToolPath);
    grpCodeSigning.Controls.Add(txtSignToolPath);
    grpCodeSigning.Controls.Add(txtTimestampUrl);
    grpCodeSigning.Controls.Add(lblTimestampUrl);
    grpCodeSigning.Controls.Add(txtCertificateThumbprint);
    grpCodeSigning.Controls.Add(lblCertificateThumbprint);
    grpCodeSigning.Location = new Point(449,118);
    grpCodeSigning.Name = "grpCodeSigning";
    grpCodeSigning.Size = new Size(501,145);
    grpCodeSigning.TabIndex = 6;
    grpCodeSigning.TabStop = false;
    grpCodeSigning.Text = "Code signing";
    // 
    // btnGenerateTestCertificate
    // 
    btnGenerateTestCertificate.Location = new Point(6,82);
    btnGenerateTestCertificate.Name = "btnGenerateTestCertificate";
    btnGenerateTestCertificate.Size = new Size(144,23);
    btnGenerateTestCertificate.TabIndex = 4;
    btnGenerateTestCertificate.Text = "Generate test certificate";
    btnGenerateTestCertificate.UseVisualStyleBackColor = true;
    btnGenerateTestCertificate.Click += btnGenerateSelfSignedCertificate_Click;
    // 
    // btnSignToolPath
    // 
    btnSignToolPath.Location = new Point(455,22);
    btnSignToolPath.Name = "btnSignToolPath";
    btnSignToolPath.Size = new Size(24,25);
    btnSignToolPath.TabIndex = 3;
    btnSignToolPath.Text = "...";
    btnSignToolPath.UseVisualStyleBackColor = true;
    btnSignToolPath.Click += btnBrowseSignToolPath_Click;
    // 
    // lblSignToolPath
    // 
    lblSignToolPath.AutoSize = true;
    lblSignToolPath.Location = new Point(6,27);
    lblSignToolPath.Name = "lblSignToolPath";
    lblSignToolPath.Size = new Size(80,15);
    lblSignToolPath.TabIndex = 0;
    lblSignToolPath.Text = "SignTool path";
    // 
    // txtSignToolPath
    // 
    txtSignToolPath.Location = new Point(120,24);
    txtSignToolPath.Name = "txtSignToolPath";
    txtSignToolPath.Size = new Size(329,23);
    txtSignToolPath.TabIndex = 2;
    // 
    // txtTimestampUrl
    // 
    txtTimestampUrl.Location = new Point(121,111);
    txtTimestampUrl.Name = "txtTimestampUrl";
    txtTimestampUrl.Size = new Size(358,23);
    txtTimestampUrl.TabIndex = 2;
    // 
    // lblTimestampUrl
    // 
    lblTimestampUrl.AutoSize = true;
    lblTimestampUrl.Location = new Point(15,114);
    lblTimestampUrl.Name = "lblTimestampUrl";
    lblTimestampUrl.Size = new Size(91,15);
    lblTimestampUrl.TabIndex = 0;
    lblTimestampUrl.Text = "Timestamp URL";
    // 
    // txtCertificateThumbprint
    // 
    txtCertificateThumbprint.Location = new Point(135,53);
    txtCertificateThumbprint.Name = "txtCertificateThumbprint";
    txtCertificateThumbprint.Size = new Size(344,23);
    txtCertificateThumbprint.TabIndex = 2;
    // 
    // lblCertificateThumbprint
    // 
    lblCertificateThumbprint.AutoSize = true;
    lblCertificateThumbprint.Location = new Point(6,56);
    lblCertificateThumbprint.Name = "lblCertificateThumbprint";
    lblCertificateThumbprint.Size = new Size(125,15);
    lblCertificateThumbprint.TabIndex = 0;
    lblCertificateThumbprint.Text = "Certificate thumbprint";
    // 
    // btnOk
    // 
    btnOk.Location = new Point(347,278);
    btnOk.Name = "btnOk";
    btnOk.Size = new Size(97,32);
    btnOk.TabIndex = 7;
    btnOk.Text = "OK";
    btnOk.UseVisualStyleBackColor = true;
    btnOk.Click += btnOk_Click;
    // 
    // btnCancel
    // 
    btnCancel.Location = new Point(450,278);
    btnCancel.Name = "btnCancel";
    btnCancel.Size = new Size(97,32);
    btnCancel.TabIndex = 7;
    btnCancel.Text = "Cancel";
    btnCancel.UseVisualStyleBackColor = true;
    btnCancel.Click += btnCancel_Click;
    // 
    // pnlMain
    // 
    pnlMain.Controls.Add(grpBuildPath);
    pnlMain.Controls.Add(btnCancel);
    pnlMain.Controls.Add(btnOk);
    pnlMain.Controls.Add(grpCodeSigning);
    pnlMain.Controls.Add(grpTools);
    pnlMain.Controls.Add(grpUplaodSettings);
    pnlMain.Dock = DockStyle.Fill;
    pnlMain.Location = new Point(1,1);
    pnlMain.Margin = new Padding(0);
    pnlMain.Name = "pnlMain";
    pnlMain.Size = new Size(964,324);
    pnlMain.TabIndex = 8;
    // 
    // grpBuildPath
    // 
    grpBuildPath.Controls.Add(btnBrowseBuildRoot);
    grpBuildPath.Controls.Add(txtBuildRoot);
    grpBuildPath.Controls.Add(lblBuildRoot);
    grpBuildPath.Location = new Point(11,218);
    grpBuildPath.Name = "grpBuildPath";
    grpBuildPath.Size = new Size(432,44);
    grpBuildPath.TabIndex = 8;
    grpBuildPath.TabStop = false;
    grpBuildPath.Text = "Build path";
    // 
    // btnBrowseBuildRoot
    // 
    btnBrowseBuildRoot.Location = new Point(402,13);
    btnBrowseBuildRoot.Name = "btnBrowseBuildRoot";
    btnBrowseBuildRoot.Size = new Size(24,25);
    btnBrowseBuildRoot.TabIndex = 3;
    btnBrowseBuildRoot.Text = "...";
    btnBrowseBuildRoot.UseVisualStyleBackColor = true;
    btnBrowseBuildRoot.Click += btnBrowseBuildRoot_Click;
    // 
    // txtBuildRoot
    // 
    txtBuildRoot.Location = new Point(119,15);
    txtBuildRoot.Name = "txtBuildRoot";
    txtBuildRoot.Size = new Size(277,23);
    txtBuildRoot.TabIndex = 2;
    // 
    // lblBuildRoot
    // 
    lblBuildRoot.AutoSize = true;
    lblBuildRoot.Location = new Point(13,18);
    lblBuildRoot.Name = "lblBuildRoot";
    lblBuildRoot.Size = new Size(59,15);
    lblBuildRoot.TabIndex = 0;
    lblBuildRoot.Text = "Build root";
    // 
    // txtUploadPassword
    // 
    txtUploadPassword.Location = new Point(290,19);
    txtUploadPassword.Name = "txtUploadPassword";
    txtUploadPassword.Size = new Size(136,23);
    txtUploadPassword.TabIndex = 4;
    txtUploadPassword.UseSystemPasswordChar = true;
    // 
    // lblPassword
    // 
    lblPassword.AutoSize = true;
    lblPassword.Location = new Point(223,23);
    lblPassword.Name = "lblPassword";
    lblPassword.Size = new Size(57,15);
    lblPassword.TabIndex = 0;
    lblPassword.Text = "Password";
    // 
    // GlobalSettingsForm
    // 
    AutoScaleDimensions = new SizeF(7F,15F);
    AutoScaleMode = AutoScaleMode.Font;
    ClientSize = new Size(966,326);
    Controls.Add(pnlMain);
    FormBorderStyle = FormBorderStyle.None;
    Name = "GlobalSettingsForm";
    Padding = new Padding(1);
    StartPosition = FormStartPosition.CenterScreen;
    Text = "GlobalSettings";
    grpUplaodSettings.ResumeLayout(false);
    grpUplaodSettings.PerformLayout();
    grpTools.ResumeLayout(false);
    grpTools.PerformLayout();
    grpCodeSigning.ResumeLayout(false);
    grpCodeSigning.PerformLayout();
    pnlMain.ResumeLayout(false);
    grpBuildPath.ResumeLayout(false);
    grpBuildPath.PerformLayout();
    ResumeLayout(false);
  }

  #endregion

  private Label lblProtocol;
  private ComboBox cmbProtocol;
  private Label lblHost;
  private TextBox txtHost;
  private Label lblUserName;
  private TextBox txtUserName;
  private Label lblRemoteDirectory;
  private TextBox txtRemoteDirectory;
  private Label lbLocalWebSiteDir;
  private TextBox txtLocalWebSite;
  private Button btnBrowseLocalWebSiteDir;
  private Label lblWebRemoteSite;
  private TextBox txtWebRemoteSite;
  private GroupBox grpUplaodSettings;
  private Label lblWinScpPath;
  private TextBox txtWinScpPath;
  private Button txtBrowseWinScpPath;
  private Label lblNotepadppPath;
  private TextBox txtNotepadppPath;
  private Button btnBrowseNotepadppPath;
  private GroupBox grpTools;
  private GroupBox grpCodeSigning;
  private Button btnSignToolPath;
  private Label lblSignToolPath;
  private TextBox txtSignToolPath;
  private TextBox txtTimestampUrl;
  private Label lblTimestampUrl;
  private TextBox txtCertificateThumbprint;
  private Button btnOk;
  private Button btnCancel;
  private Panel pnlMain;
  private Label lblCertificateThumbprint;
  private Button btnGenerateTestCertificate;
  private GroupBox grpBuildPath;
  private Button btnBrowseBuildRoot;
  private TextBox txtBuildRoot;
  private Label lblBuildRoot;
  private TextBox txtUploadPassword;
  private Label lblPassword;
}