using MsiSoftwarePackager.Core.Models;
using PB.BZH.Help.Library.Core.Profiles;
using PB.BZH.Help.Library.Core.Services;
using PB.BZH.Help.Library.UI.Forms;

namespace MsiSoftwarePackager.Core.Services;

public class HelpHelper {

  public static ApplicationInfoProfile ConstruireApplicationInfoProfile(MsiPackageProfile profile) {
    return new ApplicationInfoProfile {
      Product = new ProductInfo {
        ProductName = profile.Product.ProductName,
        Manufacturer = profile.Product.Manufacturer,
        Version = profile.Product.Version,
        Description = profile.Product.Description,
        UpgradeCode = profile.Product.UpgradeCode,
        IconPath = profile.Product.IconPath,
        LogoPath = profile.Product.LogoImage,
        DownloadPageUrl = profile.Product.DownloadPageUrl,
        PrivacyPageUrl = profile.Product.PrivacyPageUrl,
        Copyright = profile.Product.Copyright,
        EmailContact = profile.Product.EmailContact,
        ProductId = profile.Product.ProductId
      },
      Update = new UpdateInfo {
        ProductName = profile.UpdateManifest.ProductName,
        Version = profile.UpdateManifest.Version,
        Publisher = profile.UpdateManifest.Publisher,
        DownloadPage = profile.UpdateManifest.DownloadPage,
        PrivacyPage = profile.UpdateManifest.PrivacyPage,
        MsiUrl = profile.UpdateManifest.MsiUrl,
        WebSetupUrl = profile.UpdateManifest.WebSetupUrl,
        UpdateManifestUrl = profile.UpdateManifest.UpdateManifestUrl,
        ReleaseDate = profile.UpdateManifest.ReleaseDate,
        ApplicationId = profile.UpdateManifest.ApplicationId
      }
    };
  }

  public static void mnuAbout(IWin32Window owner,MsiPackageProfile profile) {
    ApplicationInfoProfile aboutProfile = ConstruireApplicationInfoProfile(profile);

    using AboutForm aboutForm = new(aboutProfile);

    aboutForm.ShowDialog(owner);
  }

  public static async void mnuCheckForUpdates(IWin32Window owner,MsiPackageProfile profile) {
    UpdateChecker.UpdateCheckResult result = await UpdateChecker.GetUpdateStatusAsync();
    ApplicationInfoProfile checkUpdate = ConstruireApplicationInfoProfile(profile);

    using UpdateForm updateForm = new(result,checkUpdate);

    updateForm.ShowDialog(owner);
  }
}
