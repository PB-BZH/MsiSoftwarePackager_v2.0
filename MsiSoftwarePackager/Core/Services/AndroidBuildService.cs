using System.Diagnostics;
using System.Text;
using MsiSoftwarePackager.Core.Models;

namespace MsiSoftwarePackager.Core.Services;

internal class AndroidBuildService {

  public string BuildApk(MsiPackageProfile profile,Action<string>? log) {
    string projectFile = profile.TargetProject.ProjectFile;
    string projectDir = Path.GetDirectoryName(projectFile)!;

    string storePassword =
        WindowsCredentialManager.ReadPassword(profile.Android.KeystorePasswordCredentialTarget);

    string keyPassword =
        WindowsCredentialManager.ReadPassword(profile.Android.KeyPasswordCredentialTarget);

    if (string.IsNullOrWhiteSpace(storePassword))
      throw new InvalidOperationException("Android keystore password is empty.");

    if (string.IsNullOrWhiteSpace(keyPassword))
      throw new InvalidOperationException("Android key password is empty.");

    ProcessStartInfo psi = new() {
      FileName = "dotnet",
      WorkingDirectory = projectDir,
      UseShellExecute = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      CreateNoWindow = true,
      StandardOutputEncoding = Encoding.UTF8,
      StandardErrorEncoding = Encoding.UTF8
    };

    psi.ArgumentList.Add("publish");
    psi.ArgumentList.Add(projectFile);
    psi.ArgumentList.Add("-f");
    psi.ArgumentList.Add(profile.Android.TargetFramework);
    psi.ArgumentList.Add("-c");
    psi.ArgumentList.Add(profile.Android.Configuration);

    psi.ArgumentList.Add("-p:AndroidPackageFormat=apk");
    psi.ArgumentList.Add("-p:AndroidKeyStore=true");
    psi.ArgumentList.Add("-p:AndroidSigningKeyStore=" + profile.Android.KeystoreFile);
    psi.ArgumentList.Add("-p:AndroidSigningKeyAlias=" + profile.Android.KeyAlias);
    psi.ArgumentList.Add("-p:AndroidSigningKeyPass=" + keyPassword);
    psi.ArgumentList.Add("-p:AndroidSigningStorePass=" + storePassword);

    using Process process = new() { StartInfo = psi };

    process.OutputDataReceived += (_,e) => {
      if (!string.IsNullOrWhiteSpace(e.Data))
        log?.Invoke("[dotnet] " + e.Data);
    };

    process.ErrorDataReceived += (_,e) => {
      if (!string.IsNullOrWhiteSpace(e.Data))
        log?.Invoke("[dotnet ERROR] " + e.Data);
    };

    log?.Invoke("[INFO] Android APK publish started.");

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    process.WaitForExit();

    if (process.ExitCode != 0)
      throw new InvalidOperationException("Android APK publish failed. Exit code: " + process.ExitCode);

    string apkPath = FindGeneratedApk(projectDir,profile);

    profile.Android.ApkFilePath = apkPath;

    log?.Invoke("[OK] Android APK generated : " + apkPath);

    return apkPath;
  }

  private static string FindGeneratedApk(string projectDir,MsiPackageProfile profile) {
    string outputDir = Path.Combine(
        projectDir,
        "bin",
        profile.Android.Configuration,
        profile.Android.TargetFramework
    );

    string publishDir = Path.Combine(outputDir,"publish");

    string[] apkFiles = Directory.Exists(publishDir)
        ? Directory.GetFiles(publishDir,"*.apk",SearchOption.TopDirectoryOnly)
        : [];

    if (apkFiles.Length == 0 && Directory.Exists(outputDir)) {
      apkFiles = Directory.GetFiles(outputDir,"*.apk",SearchOption.TopDirectoryOnly);
    }

    if (apkFiles.Length == 0)
      throw new FileNotFoundException("Generated APK was not found.",outputDir);

    return apkFiles
        .OrderByDescending(File.GetLastWriteTimeUtc)
        .First();
  }
}
