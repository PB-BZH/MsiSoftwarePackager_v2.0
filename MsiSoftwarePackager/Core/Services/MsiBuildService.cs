using System.Diagnostics;
using System.Text;
using System.Text.Json;
using MsiSoftwarePackager.Core.Models;
using PB.BZH.Help.Library.Core.Services;

namespace MsiSoftwarePackager.Core.Services;

public sealed class MsiBuildService {
  public Action<string>? LogChanged { get; set; }

  private void WriteUpdateSettingsFile(MsiPackageProfile profile) {
    if (string.IsNullOrWhiteSpace(profile.UpdateManifest.UpdateManifestUrl)) {
      Log("[WARN] Update settings not generated : update manifest URL is empty.");
      return;
    }

    string updateSettingsPath = Path.Combine(profile.Publish.PublishDirectory,"UpdateSettings.json");

    UpdateSettings settings = new() {
      ApplicationId = profile.UpdateManifest.ApplicationId,
      UpdateManifestUrl = profile.UpdateManifest.UpdateManifestUrl
    };

    string json = JsonSerializer.Serialize(settings,UpdateSettingsHelper.JsonIndentedOptions);
    File.WriteAllText(updateSettingsPath,json,EncodingHelper.Utf8NoBom);
    Log("[OK] Update settings generated : " + updateSettingsPath);
  }

  public void Build(MsiPackageProfile profile,string templateRoot) {
    Log("=== MSI BUILD START ===");

    Log("[INFO] SignArtifacts  : " + profile.Signing.SignArtifacts);
    Log("[INFO] SignExecutable : " + profile.Signing.SignExecutable);
    Log("[INFO] SignMsi        : " + profile.Signing.SignMsi);
    Log("[INFO] SignBundle     : " + profile.Signing.SignBundle);
    Log("[INFO] SignWebSetup   : " + profile.Signing.SignWebSetup);
    Log("[INFO] SignTool       : " + profile.Signing.SignToolPath);
    Log("[INFO] Certificate    : " + profile.Signing.CertificateThumbprint);

    PublishProject(profile);
    WriteUpdateSettingsFile(profile);
    SignArtifactIfRequired(
        profile,
        new SigningArtifact(
            "Published executable",
            Path.Combine(
                profile.Publish.PublishDirectory,
                profile.TargetProject.ExecutableName
            ),
            profile.Signing.SignExecutable
        )
    );

    GenerateWixFiles(profile,templateRoot);
    BuildMsi(profile);
    SignArtifactIfRequired(
        profile,
        new SigningArtifact(
            "MSI installer",
            Path.Combine(
                profile.Output.MsiOutputDirectory,
                profile.Output.MsiFileName
            ),
            profile.Signing.SignMsi
        )
    );

    ValidateGeneratedMsi(profile);
    WriteSha256SidecarFile(GetMsiPath(profile));

    if (profile.Bundle.BuildBundle) {
      BuildBundle(profile);
      SignArtifactIfRequired(
          profile,
          new SigningArtifact(
              "Setup bundle",
              Path.Combine(
                  profile.Bundle.BundleOutputDirectory,
                  profile.Bundle.BundleFileName
              ),
              profile.Signing.SignBundle
          )
      );

      ValidateGeneratedBundle(profile);
      WriteSha256SidecarFile(GetBundlePath(profile));
    }

    if (profile.WebInstaller.BuildWebInstaller) {
      BuildWebInstaller(profile);
      SignArtifactIfRequired(
          profile,
          new SigningArtifact(
              "Web setup",
              Path.Combine(
                  profile.WebInstaller.WebOutputDirectory,
                  profile.WebInstaller.WebSetupFileName
              ),
              profile.Signing.SignWebSetup
          )
      );
      ValidateGeneratedWebInstaller(profile);
      WriteSha256SidecarFile(GetWebInstallerPath(profile));
    }

    Log("=== MSI BUILD SUCCESS ===");
  }

  private void VerifySignedFile(
    MsiPackageProfile profile,
    string filePath) {
    if (string.IsNullOrWhiteSpace(filePath))
      throw new InvalidOperationException("File path to verify is empty.");

    if (!File.Exists(filePath))
      throw new FileNotFoundException("File to verify was not found.",filePath);

    string signToolPath =
        ToolPathResolver.ResolveSignToolPath(
            profile.Signing.SignToolPath
        );

    if (string.IsNullOrWhiteSpace(signToolPath) ||
        !File.Exists(signToolPath)) {
      throw new FileNotFoundException(
          "SignTool was not found for signature verification.",
          signToolPath
      );
    }

    Log("[INFO] Verifying digital signature : " + filePath);

    ProcessStartInfo psi = new() {
      FileName = signToolPath,
      Arguments =
          "verify " +
          "/pa " +
          "/v " +
          "\"" + filePath + "\"",
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

    StringBuilder outputBuilder = new();
    StringBuilder errorBuilder = new();

    process.OutputDataReceived += (_,e) => {
      if (!string.IsNullOrWhiteSpace(e.Data)) {
        outputBuilder.AppendLine(e.Data);
        Log("[SignTool VERIFY] " + e.Data);
      }
    };

    process.ErrorDataReceived += (_,e) => {
      if (!string.IsNullOrWhiteSpace(e.Data)) {
        errorBuilder.AppendLine(e.Data);
        Log("[SignTool VERIFY ERROR] " + e.Data);
      }
    };

    process.Start();

    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    process.WaitForExit();

    if (process.ExitCode != 0) {
      throw new InvalidOperationException(
          "Digital signature verification failed. Exit code: " +
          process.ExitCode +
          Environment.NewLine +
          filePath +
          Environment.NewLine +
          outputBuilder +
          errorBuilder
      );
    }

    Log("[OK] Digital signature verified : " + filePath);
  }

  private static string GetMsiPath(MsiPackageProfile profile) {
    return Path.Combine(profile.Output.MsiOutputDirectory,profile.Output.MsiFileName);
  }

  private sealed class SigningArtifact {
    public string DisplayName { get; }
    public string FilePath { get; }
    public bool IsEnabled { get; }

    public SigningArtifact(string displayName,string filePath,bool isEnabled) {
      DisplayName = displayName;
      FilePath = filePath;
      IsEnabled = isEnabled;
    }
  }

  private void SignArtifactIfRequired(
    MsiPackageProfile profile,
    SigningArtifact artifact) {
    if (artifact == null)
      throw new ArgumentNullException(nameof(artifact));

    if (!profile.Signing.SignArtifacts) {
      Log("[INFO] Signing disabled globally. Skipped : " + artifact.DisplayName);
      return;
    }

    if (!artifact.IsEnabled) {
      Log("[INFO] Signing disabled for : " + artifact.DisplayName);
      return;
    }

    if (string.IsNullOrWhiteSpace(artifact.FilePath)) {
      throw new InvalidOperationException(
          "File path is empty for signing artifact : " +
          artifact.DisplayName
      );
    }

    Log("[INFO] Signing artifact : " + artifact.DisplayName);
    SignFile(profile,artifact.FilePath);
  }

  private void SignFile(MsiPackageProfile profile,string filePath) {
    string signingProvider =
        profile.Signing.SigningProvider ?? "SignToolCka";

    if (string.Equals(
            signingProvider,
            "SslComCodeSignTool",
            StringComparison.OrdinalIgnoreCase)) {
      SignFileWithSslComCodeSignTool(profile,filePath);
      return;
    }

    SignFileWithSignToolCka(profile,filePath);
  }

  private void SignFileWithSignToolCka(MsiPackageProfile profile,string filePath) {
    if (string.IsNullOrWhiteSpace(filePath))
      throw new InvalidOperationException("File path to sign is empty.");

    if (!File.Exists(filePath))
      throw new FileNotFoundException("File to sign was not found.",filePath);

    string signToolPath =
        ToolPathResolver.ResolveSignToolPath(
            profile.Signing.SignToolPath
        );

    if (string.IsNullOrWhiteSpace(signToolPath) || !File.Exists(signToolPath))
      throw new FileNotFoundException("SignTool was not found.",signToolPath);

    if (string.IsNullOrWhiteSpace(profile.Signing.CertificateThumbprint))
      throw new InvalidOperationException("Certificate thumbprint is empty.");

    if (string.IsNullOrWhiteSpace(profile.Signing.TimestampUrl))
      throw new InvalidOperationException("Timestamp URL is empty.");

    string thumbprint =
        profile.Signing.CertificateThumbprint.Replace(" ","").Trim();

    string timestampUrl =
        profile.Signing.TimestampUrl.Trim();

    Log("[INFO] Signing file with SignTool / CKA : " + filePath);
    Log("[INFO] SignTool : " + signToolPath);
    Log("[INFO] Certificate thumbprint : " + thumbprint);

    ProcessStartInfo psi = new() {
      FileName = signToolPath,
      Arguments =
          "sign " +
          "/fd SHA256 " +
          "/tr \"" + timestampUrl + "\" " +
          "/td SHA256 " +
          "/sha1 \"" + thumbprint + "\" " +
          "\"" + filePath + "\"",
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
        Log("[SignTool] " + e.Data);
    };

    process.ErrorDataReceived += (_,e) => {
      if (!string.IsNullOrWhiteSpace(e.Data))
        Log("[SignTool ERROR] " + e.Data);
    };

    process.Start();

    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    process.WaitForExit();

    if (process.ExitCode != 0) {
      throw new InvalidOperationException(
          "File signing failed with SignTool / CKA. Exit code: " +
          process.ExitCode +
          Environment.NewLine +
          filePath
      );
    }

    Log("[OK] File signed with SignTool / CKA : " + filePath);

    VerifySignedFile(profile,filePath);

  }

  private void SignFileWithSslComCodeSignTool(MsiPackageProfile profile,string filePath) {
    if (string.IsNullOrWhiteSpace(filePath))
      throw new InvalidOperationException("File path to sign is empty.");

    if (!File.Exists(filePath))
      throw new FileNotFoundException("File to sign was not found.",filePath);

    if (string.IsNullOrWhiteSpace(profile.Signing.SslComCodeSignToolPath))
      throw new InvalidOperationException("SSL.com CodeSignTool path is empty.");

    string codeSignToolPath = profile.Signing.SslComCodeSignToolPath.Trim();

    if (!File.Exists(codeSignToolPath))
      throw new FileNotFoundException("SSL.com CodeSignTool was not found.",codeSignToolPath);

    string codeSignToolDirectory = Path.GetDirectoryName(codeSignToolPath) ?? string.Empty;

    if (string.IsNullOrWhiteSpace(codeSignToolDirectory) ||
        !Directory.Exists(codeSignToolDirectory)) {
      throw new DirectoryNotFoundException(
          "SSL.com CodeSignTool directory was not found: " +
          codeSignToolDirectory
      );
    }

    if (string.IsNullOrWhiteSpace(profile.Signing.SslComLogin))
      throw new InvalidOperationException("SSL.com login is empty.");

    if (string.IsNullOrWhiteSpace(profile.Signing.SslComCredentialTarget))
      throw new InvalidOperationException("SSL.com credential target is empty.");

    string sslComPassword = WindowsCredentialManager.ReadPassword(profile.Signing.SslComCredentialTarget);

    if (string.IsNullOrWhiteSpace(sslComPassword)) {
      throw new InvalidOperationException(
          "SSL.com password was not found in Windows Credential Manager."
      );
    }

    string totpSecret = string.Empty;

    if (!string.IsNullOrWhiteSpace(profile.Signing.SslComTotpSecretCredentialTarget)) {
      totpSecret = WindowsCredentialManager.ReadPassword(profile.Signing.SslComTotpSecretCredentialTarget);
    }

    string originalDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;
    string fileName = Path.GetFileName(filePath);
    string tempOutputDirectory = Path.Combine(originalDirectory,"_codesign_temp_" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(tempOutputDirectory);
    string signedFilePath = Path.Combine(tempOutputDirectory,fileName);

    try {
      Log("[INFO] Signing file with SSL.com CodeSignTool : " + filePath);
      Log("[INFO] CodeSignTool : " + codeSignToolPath);
      Log("[INFO] CodeSignTool working directory : " + codeSignToolDirectory);
      Log("[INFO] SSL.com login : " + profile.Signing.SslComLogin);
      Log("[INFO] Temporary signed output directory : " + tempOutputDirectory);

      string arguments =
          "sign " +
          "-username=\"" + profile.Signing.SslComLogin + "\" " +
          "-password=\"" + sslComPassword + "\" " +
          "-input_file_path=\"" + filePath + "\" " +
          "-output_dir_path=\"" + tempOutputDirectory + "\"";

      if (!string.IsNullOrWhiteSpace(profile.Signing.SslComCredentialId)) {
        arguments +=
            " -credential_id=\"" +
            profile.Signing.SslComCredentialId.Trim() +
            "\"";
      }

      if (!string.IsNullOrWhiteSpace(totpSecret)) {
        arguments +=
            " -totp_secret=\"" +
            totpSecret +
            "\"";
      }

      ProcessStartInfo psi = new() {
        FileName = codeSignToolPath,
        Arguments = arguments,
        WorkingDirectory = codeSignToolDirectory,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true,
        StandardOutputEncoding = Encoding.UTF8,
        StandardErrorEncoding = Encoding.UTF8
      };

      psi.EnvironmentVariables["CODE_SIGN_TOOL_PATH"] = codeSignToolDirectory;

      StringBuilder outputBuilder = new();
      StringBuilder errorBuilder = new();

      using Process process = new() {
        StartInfo = psi
      };

      process.OutputDataReceived += (_,e) => {
        if (string.IsNullOrWhiteSpace(e.Data))
          return;

        outputBuilder.AppendLine(e.Data);
        Log("[CodeSignTool] " + MaskSensitiveSigningLog(e.Data));
      };

      process.ErrorDataReceived += (_,e) => {
        if (string.IsNullOrWhiteSpace(e.Data))
          return;

        errorBuilder.AppendLine(e.Data);
        Log("[CodeSignTool ERROR] " + MaskSensitiveSigningLog(e.Data));
      };

      process.Start();

      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      process.WaitForExit();

      string outputText = outputBuilder.ToString();
      string errorText = errorBuilder.ToString();

      if (process.ExitCode != 0 ||
          errorText.Contains("java.io.IOException",StringComparison.OrdinalIgnoreCase) ||
          errorText.Contains("Exception",StringComparison.OrdinalIgnoreCase)) {
        throw new InvalidOperationException(
            "File signing failed with SSL.com CodeSignTool. Exit code: " +
            process.ExitCode +
            Environment.NewLine +
            filePath +
            Environment.NewLine +
            errorText
        );
      }

      if (!File.Exists(signedFilePath)) {
        throw new FileNotFoundException("Signed output file was not created by SSL.com CodeSignTool.",signedFilePath);
      }

      File.Copy(signedFilePath,filePath,overwrite: true);
      Log("[OK] Signed file copied back : " + filePath);

      VerifySignedFile(profile,filePath);
    }
    finally {
      try {
        if (Directory.Exists(tempOutputDirectory))
          Directory.Delete(tempOutputDirectory,recursive: true);
      }
      catch {
        Log("[WARNING] Unable to delete temporary signing directory : " +
            tempOutputDirectory);
      }
    }
  }

  private static string MaskSensitiveSigningLog(string value) {
    if (string.IsNullOrEmpty(value))
      return string.Empty;

    return value
        .Replace("-password=","-password=***")
        .Replace("-totp_secret=","-totp_secret=***");
  }

  private void BuildWebInstaller(
    MsiPackageProfile profile) {

    Log("Building web installer...");

    string wixDir =
      profile.Output.WixOutputDirectory;

    string webInstallerPath =
      GetWebInstallerPath(profile);

    Directory.CreateDirectory(
      Path.GetDirectoryName(webInstallerPath)!);

    string wixExe =
      ResolveWixExePath();

    Log("[INFO] WiX executable : " + wixExe);

    EnsureWixExtensionInstalled(
      wixExe,
      wixDir,
      "WixToolset.BootstrapperApplications.wixext");

    string args =
      $"build WebBundle.wxs " +
      $"-ext WixToolset.BootstrapperApplications.wixext " +
      $"-o \"{webInstallerPath}\"";

    int exitCode =
      RunProcess(
        wixExe,
        args,
        wixDir,
        Log);

    if (exitCode != 0) {
      throw new InvalidOperationException(
        "Web installer build failed. Exit code: " + exitCode);
    }

    Log("Web installer generated : " + webInstallerPath);
  }

  private void ValidateGeneratedWebInstaller(MsiPackageProfile profile) {
    string webInstallerPath = GetWebInstallerPath(profile);

    if (!File.Exists(webInstallerPath)) {
      throw new InvalidOperationException("Web installer file was not generated: " + webInstallerPath);
    }

    FileInfo info = new(webInstallerPath);

    if (info.Length == 0) {
      throw new InvalidOperationException(
          "Web installer file is empty: " +
          webInstallerPath
      );
    }

    Log("Web installer validated.");
    Log("Web installer size : " + FormatFileSize(info.Length));
  }

  private void BuildBundle(
    MsiPackageProfile profile) {

    Log("Building setup.exe bundle...");

    string wixDir =
      profile.Output.WixOutputDirectory;

    string bundlePath =
      GetBundlePath(profile);

    Directory.CreateDirectory(
      Path.GetDirectoryName(bundlePath)!);

    string wixExe =
      ResolveWixExePath();

    Log("[INFO] WiX executable : " + wixExe);

    EnsureWixExtensionInstalled(
      wixExe,
      wixDir,
      "WixToolset.BootstrapperApplications.wixext");

    string args =
      $"build Bundle.wxs " +
      $"-ext WixToolset.BootstrapperApplications.wixext " +
      $"-o \"{bundlePath}\"";

    int exitCode =
      RunProcess(
        wixExe,
        args,
        wixDir,
        Log);

    if (exitCode != 0) {
      throw new InvalidOperationException(
        "Bundle build failed. Exit code: " + exitCode);
    }

    Log("Bundle generated : " + bundlePath);
  }

  private static string GetBundlePath(MsiPackageProfile profile) {
    string bundleDir =
        GetAbsoluteDirectory(profile.Bundle.BundleOutputDirectory);

    return Path.Combine(
        bundleDir,
        profile.Bundle.BundleFileName
    );
  }

  private void ValidateGeneratedBundle(MsiPackageProfile profile) {
    string bundlePath = GetBundlePath(profile);

    if (!File.Exists(bundlePath)) {
      throw new InvalidOperationException(
          "Bundle file was not generated: " + bundlePath
      );
    }

    FileInfo info = new(bundlePath);

    if (info.Length == 0) {
      throw new InvalidOperationException(
          "Bundle file is empty: " + bundlePath
      );
    }

    Log("Bundle validated.");
    Log("Bundle size : " + FormatFileSize(info.Length));
  }

  private void PublishProject(MsiPackageProfile profile) {
    Log("Publishing .NET project...");

    string projectFile = profile.TargetProject.ProjectFile;
    string projectDir = Path.GetDirectoryName(projectFile)!;

    if (Directory.Exists(profile.Publish.PublishDirectory)) {
      Directory.Delete(
          profile.Publish.PublishDirectory,
          recursive: true
      );
    }

    string technicalLicenseArgument =
      profile.License.TechnicalLicenseRequired
        ? "-p:TechnicalLicenseRequired=true"
        : string.Empty;

    string args =
        $"publish \"{projectFile}\" " +
        $"-c {profile.Publish.Configuration} " +
        $"-r {profile.Publish.RuntimeIdentifier} " +
        $"-o \"{profile.Publish.PublishDirectory}\" " +
        $"-p:SelfContained={profile.Publish.SelfContained.ToString().ToLower()} " +
        $"-p:PublishSingleFile={profile.Publish.SingleFile.ToString().ToLower()} " +
        $"-p:PublishReadyToRun={profile.Publish.ReadyToRun.ToString().ToLower()} " +
        $"-p:PublishTrimmed={profile.Publish.Trimmed.ToString().ToLower()} " +
        technicalLicenseArgument;

    Directory.CreateDirectory(profile.Publish.PublishDirectory);

    int exitCode =
      RunProcess(
        "dotnet",
        args,
        projectDir,
        Log);

    if (exitCode != 0) {
      throw new InvalidOperationException(
        "dotnet publish failed. Exit code: " + exitCode);
    }

    string exePath = Path.Combine(profile.Publish.PublishDirectory,profile.TargetProject.ExecutableName);

    if (!File.Exists(exePath))
      throw new InvalidOperationException("Published executable not found: " + exePath);

    Log("Published executable validated : " + exePath);

    Log("Publish completed.");
  }

  private static string GetAbsoluteDirectory(string path) {
    if (string.IsNullOrWhiteSpace(path))
      throw new InvalidOperationException("Directory path is empty.");

    return Path.GetFullPath(path);
  }

  private static string GetWebInstallerPath(MsiPackageProfile profile) {
    string webDir =
        Path.GetFullPath(
            profile.WebInstaller.WebOutputDirectory
        );

    return Path.Combine(
        webDir,
        profile.WebInstaller.WebSetupFileName
    );
  }

  private void GenerateWixFiles(MsiPackageProfile profile,string templateRoot) {
    Log("Generating WiX files...");

    WixProjectGenerator.Generate(
      profile,
      templateRoot,
      profile.Output.WixOutputDirectory
      );

    Log("WiX files generated.");
  }

  private void EnsureWixExtensionInstalled(
    string wixExe,
    string wixDir,
    string extensionId) {

    Log("[INFO] Ensuring WiX extension : " + extensionId);

    RunProcess(
      wixExe,
      $"extension add {extensionId}/7.0.0",
      wixDir,
      Log);

    Log("[INFO] WiX extensions installed :");

    RunProcess(
      wixExe,
      "extension list",
      wixDir,
      Log);
  }

  private static string ResolveWixExePath() {
    string userProfile =
      Environment.GetFolderPath(
        Environment.SpecialFolder.UserProfile);

    string globalToolWix =
      Path.Combine(
        userProfile,
        ".dotnet",
        "tools",
        "wix.exe");

    if (File.Exists(globalToolWix)) {
      return globalToolWix;
    }

    return "wix";
  }

  private static string WriteSha256SidecarFile(string filePath) {
    if (!File.Exists(filePath)) {
      throw new FileNotFoundException(
          "Cannot generate SHA256 because file does not exist.",
          filePath);
    }

    using FileStream stream = File.OpenRead(filePath);
    byte[] hashBytes = System.Security.Cryptography.SHA256.HashData(stream);

    string hash =
        Convert.ToHexString(hashBytes);

    string shaPath =
        filePath + ".sha256.txt";

    string content =
        hash +
        "  " +
        Path.GetFileName(filePath) +
        Environment.NewLine;

    File.WriteAllText(
        shaPath,
        content,
        new System.Text.UTF8Encoding(false));

    return shaPath;
  }

  private void BuildMsi(MsiPackageProfile profile) {
    Log("Building MSI...");

    string wixDir =
        profile.Output.WixOutputDirectory;

    string msiPath = GetMsiPath(profile);

    Directory.CreateDirectory(
        Path.GetDirectoryName(msiPath)!
    );

    string wixExe =
      ResolveWixExePath();

    Log("[INFO] WiX executable : " + wixExe);

    string wixExtensionArgs = string.Empty;

    if (profile.License.RequireLicenseAcceptance) {
      EnsureWixExtensionInstalled(
        wixExe,
        wixDir,
        "WixToolset.UI.wixext");

      wixExtensionArgs =
        "-ext WixToolset.UI.wixext ";
    }

    string args =
        $"build Package.wxs Folders.wxs Components.wxs Package.fr-fr.wxl " +
        $"-arch {profile.Msi.Architecture} " +
        wixExtensionArgs +
        $"-o \"{msiPath}\"";

    int exitCode =
        RunProcess(
            wixExe,
            args,
            wixDir,
            Log
        );

    if (exitCode != 0)
      throw new InvalidOperationException(
          "wix build failed. Exit code: " + exitCode
      );

    Log("MSI generated : " + msiPath);
  }

  private void Log(string message) {
    LogChanged?.Invoke(
        $"[{DateTime.Now:HH:mm:ss}] {message}"
    );
  }

  private static int RunProcess(string fileName,string arguments,string workingDirectory,Action<string>? log) {
    ProcessStartInfo psi = new() {
      FileName = fileName,
      Arguments = arguments,
      WorkingDirectory = workingDirectory,
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
        log?.Invoke(e.Data);
    };

    process.ErrorDataReceived += (_,e) => {
      if (!string.IsNullOrWhiteSpace(e.Data))
        log?.Invoke("[ERROR] " + e.Data);
    };

    process.Start();

    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    process.WaitForExit();
    process.WaitForExit();

    return process.ExitCode;
  }

  private void ValidateGeneratedMsi(MsiPackageProfile profile) {
    string msiPath = GetMsiPath(profile);

    if (!File.Exists(msiPath))
      throw new InvalidOperationException(
          "MSI file was not generated: " + msiPath
      );

    FileInfo info = new(msiPath);

    if (info.Length == 0)
      throw new InvalidOperationException(
          "MSI file is empty: " + msiPath
      );

    Log("MSI validated.");
    Log("MSI size : " + FormatFileSize(info.Length));
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
}