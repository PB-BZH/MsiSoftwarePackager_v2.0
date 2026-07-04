using System.Resources;
using PB.BZH.Help.Library.UI.Theming;

namespace MsiSoftwarePackager.UI;

partial class MainForm {
  private System.ComponentModel.IContainer components = null;

  private Label lblProductName;
  private Label lblManufacturer;
  private Label lblVersion;
  private Label lblProjectFile;
  private Label lblExecutableName;
  private Label lblPublishDir;
  private Label lblMsiOutputDir;

  private TextBox txtProductName;
  private TextBox txtManufacturer;
  private TextBox txtVersion;
  private TextBox txtProjectFile;
  private TextBox txtExecutableName;
  private TextBox txtPublishDir;
  private TextBox txtMsiOutputDir;

  private CheckBox chkDesktopShortcut;
  private CheckBox chkStartMenuShortcut;
  private CheckBox chkAddToPath;

  private Button btnLoadProfile;
  private Button btnSaveProfile;
  private Button btnGenerateWix;
  private Button btnBuildMsi;

  private TabControl tabPreview;
  private Button btnBrowseProjectFile;
  private Button btnBrowsePublishDir;
  private Button btnBrowseMsiOutputDir;
  private Label lblIconPath;
  private TextBox txtIconPath;
  private Button btnBrowseIconPath;
  private Button btnOpenPackagerFolder;
  private ToolTip toolTip;
  private MenuStrip mainMenu;

  protected override void Dispose(bool disposing) {
    if (disposing && components != null)
      components.Dispose();

    base.Dispose(disposing);
  }

  private void InitializeComponent() {
    components = new System.ComponentModel.Container();
    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
    mainMenu = new MenuStrip();
    mnuFile = new ToolStripMenuItem();
    mnuNewProfile = new ToolStripMenuItem();
    mnuNewProfileFromCsproj = new ToolStripMenuItem();
    mnuLoadProfile = new ToolStripMenuItem();
    mnuSaveProfile = new ToolStripMenuItem();
    mnuSaveProfileAs = new ToolStripMenuItem();
    toolStripSeparator4 = new ToolStripSeparator();
    mnuUpdateCsprojFromProfile = new ToolStripMenuItem();
    toolStripSeparator1 = new ToolStripSeparator();
    mnuPrintSelectedView = new ToolStripMenuItem();
    mnuEditSelectedViewNoteapad = new ToolStripMenuItem();
    toolStripSeparator2 = new ToolStripSeparator();
    mnuExit = new ToolStripMenuItem();
    mnuBuild = new ToolStripMenuItem();
    mnuGenerateWix = new ToolStripMenuItem();
    mnuBuildMsi = new ToolStripMenuItem();
    toolStripSeparator3 = new ToolStripSeparator();
    mnuGenerateWebSiteOnly = new ToolStripMenuItem();
    mnuUploadWebSiteOnly = new ToolStripMenuItem();
    toolStripSeparator8 = new ToolStripSeparator();
    mnuReleaseChecklist = new ToolStripMenuItem();
    mnuReleaseSummary = new ToolStripMenuItem();
    mnuTools = new ToolStripMenuItem();
    mnuValidateProfile = new ToolStripMenuItem();
    mnuRestoreDefaultWinScpPath = new ToolStripMenuItem();
    mnuSaveUploadCredential = new ToolStripMenuItem();
    toolStripSeparator5 = new ToolStripSeparator();
    mnuCodeSigningSettings = new ToolStripMenuItem();
    toolStripSeparator6 = new ToolStripSeparator();
    mnuOpenPackagerFolder = new ToolStripMenuItem();
    mnuOpenLocalSiteWeb = new ToolStripMenuItem();
    mnuOpenBuildReportsFolder = new ToolStripMenuItem();
    mnuOpenBuildArchivesFolder = new ToolStripMenuItem();
    mnuRefreshBuildHistory = new ToolStripMenuItem();
    toolStripSeparator7 = new ToolStripSeparator();
    mnuEnvironmentCheck = new ToolStripMenuItem();
    mnuGlobalSettings = new ToolStripMenuItem();
    mnuOpenGlobalSettingsFile = new ToolStripMenuItem();
    mnuOpenGlobalSettingsFolder = new ToolStripMenuItem();
    mnuHelp = new ToolStripMenuItem();
    mnuCheckForUpdates = new ToolStripMenuItem();
    mnuAbout = new ToolStripMenuItem();
    mnuLicense = new ToolStripMenuItem();
    mnuImportLicense = new ToolStripMenuItem();
    mnuUploadNow = new ToolStripMenuItem();
    btnOpenPackagerFolder = new Button();
    lblIconPath = new Label();
    txtIconPath = new TextBox();
    btnBrowseIconPath = new Button();
    btnBrowseProjectFile = new Button();
    btnBrowsePublishDir = new Button();
    btnBrowseMsiOutputDir = new Button();
    lblProductName = new Label();
    lblManufacturer = new Label();
    lblVersion = new Label();
    lblProjectFile = new Label();
    lblExecutableName = new Label();
    lblPublishDir = new Label();
    lblMsiOutputDir = new Label();
    txtProductName = new TextBox();
    txtManufacturer = new TextBox();
    txtVersion = new TextBox();
    txtProjectFile = new TextBox();
    txtExecutableName = new TextBox();
    txtPublishDir = new TextBox();
    txtMsiOutputDir = new TextBox();
    chkDesktopShortcut = new CheckBox();
    chkStartMenuShortcut = new CheckBox();
    chkAddToPath = new CheckBox();
    btnLoadProfile = new Button();
    btnSaveProfile = new Button();
    btnGenerateWix = new Button();
    btnBuildMsi = new Button();
    tabPreview = new TabControl();
    txtUpgradeCode = new TextBox();
    btnGenerateUpgradeCode = new Button();
    btnBrowseWixOutputDir = new Button();
    txtWixOutputDir = new TextBox();
    label1 = new Label();
    chkRequireAdministrator = new CheckBox();
    btnVersionPatch = new Button();
    btnVersionMinor = new Button();
    btnVersionMajor = new Button();
    chkBuildBundle = new CheckBox();
    txtBundleOutputDir = new TextBox();
    lblBundleOutputDir = new Label();
    btnBrowseBundleDir = new Button();
    lblBundleFileName = new Label();
    txtBundleFileName = new TextBox();
    txtBundleUpgradeCode = new TextBox();
    grpbxFeature = new GroupBox();
    grpbxBundle = new GroupBox();
    lblBundleName = new Label();
    txtBundleName = new TextBox();
    btnGenerateBundleUpgradeCode = new Button();
    grpbxMisOptions = new GroupBox();
    chkTechnicalLicenseRequired = new CheckBox();
    chkRequireLicenseAcceptance = new CheckBox();
    grpWebInstaller = new GroupBox();
    lblWebBundleName = new Label();
    lblWebPublishDir = new Label();
    lblMsiDownloadUrl = new Label();
    lblWebPoductFolder = new Label();
    lblWebSetupFileName = new Label();
    lblWebOutputDir = new Label();
    chkBuildWebInstaller = new CheckBox();
    txtWebBundleUpgradeCode = new TextBox();
    txtWebPublishDirectory = new TextBox();
    txtWebBundleName = new TextBox();
    txtMsiDownloadUrl = new TextBox();
    txtWebProductFolder = new TextBox();
    txtWebSetupFileName = new TextBox();
    txtWebOutputDir = new TextBox();
    btnWebPublisDir = new Button();
    btnBrowseWebOutputDir = new Button();
    btnGenerateWebBundleUpgradeCode = new Button();
    toolTip = new ToolTip(components);
    grpWebUpload = new GroupBox();
    cmbUploadProtocol = new ComboBox();
    lblProtocole = new Label();
    lblWebSite = new Label();
    lblWinScpPath = new Label();
    lblRemoteDir = new Label();
    lblLocalDir = new Label();
    lblPassword = new Label();
    lblUserName = new Label();
    lblUplaodHost = new Label();
    chkUploadWebFilesAfterBuild = new CheckBox();
    txtUploadLocalDir = new TextBox();
    txtWebRemoteSite = new TextBox();
    btnBrowseWinScpPath = new Button();
    btnBrowseUploadLocalDir = new Button();
    textBox1 = new TextBox();
    txtWinScpPath = new TextBox();
    txtUploadRemoteDir = new TextBox();
    txtUploadPassword = new TextBox();
    txtUploadUserName = new TextBox();
    txtUploadHost = new TextBox();
    btnNewProfile = new Button();
    btnSaveProfilAs = new Button();
    pnlMain = new Panel();
    btnTestUploadConnection = new Button();
    btnUploadNow = new Button();
    txtLog = new RichTextBox();
    btnAndroidPackageSettings = new Button();
    mainMenu.SuspendLayout();
    grpbxFeature.SuspendLayout();
    grpbxBundle.SuspendLayout();
    grpbxMisOptions.SuspendLayout();
    grpWebInstaller.SuspendLayout();
    grpWebUpload.SuspendLayout();
    pnlMain.SuspendLayout();
    SuspendLayout();
    // 
    // mainMenu
    // 
    mainMenu.Items.AddRange(new ToolStripItem[] { mnuFile,mnuBuild,mnuTools,mnuHelp });
    mainMenu.Location = new Point(0,0);
    mainMenu.Name = "mainMenu";
    mainMenu.Size = new Size(1340,24);
    mainMenu.TabIndex = 106;
    // 
    // mnuFile
    // 
    mnuFile.DropDownItems.AddRange(new ToolStripItem[] { mnuNewProfile,mnuNewProfileFromCsproj,mnuLoadProfile,mnuSaveProfile,mnuSaveProfileAs,toolStripSeparator4,mnuUpdateCsprojFromProfile,toolStripSeparator1,mnuPrintSelectedView,mnuEditSelectedViewNoteapad,toolStripSeparator2,mnuExit });
    mnuFile.Name = "mnuFile";
    mnuFile.Size = new Size(37,20);
    mnuFile.Text = "File";
    // 
    // mnuNewProfile
    // 
    mnuNewProfile.Name = "mnuNewProfile";
    mnuNewProfile.Size = new Size(257,22);
    mnuNewProfile.Text = "New profile";
    mnuNewProfile.Click += btnNewProfile_Click;
    // 
    // mnuNewProfileFromCsproj
    // 
    mnuNewProfileFromCsproj.Name = "mnuNewProfileFromCsproj";
    mnuNewProfileFromCsproj.Size = new Size(257,22);
    mnuNewProfileFromCsproj.Text = "New profile from .csproj";
    mnuNewProfileFromCsproj.Click += mnuNewProfileFromCsproj_Click;
    // 
    // mnuLoadProfile
    // 
    mnuLoadProfile.Name = "mnuLoadProfile";
    mnuLoadProfile.Size = new Size(257,22);
    mnuLoadProfile.Text = "Load profile";
    mnuLoadProfile.Click += btnLoadProfile_Click;
    // 
    // mnuSaveProfile
    // 
    mnuSaveProfile.Name = "mnuSaveProfile";
    mnuSaveProfile.Size = new Size(257,22);
    mnuSaveProfile.Text = "Save profile";
    mnuSaveProfile.Click += btnSaveProfile_Click;
    // 
    // mnuSaveProfileAs
    // 
    mnuSaveProfileAs.Name = "mnuSaveProfileAs";
    mnuSaveProfileAs.Size = new Size(257,22);
    mnuSaveProfileAs.Text = "Save profile as ...";
    mnuSaveProfileAs.Click += btnSaveProfileAs_Click;
    // 
    // toolStripSeparator4
    // 
    toolStripSeparator4.Name = "toolStripSeparator4";
    toolStripSeparator4.Size = new Size(254,6);
    // 
    // mnuUpdateCsprojFromProfile
    // 
    mnuUpdateCsprojFromProfile.Name = "mnuUpdateCsprojFromProfile";
    mnuUpdateCsprojFromProfile.Size = new Size(257,22);
    mnuUpdateCsprojFromProfile.Text = "Update .csproj from current profile";
    mnuUpdateCsprojFromProfile.Click += mnuUpdateCsprojFromProfile_Click;
    // 
    // toolStripSeparator1
    // 
    toolStripSeparator1.Name = "toolStripSeparator1";
    toolStripSeparator1.Size = new Size(254,6);
    // 
    // mnuPrintSelectedView
    // 
    mnuPrintSelectedView.Name = "mnuPrintSelectedView";
    mnuPrintSelectedView.Size = new Size(257,22);
    mnuPrintSelectedView.Text = "Print selected view";
    mnuPrintSelectedView.Click += mnuPrintSelectedView_Click;
    // 
    // mnuEditSelectedViewNoteapad
    // 
    mnuEditSelectedViewNoteapad.Name = "mnuEditSelectedViewNoteapad";
    mnuEditSelectedViewNoteapad.Size = new Size(257,22);
    mnuEditSelectedViewNoteapad.Text = "Edit selected view";
    mnuEditSelectedViewNoteapad.Click += mnuEditSelectedViewNoteapad_Click;
    // 
    // toolStripSeparator2
    // 
    toolStripSeparator2.Name = "toolStripSeparator2";
    toolStripSeparator2.Size = new Size(254,6);
    // 
    // mnuExit
    // 
    mnuExit.Name = "mnuExit";
    mnuExit.Size = new Size(257,22);
    mnuExit.Text = "Exit";
    mnuExit.Click += mnuExit_Click;
    // 
    // mnuBuild
    // 
    mnuBuild.DropDownItems.AddRange(new ToolStripItem[] { mnuGenerateWix,mnuBuildMsi,toolStripSeparator3,mnuGenerateWebSiteOnly,mnuUploadWebSiteOnly,toolStripSeparator8,mnuReleaseChecklist,mnuReleaseSummary });
    mnuBuild.Name = "mnuBuild";
    mnuBuild.Size = new Size(46,20);
    mnuBuild.Text = "Build";
    // 
    // mnuGenerateWix
    // 
    mnuGenerateWix.Name = "mnuGenerateWix";
    mnuGenerateWix.Size = new Size(193,22);
    mnuGenerateWix.Text = "Generate Wix";
    mnuGenerateWix.Click += btnGenerateWix_Click;
    // 
    // mnuBuildMsi
    // 
    mnuBuildMsi.Name = "mnuBuildMsi";
    mnuBuildMsi.Size = new Size(193,22);
    mnuBuildMsi.Text = "Build MSI";
    mnuBuildMsi.Click += btnBuildMsi_Click;
    // 
    // toolStripSeparator3
    // 
    toolStripSeparator3.Name = "toolStripSeparator3";
    toolStripSeparator3.Size = new Size(190,6);
    // 
    // mnuGenerateWebSiteOnly
    // 
    mnuGenerateWebSiteOnly.Name = "mnuGenerateWebSiteOnly";
    mnuGenerateWebSiteOnly.Size = new Size(193,22);
    mnuGenerateWebSiteOnly.Text = "Generate web site only";
    mnuGenerateWebSiteOnly.Click += mnuGenerateWebSiteOnly_Click;
    // 
    // mnuUploadWebSiteOnly
    // 
    mnuUploadWebSiteOnly.Name = "mnuUploadWebSiteOnly";
    mnuUploadWebSiteOnly.Size = new Size(193,22);
    mnuUploadWebSiteOnly.Text = "Upload web site only";
    mnuUploadWebSiteOnly.Click += mnuUploadWebSiteOnly_Click;
    // 
    // toolStripSeparator8
    // 
    toolStripSeparator8.Name = "toolStripSeparator8";
    toolStripSeparator8.Size = new Size(190,6);
    // 
    // mnuReleaseChecklist
    // 
    mnuReleaseChecklist.Name = "mnuReleaseChecklist";
    mnuReleaseChecklist.Size = new Size(193,22);
    mnuReleaseChecklist.Text = "Release Checklist";
    mnuReleaseChecklist.Click += mnuReleaseChecklist_Click;
    // 
    // mnuReleaseSummary
    // 
    mnuReleaseSummary.Name = "mnuReleaseSummary";
    mnuReleaseSummary.Size = new Size(193,22);
    mnuReleaseSummary.Text = "Release summary";
    mnuReleaseSummary.Click += mnuReleaseSummary_Click;
    // 
    // mnuTools
    // 
    mnuTools.DropDownItems.AddRange(new ToolStripItem[] { mnuValidateProfile,mnuRestoreDefaultWinScpPath,mnuSaveUploadCredential,toolStripSeparator5,mnuCodeSigningSettings,toolStripSeparator6,mnuOpenPackagerFolder,mnuOpenLocalSiteWeb,mnuOpenBuildReportsFolder,mnuOpenBuildArchivesFolder,mnuRefreshBuildHistory,toolStripSeparator7,mnuEnvironmentCheck,mnuGlobalSettings,mnuOpenGlobalSettingsFile,mnuOpenGlobalSettingsFolder });
    mnuTools.Name = "mnuTools";
    mnuTools.Size = new Size(47,20);
    mnuTools.Text = "Tools";
    // 
    // mnuValidateProfile
    // 
    mnuValidateProfile.Name = "mnuValidateProfile";
    mnuValidateProfile.Size = new Size(364,22);
    mnuValidateProfile.Text = "Validate package profile";
    mnuValidateProfile.Click += mnuValidateProfile_Click;
    // 
    // mnuRestoreDefaultWinScpPath
    // 
    mnuRestoreDefaultWinScpPath.Name = "mnuRestoreDefaultWinScpPath";
    mnuRestoreDefaultWinScpPath.Size = new Size(364,22);
    mnuRestoreDefaultWinScpPath.Text = "Restaure default WinSCP path";
    mnuRestoreDefaultWinScpPath.Click += mnuRestoreDefaultWinScpPath_Click;
    // 
    // mnuSaveUploadCredential
    // 
    mnuSaveUploadCredential.Name = "mnuSaveUploadCredential";
    mnuSaveUploadCredential.Size = new Size(364,22);
    mnuSaveUploadCredential.Text = "Save upload password to Windows Credential Manager";
    mnuSaveUploadCredential.Click += mnuSaveUploadCredential_Click;
    // 
    // toolStripSeparator5
    // 
    toolStripSeparator5.Name = "toolStripSeparator5";
    toolStripSeparator5.Size = new Size(361,6);
    // 
    // mnuCodeSigningSettings
    // 
    mnuCodeSigningSettings.Name = "mnuCodeSigningSettings";
    mnuCodeSigningSettings.Size = new Size(364,22);
    mnuCodeSigningSettings.Text = "Code signing settings...";
    mnuCodeSigningSettings.Click += mnuCodeSigningSettings_Click;
    // 
    // toolStripSeparator6
    // 
    toolStripSeparator6.Name = "toolStripSeparator6";
    toolStripSeparator6.Size = new Size(361,6);
    // 
    // mnuOpenPackagerFolder
    // 
    mnuOpenPackagerFolder.Name = "mnuOpenPackagerFolder";
    mnuOpenPackagerFolder.Size = new Size(364,22);
    mnuOpenPackagerFolder.Text = "Open Packager Folder";
    mnuOpenPackagerFolder.Click += btnOpenMsiFolder_Click;
    // 
    // mnuOpenLocalSiteWeb
    // 
    mnuOpenLocalSiteWeb.Name = "mnuOpenLocalSiteWeb";
    mnuOpenLocalSiteWeb.Size = new Size(364,22);
    mnuOpenLocalSiteWeb.Text = "Open local web site folder";
    mnuOpenLocalSiteWeb.Click += mnuOpenLocalSiteWeb_Click;
    // 
    // mnuOpenBuildReportsFolder
    // 
    mnuOpenBuildReportsFolder.Name = "mnuOpenBuildReportsFolder";
    mnuOpenBuildReportsFolder.Size = new Size(364,22);
    mnuOpenBuildReportsFolder.Text = "Open build reports folder";
    // 
    // mnuOpenBuildArchivesFolder
    // 
    mnuOpenBuildArchivesFolder.Name = "mnuOpenBuildArchivesFolder";
    mnuOpenBuildArchivesFolder.Size = new Size(364,22);
    mnuOpenBuildArchivesFolder.Text = "Open build  archives folder";
    mnuOpenBuildArchivesFolder.Click += mnuOpenBuildArchiveFolder_Click;
    // 
    // mnuRefreshBuildHistory
    // 
    mnuRefreshBuildHistory.Name = "mnuRefreshBuildHistory";
    mnuRefreshBuildHistory.Size = new Size(364,22);
    mnuRefreshBuildHistory.Text = "Refresh build history";
    mnuRefreshBuildHistory.Click += mnuRefreshBuildHistory_Click;
    // 
    // toolStripSeparator7
    // 
    toolStripSeparator7.Name = "toolStripSeparator7";
    toolStripSeparator7.Size = new Size(361,6);
    // 
    // mnuEnvironmentCheck
    // 
    mnuEnvironmentCheck.Name = "mnuEnvironmentCheck";
    mnuEnvironmentCheck.Size = new Size(364,22);
    mnuEnvironmentCheck.Text = "Environment check";
    mnuEnvironmentCheck.Click += mnuEnvironmentCheck_Click;
    // 
    // mnuGlobalSettings
    // 
    mnuGlobalSettings.Name = "mnuGlobalSettings";
    mnuGlobalSettings.Size = new Size(364,22);
    mnuGlobalSettings.Text = "Global settings...";
    mnuGlobalSettings.Click += mnuGlobalSettings_Click;
    // 
    // mnuOpenGlobalSettingsFile
    // 
    mnuOpenGlobalSettingsFile.Name = "mnuOpenGlobalSettingsFile";
    mnuOpenGlobalSettingsFile.Size = new Size(364,22);
    mnuOpenGlobalSettingsFile.Text = "Open global settings file";
    mnuOpenGlobalSettingsFile.Click += mnuOpenGlobalSettingsFile_Click;
    // 
    // mnuOpenGlobalSettingsFolder
    // 
    mnuOpenGlobalSettingsFolder.Name = "mnuOpenGlobalSettingsFolder";
    mnuOpenGlobalSettingsFolder.Size = new Size(364,22);
    mnuOpenGlobalSettingsFolder.Text = "Open global settings folder";
    mnuOpenGlobalSettingsFolder.Click += mnuOpenGlobalSettingsFolder_Click;
    // 
    // mnuHelp
    // 
    mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { mnuCheckForUpdates,mnuAbout,mnuLicense,mnuImportLicense });
    mnuHelp.Name = "mnuHelp";
    mnuHelp.Size = new Size(44,20);
    mnuHelp.Text = "Help";
    // 
    // mnuCheckForUpdates
    // 
    mnuCheckForUpdates.Name = "mnuCheckForUpdates";
    mnuCheckForUpdates.Size = new Size(229,22);
    mnuCheckForUpdates.Text = "Check for update";
    mnuCheckForUpdates.Click += mnuCheckForUpdates_Click;
    // 
    // mnuAbout
    // 
    mnuAbout.Name = "mnuAbout";
    mnuAbout.Size = new Size(229,22);
    mnuAbout.Text = "About Msi Software Packager";
    mnuAbout.Click += mnuAbout_Click;
    // 
    // mnuLicense
    // 
    mnuLicense.Name = "mnuLicense";
    mnuLicense.Size = new Size(229,22);
    mnuLicense.Text = "License...";
    mnuLicense.Click += mnuLicense_Click;
    // 
    // mnuImportLicense
    // 
    mnuImportLicense.Name = "mnuImportLicense";
    mnuImportLicense.Size = new Size(229,22);
    mnuImportLicense.Text = "Import new license";
    mnuImportLicense.Click += mnuImportLicense_Click;
    // 
    // mnuUploadNow
    // 
    mnuUploadNow.Name = "mnuUploadNow";
    mnuUploadNow.Size = new Size(231,22);
    mnuUploadNow.Text = "Upload now";
    mnuUploadNow.Click += btnUploadNow_Click;
    // 
    // btnOpenPackagerFolder
    // 
    btnOpenPackagerFolder.AutoSize = true;
    btnOpenPackagerFolder.Location = new Point(756,445);
    btnOpenPackagerFolder.Margin = new Padding(0);
    btnOpenPackagerFolder.Name = "btnOpenPackagerFolder";
    btnOpenPackagerFolder.Size = new Size(139,30);
    btnOpenPackagerFolder.TabIndex = 99;
    btnOpenPackagerFolder.Text = "Open Packager Folder";
    btnOpenPackagerFolder.UseVisualStyleBackColor = true;
    btnOpenPackagerFolder.Click += btnOpenMsiFolder_Click;
    // 
    // lblIconPath
    // 
    lblIconPath.AutoSize = true;
    lblIconPath.Location = new Point(35,154);
    lblIconPath.Name = "lblIconPath";
    lblIconPath.Size = new Size(49,15);
    lblIconPath.TabIndex = 8;
    lblIconPath.Text = "Icon file";
    // 
    // txtIconPath
    // 
    txtIconPath.Location = new Point(124,151);
    txtIconPath.Name = "txtIconPath";
    txtIconPath.Size = new Size(581,23);
    txtIconPath.TabIndex = 9;
    // 
    // btnBrowseIconPath
    // 
    btnBrowseIconPath.Location = new Point(711,150);
    btnBrowseIconPath.Name = "btnBrowseIconPath";
    btnBrowseIconPath.Size = new Size(24,23);
    btnBrowseIconPath.TabIndex = 10;
    btnBrowseIconPath.Text = "...";
    btnBrowseIconPath.UseVisualStyleBackColor = true;
    btnBrowseIconPath.Click += btnBrowseIconPath_Click;
    // 
    // btnBrowseProjectFile
    // 
    btnBrowseProjectFile.Location = new Point(711,118);
    btnBrowseProjectFile.Name = "btnBrowseProjectFile";
    btnBrowseProjectFile.Size = new Size(24,23);
    btnBrowseProjectFile.TabIndex = 23;
    btnBrowseProjectFile.Text = "...";
    btnBrowseProjectFile.UseVisualStyleBackColor = true;
    btnBrowseProjectFile.Click += btnBrowseProjectFile_Click;
    // 
    // btnBrowsePublishDir
    // 
    btnBrowsePublishDir.Location = new Point(711,253);
    btnBrowsePublishDir.Name = "btnBrowsePublishDir";
    btnBrowsePublishDir.Size = new Size(24,23);
    btnBrowsePublishDir.TabIndex = 24;
    btnBrowsePublishDir.Text = "...";
    btnBrowsePublishDir.UseVisualStyleBackColor = true;
    btnBrowsePublishDir.Click += btnBrowsePublishDir_Click;
    // 
    // btnBrowseMsiOutputDir
    // 
    btnBrowseMsiOutputDir.Location = new Point(711,285);
    btnBrowseMsiOutputDir.Name = "btnBrowseMsiOutputDir";
    btnBrowseMsiOutputDir.Size = new Size(24,23);
    btnBrowseMsiOutputDir.TabIndex = 25;
    btnBrowseMsiOutputDir.Text = "...";
    btnBrowseMsiOutputDir.UseVisualStyleBackColor = true;
    btnBrowseMsiOutputDir.Click += btnBrowseMsiOutputDir_Click;
    // 
    // lblProductName
    // 
    lblProductName.AutoSize = true;
    lblProductName.Location = new Point(35,25);
    lblProductName.Name = "lblProductName";
    lblProductName.Size = new Size(82,15);
    lblProductName.TabIndex = 0;
    lblProductName.Text = "Product name";
    // 
    // lblManufacturer
    // 
    lblManufacturer.AutoSize = true;
    lblManufacturer.Location = new Point(35,58);
    lblManufacturer.Name = "lblManufacturer";
    lblManufacturer.Size = new Size(79,15);
    lblManufacturer.TabIndex = 2;
    lblManufacturer.Text = "Manufacturer";
    // 
    // lblVersion
    // 
    lblVersion.AutoSize = true;
    lblVersion.Location = new Point(35,90);
    lblVersion.Name = "lblVersion";
    lblVersion.Size = new Size(45,15);
    lblVersion.TabIndex = 4;
    lblVersion.Text = "Version";
    // 
    // lblProjectFile
    // 
    lblProjectFile.AutoSize = true;
    lblProjectFile.Location = new Point(35,122);
    lblProjectFile.Name = "lblProjectFile";
    lblProjectFile.Size = new Size(63,15);
    lblProjectFile.TabIndex = 6;
    lblProjectFile.Text = "Project file";
    // 
    // lblExecutableName
    // 
    lblExecutableName.AutoSize = true;
    lblExecutableName.Location = new Point(35,187);
    lblExecutableName.Name = "lblExecutableName";
    lblExecutableName.Size = new Size(63,15);
    lblExecutableName.TabIndex = 8;
    lblExecutableName.Text = "Executable";
    // 
    // lblPublishDir
    // 
    lblPublishDir.AutoSize = true;
    lblPublishDir.Location = new Point(35,257);
    lblPublishDir.Name = "lblPublishDir";
    lblPublishDir.Size = new Size(63,15);
    lblPublishDir.TabIndex = 10;
    lblPublishDir.Text = "Publish dir";
    // 
    // lblMsiOutputDir
    // 
    lblMsiOutputDir.AutoSize = true;
    lblMsiOutputDir.Location = new Point(35,289);
    lblMsiOutputDir.Name = "lblMsiOutputDir";
    lblMsiOutputDir.Size = new Size(83,15);
    lblMsiOutputDir.TabIndex = 12;
    lblMsiOutputDir.Text = "MSI output dir";
    // 
    // txtProductName
    // 
    txtProductName.Location = new Point(124,22);
    txtProductName.Name = "txtProductName";
    txtProductName.Size = new Size(198,23);
    txtProductName.TabIndex = 1;
    // 
    // txtManufacturer
    // 
    txtManufacturer.Location = new Point(124,54);
    txtManufacturer.Name = "txtManufacturer";
    txtManufacturer.Size = new Size(198,23);
    txtManufacturer.TabIndex = 3;
    // 
    // txtVersion
    // 
    txtVersion.Location = new Point(124,86);
    txtVersion.Name = "txtVersion";
    txtVersion.Size = new Size(147,23);
    txtVersion.TabIndex = 5;
    // 
    // txtProjectFile
    // 
    txtProjectFile.Location = new Point(124,118);
    txtProjectFile.Name = "txtProjectFile";
    txtProjectFile.Size = new Size(581,23);
    txtProjectFile.TabIndex = 7;
    // 
    // txtExecutableName
    // 
    txtExecutableName.Location = new Point(123,184);
    txtExecutableName.Name = "txtExecutableName";
    txtExecutableName.Size = new Size(198,23);
    txtExecutableName.TabIndex = 9;
    // 
    // txtPublishDir
    // 
    txtPublishDir.Location = new Point(124,253);
    txtPublishDir.Name = "txtPublishDir";
    txtPublishDir.Size = new Size(581,23);
    txtPublishDir.TabIndex = 11;
    // 
    // txtMsiOutputDir
    // 
    txtMsiOutputDir.Location = new Point(124,285);
    txtMsiOutputDir.Name = "txtMsiOutputDir";
    txtMsiOutputDir.Size = new Size(581,23);
    txtMsiOutputDir.TabIndex = 13;
    // 
    // chkDesktopShortcut
    // 
    chkDesktopShortcut.AutoSize = true;
    chkDesktopShortcut.Location = new Point(55,43);
    chkDesktopShortcut.Name = "chkDesktopShortcut";
    chkDesktopShortcut.Size = new Size(116,19);
    chkDesktopShortcut.TabIndex = 14;
    chkDesktopShortcut.Text = "Desktop shortcut";
    chkDesktopShortcut.UseVisualStyleBackColor = true;
    // 
    // chkStartMenuShortcut
    // 
    chkStartMenuShortcut.AutoSize = true;
    chkStartMenuShortcut.Location = new Point(55,18);
    chkStartMenuShortcut.Name = "chkStartMenuShortcut";
    chkStartMenuShortcut.Size = new Size(131,19);
    chkStartMenuShortcut.TabIndex = 15;
    chkStartMenuShortcut.Text = "Start menu shortcut";
    chkStartMenuShortcut.UseVisualStyleBackColor = true;
    // 
    // chkAddToPath
    // 
    chkAddToPath.AutoSize = true;
    chkAddToPath.Location = new Point(244,18);
    chkAddToPath.Name = "chkAddToPath";
    chkAddToPath.Size = new Size(162,19);
    chkAddToPath.TabIndex = 16;
    chkAddToPath.Text = "Add install folder to PATH";
    chkAddToPath.UseVisualStyleBackColor = true;
    // 
    // btnLoadProfile
    // 
    btnLoadProfile.Location = new Point(115,443);
    btnLoadProfile.Name = "btnLoadProfile";
    btnLoadProfile.Size = new Size(90,30);
    btnLoadProfile.TabIndex = 17;
    btnLoadProfile.Text = "Load profile";
    btnLoadProfile.UseVisualStyleBackColor = true;
    btnLoadProfile.Click += btnLoadProfile_Click;
    // 
    // btnSaveProfile
    // 
    btnSaveProfile.Location = new Point(211,443);
    btnSaveProfile.Name = "btnSaveProfile";
    btnSaveProfile.Size = new Size(90,30);
    btnSaveProfile.TabIndex = 18;
    btnSaveProfile.Text = "Save profile";
    btnSaveProfile.UseVisualStyleBackColor = true;
    btnSaveProfile.Click += btnSaveProfile_Click;
    // 
    // btnGenerateWix
    // 
    btnGenerateWix.Location = new Point(567,445);
    btnGenerateWix.Name = "btnGenerateWix";
    btnGenerateWix.Size = new Size(90,30);
    btnGenerateWix.TabIndex = 19;
    btnGenerateWix.Text = "Generate WiX";
    btnGenerateWix.UseVisualStyleBackColor = true;
    btnGenerateWix.Click += btnGenerateWix_Click;
    // 
    // btnBuildMsi
    // 
    btnBuildMsi.Location = new Point(663,445);
    btnBuildMsi.Name = "btnBuildMsi";
    btnBuildMsi.Size = new Size(90,30);
    btnBuildMsi.TabIndex = 20;
    btnBuildMsi.Text = "Build MSI";
    btnBuildMsi.UseVisualStyleBackColor = true;
    btnBuildMsi.Click += btnBuildMsi_Click;
    // 
    // tabPreview
    // 
    tabPreview.Location = new Point(12,478);
    tabPreview.Name = "tabPreview";
    tabPreview.SelectedIndex = 0;
    tabPreview.Size = new Size(883,237);
    tabPreview.TabIndex = 21;
    // 
    // txtUpgradeCode
    // 
    txtUpgradeCode.Location = new Point(493,25);
    txtUpgradeCode.Name = "txtUpgradeCode";
    txtUpgradeCode.Size = new Size(242,23);
    txtUpgradeCode.TabIndex = 5;
    // 
    // btnGenerateUpgradeCode
    // 
    btnGenerateUpgradeCode.Location = new Point(360,25);
    btnGenerateUpgradeCode.Name = "btnGenerateUpgradeCode";
    btnGenerateUpgradeCode.Size = new Size(127,23);
    btnGenerateUpgradeCode.TabIndex = 23;
    btnGenerateUpgradeCode.Text = "Générer update code";
    btnGenerateUpgradeCode.UseVisualStyleBackColor = true;
    btnGenerateUpgradeCode.Click += btnGenerateUpgradeCode_Click;
    // 
    // btnBrowseWixOutputDir
    // 
    btnBrowseWixOutputDir.Location = new Point(711,220);
    btnBrowseWixOutputDir.Name = "btnBrowseWixOutputDir";
    btnBrowseWixOutputDir.Size = new Size(24,23);
    btnBrowseWixOutputDir.TabIndex = 24;
    btnBrowseWixOutputDir.Text = "...";
    btnBrowseWixOutputDir.UseVisualStyleBackColor = true;
    btnBrowseWixOutputDir.Click += btnBrowseWixOutputDir_Click;
    // 
    // txtWixOutputDir
    // 
    txtWixOutputDir.Location = new Point(124,220);
    txtWixOutputDir.Name = "txtWixOutputDir";
    txtWixOutputDir.Size = new Size(581,23);
    txtWixOutputDir.TabIndex = 11;
    // 
    // label1
    // 
    label1.AutoSize = true;
    label1.Location = new Point(35,224);
    label1.Name = "label1";
    label1.Size = new Size(82,15);
    label1.TabIndex = 10;
    label1.Text = "Wix output dir";
    // 
    // chkRequireAdministrator
    // 
    chkRequireAdministrator.AutoSize = true;
    chkRequireAdministrator.Location = new Point(244,43);
    chkRequireAdministrator.Name = "chkRequireAdministrator";
    chkRequireAdministrator.Size = new Size(197,19);
    chkRequireAdministrator.TabIndex = 100;
    chkRequireAdministrator.Text = "Run application as administrator";
    chkRequireAdministrator.UseVisualStyleBackColor = true;
    // 
    // btnVersionPatch
    // 
    btnVersionPatch.Location = new Point(326,85);
    btnVersionPatch.Name = "btnVersionPatch";
    btnVersionPatch.Size = new Size(66,23);
    btnVersionPatch.TabIndex = 20;
    btnVersionPatch.Text = "+ Path";
    btnVersionPatch.UseVisualStyleBackColor = true;
    btnVersionPatch.Click += btnVersionPatch_Click;
    // 
    // btnVersionMinor
    // 
    btnVersionMinor.Location = new Point(402,85);
    btnVersionMinor.Name = "btnVersionMinor";
    btnVersionMinor.Size = new Size(66,23);
    btnVersionMinor.TabIndex = 20;
    btnVersionMinor.Text = "+Minor";
    btnVersionMinor.UseVisualStyleBackColor = true;
    btnVersionMinor.Click += btnVersionMinor_Click;
    // 
    // btnVersionMajor
    // 
    btnVersionMajor.Location = new Point(478,85);
    btnVersionMajor.Name = "btnVersionMajor";
    btnVersionMajor.Size = new Size(66,23);
    btnVersionMajor.TabIndex = 20;
    btnVersionMajor.Text = "+ Major";
    btnVersionMajor.UseVisualStyleBackColor = true;
    btnVersionMajor.Click += btnVersionMajor_Click;
    // 
    // chkBuildBundle
    // 
    chkBuildBundle.AutoSize = true;
    chkBuildBundle.Location = new Point(128,12);
    chkBuildBundle.Name = "chkBuildBundle";
    chkBuildBundle.Size = new Size(145,19);
    chkBuildBundle.TabIndex = 101;
    chkBuildBundle.Text = "Build setup.exe bundle";
    chkBuildBundle.UseVisualStyleBackColor = true;
    // 
    // txtBundleOutputDir
    // 
    txtBundleOutputDir.Location = new Point(161,85);
    txtBundleOutputDir.Name = "txtBundleOutputDir";
    txtBundleOutputDir.Size = new Size(235,23);
    txtBundleOutputDir.TabIndex = 5;
    // 
    // lblBundleOutputDir
    // 
    lblBundleOutputDir.AutoSize = true;
    lblBundleOutputDir.Location = new Point(20,87);
    lblBundleOutputDir.Name = "lblBundleOutputDir";
    lblBundleOutputDir.Size = new Size(100,15);
    lblBundleOutputDir.TabIndex = 102;
    lblBundleOutputDir.Text = "Bundle output dir";
    // 
    // btnBrowseBundleDir
    // 
    btnBrowseBundleDir.Location = new Point(402,83);
    btnBrowseBundleDir.Name = "btnBrowseBundleDir";
    btnBrowseBundleDir.Size = new Size(24,23);
    btnBrowseBundleDir.TabIndex = 23;
    btnBrowseBundleDir.Text = "...";
    btnBrowseBundleDir.UseVisualStyleBackColor = true;
    btnBrowseBundleDir.Click += btnBrowseBundleDir_Click;
    // 
    // lblBundleFileName
    // 
    lblBundleFileName.AutoSize = true;
    lblBundleFileName.Location = new Point(20,124);
    lblBundleFileName.Name = "lblBundleFileName";
    lblBundleFileName.Size = new Size(96,15);
    lblBundleFileName.TabIndex = 102;
    lblBundleFileName.Text = "Bundle file name";
    // 
    // txtBundleFileName
    // 
    txtBundleFileName.Location = new Point(161,123);
    txtBundleFileName.Name = "txtBundleFileName";
    txtBundleFileName.Size = new Size(265,23);
    txtBundleFileName.TabIndex = 5;
    // 
    // txtBundleUpgradeCode
    // 
    txtBundleUpgradeCode.Location = new Point(161,161);
    txtBundleUpgradeCode.Name = "txtBundleUpgradeCode";
    txtBundleUpgradeCode.Size = new Size(265,23);
    txtBundleUpgradeCode.TabIndex = 5;
    // 
    // grpbxFeature
    // 
    grpbxFeature.Controls.Add(lblProductName);
    grpbxFeature.Controls.Add(txtProductName);
    grpbxFeature.Controls.Add(lblManufacturer);
    grpbxFeature.Controls.Add(txtManufacturer);
    grpbxFeature.Controls.Add(lblVersion);
    grpbxFeature.Controls.Add(txtUpgradeCode);
    grpbxFeature.Controls.Add(txtVersion);
    grpbxFeature.Controls.Add(lblProjectFile);
    grpbxFeature.Controls.Add(txtProjectFile);
    grpbxFeature.Controls.Add(lblExecutableName);
    grpbxFeature.Controls.Add(txtExecutableName);
    grpbxFeature.Controls.Add(label1);
    grpbxFeature.Controls.Add(lblPublishDir);
    grpbxFeature.Controls.Add(txtWixOutputDir);
    grpbxFeature.Controls.Add(txtPublishDir);
    grpbxFeature.Controls.Add(lblMsiOutputDir);
    grpbxFeature.Controls.Add(txtMsiOutputDir);
    grpbxFeature.Controls.Add(btnVersionMajor);
    grpbxFeature.Controls.Add(btnVersionMinor);
    grpbxFeature.Controls.Add(btnVersionPatch);
    grpbxFeature.Controls.Add(btnGenerateUpgradeCode);
    grpbxFeature.Controls.Add(btnBrowseWixOutputDir);
    grpbxFeature.Controls.Add(btnBrowseProjectFile);
    grpbxFeature.Controls.Add(btnBrowsePublishDir);
    grpbxFeature.Controls.Add(btnBrowseMsiOutputDir);
    grpbxFeature.Controls.Add(lblIconPath);
    grpbxFeature.Controls.Add(txtIconPath);
    grpbxFeature.Controls.Add(btnBrowseIconPath);
    grpbxFeature.Location = new Point(12,32);
    grpbxFeature.Name = "grpbxFeature";
    grpbxFeature.Size = new Size(765,321);
    grpbxFeature.TabIndex = 103;
    grpbxFeature.TabStop = false;
    grpbxFeature.Text = "Feature";
    // 
    // grpbxBundle
    // 
    grpbxBundle.Controls.Add(lblBundleName);
    grpbxBundle.Controls.Add(lblBundleFileName);
    grpbxBundle.Controls.Add(lblBundleOutputDir);
    grpbxBundle.Controls.Add(chkBuildBundle);
    grpbxBundle.Controls.Add(txtBundleUpgradeCode);
    grpbxBundle.Controls.Add(txtBundleName);
    grpbxBundle.Controls.Add(txtBundleFileName);
    grpbxBundle.Controls.Add(txtBundleOutputDir);
    grpbxBundle.Controls.Add(btnBrowseBundleDir);
    grpbxBundle.Controls.Add(btnGenerateBundleUpgradeCode);
    grpbxBundle.Location = new Point(902,32);
    grpbxBundle.Name = "grpbxBundle";
    grpbxBundle.Size = new Size(430,203);
    grpbxBundle.TabIndex = 104;
    grpbxBundle.TabStop = false;
    grpbxBundle.Text = "Setup.exe Bundle";
    // 
    // lblBundleName
    // 
    lblBundleName.AutoSize = true;
    lblBundleName.Location = new Point(20,50);
    lblBundleName.Name = "lblBundleName";
    lblBundleName.Size = new Size(77,15);
    lblBundleName.TabIndex = 102;
    lblBundleName.Text = "Bundle name";
    // 
    // txtBundleName
    // 
    txtBundleName.Location = new Point(161,47);
    txtBundleName.Name = "txtBundleName";
    txtBundleName.Size = new Size(265,23);
    txtBundleName.TabIndex = 5;
    // 
    // btnGenerateBundleUpgradeCode
    // 
    btnGenerateBundleUpgradeCode.AutoSize = true;
    btnGenerateBundleUpgradeCode.Location = new Point(12,156);
    btnGenerateBundleUpgradeCode.Margin = new Padding(0);
    btnGenerateBundleUpgradeCode.Name = "btnGenerateBundleUpgradeCode";
    btnGenerateBundleUpgradeCode.Size = new Size(137,31);
    btnGenerateBundleUpgradeCode.TabIndex = 99;
    btnGenerateBundleUpgradeCode.Text = "Bundle upgrade code";
    btnGenerateBundleUpgradeCode.TextAlign = ContentAlignment.MiddleLeft;
    btnGenerateBundleUpgradeCode.UseVisualStyleBackColor = true;
    btnGenerateBundleUpgradeCode.Click += btnGenerateBundleUpgradeCode_Click;
    // 
    // grpbxMisOptions
    // 
    grpbxMisOptions.Controls.Add(chkTechnicalLicenseRequired);
    grpbxMisOptions.Controls.Add(chkRequireLicenseAcceptance);
    grpbxMisOptions.Controls.Add(chkRequireAdministrator);
    grpbxMisOptions.Controls.Add(chkDesktopShortcut);
    grpbxMisOptions.Controls.Add(chkStartMenuShortcut);
    grpbxMisOptions.Controls.Add(chkAddToPath);
    grpbxMisOptions.Location = new Point(12,359);
    grpbxMisOptions.Name = "grpbxMisOptions";
    grpbxMisOptions.Size = new Size(765,68);
    grpbxMisOptions.TabIndex = 105;
    grpbxMisOptions.TabStop = false;
    grpbxMisOptions.Text = "MSI options";
    // 
    // chkTechnicalLicenseRequired
    // 
    chkTechnicalLicenseRequired.AutoSize = true;
    chkTechnicalLicenseRequired.Location = new Point(503,43);
    chkTechnicalLicenseRequired.Name = "chkTechnicalLicenseRequired";
    chkTechnicalLicenseRequired.Size = new Size(251,19);
    chkTechnicalLicenseRequired.TabIndex = 101;
    chkTechnicalLicenseRequired.Text = "Exiger une licence technique au lancement";
    chkTechnicalLicenseRequired.UseVisualStyleBackColor = true;
    // 
    // chkRequireLicenseAcceptance
    // 
    chkRequireLicenseAcceptance.AutoSize = true;
    chkRequireLicenseAcceptance.Location = new Point(503,18);
    chkRequireLicenseAcceptance.Name = "chkRequireLicenseAcceptance";
    chkRequireLicenseAcceptance.Size = new Size(220,19);
    chkRequireLicenseAcceptance.TabIndex = 101;
    chkRequireLicenseAcceptance.Text = "Demander l'acceptation de la licence";
    chkRequireLicenseAcceptance.UseVisualStyleBackColor = true;
    // 
    // grpWebInstaller
    // 
    grpWebInstaller.Controls.Add(lblWebBundleName);
    grpWebInstaller.Controls.Add(lblWebPublishDir);
    grpWebInstaller.Controls.Add(lblMsiDownloadUrl);
    grpWebInstaller.Controls.Add(lblWebPoductFolder);
    grpWebInstaller.Controls.Add(lblWebSetupFileName);
    grpWebInstaller.Controls.Add(lblWebOutputDir);
    grpWebInstaller.Controls.Add(chkBuildWebInstaller);
    grpWebInstaller.Controls.Add(txtWebBundleUpgradeCode);
    grpWebInstaller.Controls.Add(txtWebPublishDirectory);
    grpWebInstaller.Controls.Add(txtWebBundleName);
    grpWebInstaller.Controls.Add(txtMsiDownloadUrl);
    grpWebInstaller.Controls.Add(txtWebProductFolder);
    grpWebInstaller.Controls.Add(txtWebSetupFileName);
    grpWebInstaller.Controls.Add(txtWebOutputDir);
    grpWebInstaller.Controls.Add(btnWebPublisDir);
    grpWebInstaller.Controls.Add(btnBrowseWebOutputDir);
    grpWebInstaller.Controls.Add(btnGenerateWebBundleUpgradeCode);
    grpWebInstaller.Location = new Point(902,253);
    grpWebInstaller.Name = "grpWebInstaller";
    grpWebInstaller.Size = new Size(430,260);
    grpWebInstaller.TabIndex = 104;
    grpWebInstaller.TabStop = false;
    grpWebInstaller.Text = "Web Installer";
    // 
    // lblWebBundleName
    // 
    lblWebBundleName.AutoSize = true;
    lblWebBundleName.Location = new Point(20,48);
    lblWebBundleName.Name = "lblWebBundleName";
    lblWebBundleName.Size = new Size(104,15);
    lblWebBundleName.TabIndex = 102;
    lblWebBundleName.Text = "Web bundle name";
    // 
    // lblWebPublishDir
    // 
    lblWebPublishDir.AutoSize = true;
    lblWebPublishDir.Location = new Point(20,225);
    lblWebPublishDir.Name = "lblWebPublishDir";
    lblWebPublishDir.Size = new Size(87,15);
    lblWebPublishDir.TabIndex = 102;
    lblWebPublishDir.Text = "Webpublish dir";
    // 
    // lblMsiDownloadUrl
    // 
    lblMsiDownloadUrl.AutoSize = true;
    lblMsiDownloadUrl.Location = new Point(19,192);
    lblMsiDownloadUrl.Name = "lblMsiDownloadUrl";
    lblMsiDownloadUrl.Size = new Size(107,15);
    lblMsiDownloadUrl.TabIndex = 102;
    lblMsiDownloadUrl.Text = "MSI download URL";
    // 
    // lblWebPoductFolder
    // 
    lblWebPoductFolder.AutoSize = true;
    lblWebPoductFolder.Location = new Point(20,107);
    lblWebPoductFolder.Name = "lblWebPoductFolder";
    lblWebPoductFolder.Size = new Size(110,15);
    lblWebPoductFolder.TabIndex = 102;
    lblWebPoductFolder.Text = "Web product folder";
    // 
    // lblWebSetupFileName
    // 
    lblWebSetupFileName.AutoSize = true;
    lblWebSetupFileName.Location = new Point(20,136);
    lblWebSetupFileName.Name = "lblWebSetupFileName";
    lblWebSetupFileName.Size = new Size(115,15);
    lblWebSetupFileName.TabIndex = 102;
    lblWebSetupFileName.Text = "Web setup file name";
    // 
    // lblWebOutputDir
    // 
    lblWebOutputDir.AutoSize = true;
    lblWebOutputDir.Location = new Point(20,78);
    lblWebOutputDir.Name = "lblWebOutputDir";
    lblWebOutputDir.Size = new Size(87,15);
    lblWebOutputDir.TabIndex = 102;
    lblWebOutputDir.Text = "Web output dir";
    // 
    // chkBuildWebInstaller
    // 
    chkBuildWebInstaller.AutoSize = true;
    chkBuildWebInstaller.Location = new Point(128,12);
    chkBuildWebInstaller.Name = "chkBuildWebInstaller";
    chkBuildWebInstaller.Size = new Size(122,19);
    chkBuildWebInstaller.TabIndex = 101;
    chkBuildWebInstaller.Text = "Build web installer";
    chkBuildWebInstaller.UseVisualStyleBackColor = true;
    // 
    // txtWebBundleUpgradeCode
    // 
    txtWebBundleUpgradeCode.Location = new Point(161,162);
    txtWebBundleUpgradeCode.Name = "txtWebBundleUpgradeCode";
    txtWebBundleUpgradeCode.Size = new Size(265,23);
    txtWebBundleUpgradeCode.TabIndex = 5;
    // 
    // txtWebPublishDirectory
    // 
    txtWebPublishDirectory.Location = new Point(161,221);
    txtWebPublishDirectory.Name = "txtWebPublishDirectory";
    txtWebPublishDirectory.Size = new Size(237,23);
    txtWebPublishDirectory.TabIndex = 5;
    // 
    // txtWebBundleName
    // 
    txtWebBundleName.Location = new Point(161,46);
    txtWebBundleName.Name = "txtWebBundleName";
    txtWebBundleName.Size = new Size(265,23);
    txtWebBundleName.TabIndex = 5;
    // 
    // txtMsiDownloadUrl
    // 
    txtMsiDownloadUrl.Location = new Point(161,189);
    txtMsiDownloadUrl.Name = "txtMsiDownloadUrl";
    txtMsiDownloadUrl.Size = new Size(265,23);
    txtMsiDownloadUrl.TabIndex = 5;
    // 
    // txtWebProductFolder
    // 
    txtWebProductFolder.Location = new Point(161,104);
    txtWebProductFolder.Name = "txtWebProductFolder";
    txtWebProductFolder.Size = new Size(265,23);
    txtWebProductFolder.TabIndex = 5;
    // 
    // txtWebSetupFileName
    // 
    txtWebSetupFileName.Location = new Point(161,133);
    txtWebSetupFileName.Name = "txtWebSetupFileName";
    txtWebSetupFileName.Size = new Size(265,23);
    txtWebSetupFileName.TabIndex = 5;
    // 
    // txtWebOutputDir
    // 
    txtWebOutputDir.Location = new Point(161,75);
    txtWebOutputDir.Name = "txtWebOutputDir";
    txtWebOutputDir.Size = new Size(235,23);
    txtWebOutputDir.TabIndex = 5;
    // 
    // btnWebPublisDir
    // 
    btnWebPublisDir.Location = new Point(400,220);
    btnWebPublisDir.Name = "btnWebPublisDir";
    btnWebPublisDir.Size = new Size(24,23);
    btnWebPublisDir.TabIndex = 23;
    btnWebPublisDir.Text = "...";
    btnWebPublisDir.UseVisualStyleBackColor = true;
    btnWebPublisDir.Click += btnWebPublisDir_Click;
    // 
    // btnBrowseWebOutputDir
    // 
    btnBrowseWebOutputDir.Location = new Point(401,74);
    btnBrowseWebOutputDir.Name = "btnBrowseWebOutputDir";
    btnBrowseWebOutputDir.Size = new Size(24,23);
    btnBrowseWebOutputDir.TabIndex = 23;
    btnBrowseWebOutputDir.Text = "...";
    btnBrowseWebOutputDir.UseVisualStyleBackColor = true;
    btnBrowseWebOutputDir.Click += btnBrowseWebOutputDir_Click;
    // 
    // btnGenerateWebBundleUpgradeCode
    // 
    btnGenerateWebBundleUpgradeCode.AutoSize = true;
    btnGenerateWebBundleUpgradeCode.Location = new Point(2,157);
    btnGenerateWebBundleUpgradeCode.Margin = new Padding(0);
    btnGenerateWebBundleUpgradeCode.Name = "btnGenerateWebBundleUpgradeCode";
    btnGenerateWebBundleUpgradeCode.Size = new Size(157,31);
    btnGenerateWebBundleUpgradeCode.TabIndex = 99;
    btnGenerateWebBundleUpgradeCode.Text = "Web bundle UpgradeCode";
    btnGenerateWebBundleUpgradeCode.TextAlign = ContentAlignment.MiddleLeft;
    btnGenerateWebBundleUpgradeCode.UseVisualStyleBackColor = true;
    btnGenerateWebBundleUpgradeCode.Click += btnGenerateWebBundleUpgradeCode_Click;
    // 
    // grpWebUpload
    // 
    grpWebUpload.Controls.Add(cmbUploadProtocol);
    grpWebUpload.Controls.Add(lblProtocole);
    grpWebUpload.Controls.Add(lblWebSite);
    grpWebUpload.Controls.Add(lblWinScpPath);
    grpWebUpload.Controls.Add(lblRemoteDir);
    grpWebUpload.Controls.Add(lblLocalDir);
    grpWebUpload.Controls.Add(lblPassword);
    grpWebUpload.Controls.Add(lblUserName);
    grpWebUpload.Controls.Add(lblUplaodHost);
    grpWebUpload.Controls.Add(chkUploadWebFilesAfterBuild);
    grpWebUpload.Controls.Add(txtUploadLocalDir);
    grpWebUpload.Controls.Add(txtWebRemoteSite);
    grpWebUpload.Controls.Add(btnBrowseWinScpPath);
    grpWebUpload.Controls.Add(btnBrowseUploadLocalDir);
    grpWebUpload.Controls.Add(textBox1);
    grpWebUpload.Controls.Add(txtWinScpPath);
    grpWebUpload.Controls.Add(txtUploadRemoteDir);
    grpWebUpload.Controls.Add(txtUploadPassword);
    grpWebUpload.Controls.Add(txtUploadUserName);
    grpWebUpload.Controls.Add(txtUploadHost);
    grpWebUpload.Location = new Point(901,531);
    grpWebUpload.Name = "grpWebUpload";
    grpWebUpload.Size = new Size(430,328);
    grpWebUpload.TabIndex = 104;
    grpWebUpload.TabStop = false;
    grpWebUpload.Text = "FTP / Web Upload";
    // 
    // cmbUploadProtocol
    // 
    cmbUploadProtocol.FormattingEnabled = true;
    cmbUploadProtocol.Items.AddRange(new object[] { "FTP","FTPS","SFTP" });
    cmbUploadProtocol.Location = new Point(159,45);
    cmbUploadProtocol.Name = "cmbUploadProtocol";
    cmbUploadProtocol.Size = new Size(265,23);
    cmbUploadProtocol.TabIndex = 103;
    // 
    // lblProtocole
    // 
    lblProtocole.AutoSize = true;
    lblProtocole.Location = new Point(21,48);
    lblProtocole.Name = "lblProtocole";
    lblProtocole.Size = new Size(93,15);
    lblProtocole.TabIndex = 102;
    lblProtocole.Text = "Uplaod Protocol";
    // 
    // lblWebSite
    // 
    lblWebSite.AutoSize = true;
    lblWebSite.Location = new Point(19,300);
    lblWebSite.Name = "lblWebSite";
    lblWebSite.Size = new Size(93,15);
    lblWebSite.TabIndex = 102;
    lblWebSite.Text = "Web remote site";
    // 
    // lblWinScpPath
    // 
    lblWinScpPath.AutoSize = true;
    lblWinScpPath.Location = new Point(19,264);
    lblWinScpPath.Name = "lblWinScpPath";
    lblWinScpPath.Size = new Size(76,15);
    lblWinScpPath.TabIndex = 102;
    lblWinScpPath.Text = "WinSCP path";
    // 
    // lblRemoteDir
    // 
    lblRemoteDir.AutoSize = true;
    lblRemoteDir.Location = new Point(21,192);
    lblRemoteDir.Name = "lblRemoteDir";
    lblRemoteDir.Size = new Size(65,15);
    lblRemoteDir.TabIndex = 102;
    lblRemoteDir.Text = "Remote dir";
    // 
    // lblLocalDir
    // 
    lblLocalDir.AutoSize = true;
    lblLocalDir.Location = new Point(21,228);
    lblLocalDir.Name = "lblLocalDir";
    lblLocalDir.Size = new Size(52,15);
    lblLocalDir.TabIndex = 102;
    lblLocalDir.Text = "Local dir";
    // 
    // lblPassword
    // 
    lblPassword.AutoSize = true;
    lblPassword.Location = new Point(21,156);
    lblPassword.Name = "lblPassword";
    lblPassword.Size = new Size(57,15);
    lblPassword.TabIndex = 102;
    lblPassword.Text = "Password";
    // 
    // lblUserName
    // 
    lblUserName.AutoSize = true;
    lblUserName.Location = new Point(20,120);
    lblUserName.Name = "lblUserName";
    lblUserName.Size = new Size(65,15);
    lblUserName.TabIndex = 102;
    lblUserName.Text = "User Name";
    // 
    // lblUplaodHost
    // 
    lblUplaodHost.AutoSize = true;
    lblUplaodHost.Location = new Point(21,83);
    lblUplaodHost.Name = "lblUplaodHost";
    lblUplaodHost.Size = new Size(73,15);
    lblUplaodHost.TabIndex = 102;
    lblUplaodHost.Text = "Uplaod Host";
    // 
    // chkUploadWebFilesAfterBuild
    // 
    chkUploadWebFilesAfterBuild.AutoSize = true;
    chkUploadWebFilesAfterBuild.Location = new Point(128,12);
    chkUploadWebFilesAfterBuild.Name = "chkUploadWebFilesAfterBuild";
    chkUploadWebFilesAfterBuild.Size = new Size(170,19);
    chkUploadWebFilesAfterBuild.TabIndex = 101;
    chkUploadWebFilesAfterBuild.Text = "Upload web files after build";
    chkUploadWebFilesAfterBuild.UseVisualStyleBackColor = true;
    // 
    // txtUploadLocalDir
    // 
    txtUploadLocalDir.Location = new Point(160,225);
    txtUploadLocalDir.Name = "txtUploadLocalDir";
    txtUploadLocalDir.Size = new Size(237,23);
    txtUploadLocalDir.TabIndex = 5;
    // 
    // txtWebRemoteSite
    // 
    txtWebRemoteSite.Location = new Point(159,297);
    txtWebRemoteSite.Name = "txtWebRemoteSite";
    txtWebRemoteSite.Size = new Size(265,23);
    txtWebRemoteSite.TabIndex = 5;
    // 
    // btnBrowseWinScpPath
    // 
    btnBrowseWinScpPath.Location = new Point(403,260);
    btnBrowseWinScpPath.Name = "btnBrowseWinScpPath";
    btnBrowseWinScpPath.Size = new Size(24,23);
    btnBrowseWinScpPath.TabIndex = 23;
    btnBrowseWinScpPath.Text = "...";
    btnBrowseWinScpPath.UseVisualStyleBackColor = true;
    btnBrowseWinScpPath.Click += btnBrowseWinScpPath_Click;
    // 
    // btnBrowseUploadLocalDir
    // 
    btnBrowseUploadLocalDir.Location = new Point(403,228);
    btnBrowseUploadLocalDir.Name = "btnBrowseUploadLocalDir";
    btnBrowseUploadLocalDir.Size = new Size(24,23);
    btnBrowseUploadLocalDir.TabIndex = 23;
    btnBrowseUploadLocalDir.Text = "...";
    btnBrowseUploadLocalDir.UseVisualStyleBackColor = true;
    btnBrowseUploadLocalDir.Click += btnBrowseUploadLocalDir_Click;
    // 
    // textBox1
    // 
    textBox1.Location = new Point(159,668);
    textBox1.Name = "textBox1";
    textBox1.Size = new Size(237,23);
    textBox1.TabIndex = 5;
    // 
    // txtWinScpPath
    // 
    txtWinScpPath.Location = new Point(160,261);
    txtWinScpPath.Name = "txtWinScpPath";
    txtWinScpPath.Size = new Size(237,23);
    txtWinScpPath.TabIndex = 5;
    // 
    // txtUploadRemoteDir
    // 
    txtUploadRemoteDir.Location = new Point(159,189);
    txtUploadRemoteDir.Name = "txtUploadRemoteDir";
    txtUploadRemoteDir.Size = new Size(265,23);
    txtUploadRemoteDir.TabIndex = 5;
    // 
    // txtUploadPassword
    // 
    txtUploadPassword.Location = new Point(160,153);
    txtUploadPassword.Name = "txtUploadPassword";
    txtUploadPassword.PasswordChar = '●';
    txtUploadPassword.Size = new Size(264,23);
    txtUploadPassword.TabIndex = 5;
    // 
    // txtUploadUserName
    // 
    txtUploadUserName.Location = new Point(159,117);
    txtUploadUserName.Name = "txtUploadUserName";
    txtUploadUserName.Size = new Size(264,23);
    txtUploadUserName.TabIndex = 5;
    // 
    // txtUploadHost
    // 
    txtUploadHost.Location = new Point(160,81);
    txtUploadHost.Name = "txtUploadHost";
    txtUploadHost.Size = new Size(264,23);
    txtUploadHost.TabIndex = 5;
    // 
    // btnNewProfile
    // 
    btnNewProfile.Location = new Point(19,443);
    btnNewProfile.Name = "btnNewProfile";
    btnNewProfile.Size = new Size(90,30);
    btnNewProfile.TabIndex = 18;
    btnNewProfile.Text = "New profile";
    btnNewProfile.UseVisualStyleBackColor = true;
    btnNewProfile.Click += btnNewProfile_Click;
    // 
    // btnSaveProfilAs
    // 
    btnSaveProfilAs.Location = new Point(307,443);
    btnSaveProfilAs.Name = "btnSaveProfilAs";
    btnSaveProfilAs.Size = new Size(104,30);
    btnSaveProfilAs.TabIndex = 18;
    btnSaveProfilAs.Text = "Save profile as";
    btnSaveProfilAs.UseVisualStyleBackColor = true;
    btnSaveProfilAs.Click += btnSaveProfileAs_Click;
    // 
    // pnlMain
    // 
    pnlMain.Controls.Add(btnTestUploadConnection);
    pnlMain.Controls.Add(btnUploadNow);
    pnlMain.Controls.Add(txtLog);
    pnlMain.Controls.Add(grpbxMisOptions);
    pnlMain.Controls.Add(grpWebUpload);
    pnlMain.Controls.Add(grpWebInstaller);
    pnlMain.Controls.Add(grpbxBundle);
    pnlMain.Controls.Add(grpbxFeature);
    pnlMain.Controls.Add(btnLoadProfile);
    pnlMain.Controls.Add(btnSaveProfilAs);
    pnlMain.Controls.Add(btnNewProfile);
    pnlMain.Controls.Add(btnSaveProfile);
    pnlMain.Controls.Add(btnAndroidPackageSettings);
    pnlMain.Controls.Add(btnGenerateWix);
    pnlMain.Controls.Add(btnBuildMsi);
    pnlMain.Controls.Add(tabPreview);
    pnlMain.Controls.Add(btnOpenPackagerFolder);
    pnlMain.Controls.Add(mainMenu);
    pnlMain.Dock = DockStyle.Fill;
    pnlMain.Location = new Point(2,2);
    pnlMain.Margin = new Padding(0);
    pnlMain.Name = "pnlMain";
    pnlMain.Size = new Size(1340,917);
    pnlMain.TabIndex = 107;
    // 
    // btnTestUploadConnection
    // 
    btnTestUploadConnection.Location = new Point(1029,873);
    btnTestUploadConnection.Name = "btnTestUploadConnection";
    btnTestUploadConnection.Size = new Size(106,30);
    btnTestUploadConnection.TabIndex = 107;
    btnTestUploadConnection.Text = "Connection Test";
    btnTestUploadConnection.UseVisualStyleBackColor = true;
    // 
    // btnUploadNow
    // 
    btnUploadNow.Location = new Point(1144,873);
    btnUploadNow.Name = "btnUploadNow";
    btnUploadNow.Size = new Size(106,30);
    btnUploadNow.TabIndex = 108;
    btnUploadNow.Text = "Upload";
    btnUploadNow.UseVisualStyleBackColor = true;
    // 
    // txtLog
    // 
    txtLog.Location = new Point(12,723);
    txtLog.Name = "txtLog";
    txtLog.Size = new Size(883,180);
    txtLog.TabIndex = 109;
    txtLog.Text = "";
    // 
    // btnAndroidPackageSettings
    // 
    btnAndroidPackageSettings.Location = new Point(471,445);
    btnAndroidPackageSettings.Name = "btnAndroidPackageSettings";
    btnAndroidPackageSettings.Size = new Size(90,30);
    btnAndroidPackageSettings.TabIndex = 19;
    btnAndroidPackageSettings.Text = "APK setting...";
    btnAndroidPackageSettings.UseVisualStyleBackColor = true;
    btnAndroidPackageSettings.Click += btnAndroidPackageSettings_Click;
    // 
    // MainForm
    // 
    AutoScaleDimensions = new SizeF(7F,15F);
    AutoScaleMode = AutoScaleMode.Font;
    ClientSize = new Size(1344,921);
    Controls.Add(pnlMain);
    Icon = (Icon)resources.GetObject("$this.Icon");
    MaximizeBox = false;
    MinimizeBox = false;
    Name = "MainForm";
    Padding = new Padding(2);
    StartPosition = FormStartPosition.CenterScreen;
    Text = "MSI Software Packager";
    Click += btnGenerateBundleUpgradeCode_Click;
    mainMenu.ResumeLayout(false);
    mainMenu.PerformLayout();
    grpbxFeature.ResumeLayout(false);
    grpbxFeature.PerformLayout();
    grpbxBundle.ResumeLayout(false);
    grpbxBundle.PerformLayout();
    grpbxMisOptions.ResumeLayout(false);
    grpbxMisOptions.PerformLayout();
    grpWebInstaller.ResumeLayout(false);
    grpWebInstaller.PerformLayout();
    grpWebUpload.ResumeLayout(false);
    grpWebUpload.PerformLayout();
    pnlMain.ResumeLayout(false);
    pnlMain.PerformLayout();
    ResumeLayout(false);
  }

  private TextBox txtUpgradeCode;
  private Button btnGenerateUpgradeCode;
  private Button btnBrowseWixOutputDir;
  private TextBox txtWixOutputDir;
  private Label label1;
  private CheckBox chkRequireAdministrator;
  private Button btnVersionPatch;
  private Button btnVersionMinor;
  private Button btnVersionMajor;
  private CheckBox chkBuildBundle;
  private TextBox txtBundleOutputDir;
  private Label lblBundleOutputDir;
  private Button btnBrowseBundleDir;
  private Label lblBundleFileName;
  private TextBox txtBundleFileName;
  private TextBox txtBundleUpgradeCode;
  private GroupBox grpbxFeature;
  private GroupBox grpbxBundle;
  private Button btnGenerateBundleUpgradeCode;
  private GroupBox grpbxMisOptions;
  private Label lblBundleName;
  private TextBox txtBundleName;
  private GroupBox grpWebInstaller;
  private Label lblWebBundleName;
  private Label lblWebSetupFileName;
  private Label lblWebOutputDir;
  private CheckBox chkBuildWebInstaller;
  private TextBox txtWebBundleUpgradeCode;
  private TextBox txtWebBundleName;
  private TextBox txtWebSetupFileName;
  private TextBox txtWebOutputDir;
  private Button btnBrowseWebOutputDir;
  private Button btnGenerateWebBundleUpgradeCode;
  private Label lblMsiDownloadUrl;
  private TextBox txtMsiDownloadUrl;
  private Label lblWebPublishDir;
  private TextBox txtWebPublishDirectory;
  private GroupBox grpWebUpload;
  private Label lblProtocole;
  private Label lblWebSite;
  private Label lblWinScpPath;
  private Label lblLocalDir;
  private Label lblUplaodHost;
  private CheckBox chkUploadWebFilesAfterBuild;
  private TextBox txtUploadLocalDir;
  private TextBox txtWebRemoteSite;
  private TextBox txtWinScpPath;
  private TextBox txtUploadRemoteDir;
  private TextBox txtUploadHost;
  private ComboBox cmbUploadProtocol;
  private Label lblRemoteDir;
  private Label lblPassword;
  private TextBox txtUploadPassword;
  private Label lblUserName;
  private TextBox txtUploadUserName;
  private Button btnBrowseUploadLocalDir;
  private TextBox textBox1;
  private Button btnBrowseWinScpPath;
  private Button btnWebPublisDir;
  private Button btnNewProfile;
  private Button btnSaveProfilAs;
  private ToolStripMenuItem mnuFile;
  private ToolStripMenuItem mnuNewProfile;
  private ToolStripMenuItem mnuLoadProfile;
  private ToolStripMenuItem mnuSaveProfile;
  private ToolStripMenuItem mnuSaveProfileAs;
  private ToolStripMenuItem mnuTools;
  private ToolStripMenuItem mnuGenerateWix;
  private ToolStripSeparator toolStripSeparator1;
  private ToolStripMenuItem mnuPrintSelectedView;
  private ToolStripSeparator toolStripSeparator2;
  private ToolStripMenuItem mnuBuildMsi;
  private ToolStripMenuItem mnuUploadNow;
  private ToolStripMenuItem mnuExit;
  private ToolStripMenuItem mnuOpenPackagerFolder;
  private ToolStripSeparator toolStripSeparator3;
  private ToolStripMenuItem mnuEditSelectedViewNoteapad;
  private ToolStripMenuItem mnuNewProfileFromCsproj;
  private ToolStripMenuItem mnuValidateProfile;
  private ToolStripMenuItem mnuUpdateCsprojFromProfile;
  private ToolStripSeparator toolStripSeparator4;
  private ToolStripMenuItem mnuSaveUploadCredential;
  private ToolStripMenuItem mnuRestoreDefaultWinScpPath;
  private ToolStripMenuItem mnuGenerateWebSiteOnly;
  private ToolStripMenuItem mnuUploadWebSiteOnly;
  private ToolStripMenuItem mnuOpenLocalSiteWeb;
  private ToolStripMenuItem mnuOpenBuildReportsFolder;
  private ToolStripMenuItem mnuOpenBuildArchivesFolder;
  private ToolStripMenuItem mnuRefreshBuildHistory;
  private ToolStripMenuItem mnuBuild;
  private ToolStripSeparator toolStripSeparator5;
  private ToolStripMenuItem mnuHelp;
  private ToolStripMenuItem mnuCheckForUpdates;
  private ToolStripMenuItem mnuAbout;
  private ToolStripMenuItem mnuCodeSigningSettings;
  private ToolStripSeparator toolStripSeparator6;
  private ToolStripSeparator toolStripSeparator7;
  private ToolStripMenuItem mnuEnvironmentCheck;
  private ToolStripSeparator toolStripSeparator8;
  private ToolStripMenuItem mnuReleaseChecklist;
  private ToolStripMenuItem mnuGlobalSettings;
  private ToolStripMenuItem mnuOpenGlobalSettingsFile;
  private ToolStripMenuItem mnuOpenGlobalSettingsFolder;
  private ToolStripMenuItem mnuReleaseSummary;
  private Label lblWebPoductFolder;
  private TextBox txtWebProductFolder;
  private CheckBox chkRequireLicenseAcceptance;
  private ToolStripMenuItem mnuLicense;
  private ToolStripMenuItem mnuImportLicense;
  private CheckBox chkTechnicalLicenseRequired;
  private Panel pnlMain;
  private Button btnTestUploadConnection;
  private Button btnUploadNow;
  private RichTextBox txtLog;
  private Button btnAndroidPackageSettings;
}