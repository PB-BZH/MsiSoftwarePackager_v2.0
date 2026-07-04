using MsiSoftwarePackager.Core.Models;
using MsiSoftwarePackager.Core.Services;

namespace MsiSoftwarePackager.UI;

internal static class Program {
  private static MsiPackageProfile _profile = new();

  [STAThread]
  static void Main() {
    ApplicationConfiguration.Initialize();

    if (!LicenseHelper.VerifierLicenceAuDemarrage(_profile)) {
      return;
    }

    Application.Run(new MainForm());
  }
}