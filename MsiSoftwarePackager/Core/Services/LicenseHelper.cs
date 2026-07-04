using MsiSoftwarePackager.Core.Models;
using PB.BZH.Licensing.Core.Models;
using PB.BZH.Licensing.Core.Services;
using PB.BZH.Licensing.UI.Forms;

namespace MsiSoftwarePackager.Core.Services;

public static class LicenseHelper {
  public static bool TechnicalLicenseRequired {
    get {
#if TECHNICAL_LICENSE_REQUIRED
      return true;
#else
      return false;
#endif
    }
  }

  public static LicenseOptions ConstruireLicenseOptions(MsiPackageProfile profile) {
    string productId =
      !string.IsNullOrWhiteSpace(profile.Product.ProductId)
        ? profile.Product.ProductId
        : "_";

    string applicationFolder =
      !string.IsNullOrWhiteSpace(profile.Product.ProductFolder)
        ? profile.Product.ProductFolder
        : productId;

    return new LicenseOptions {
      ProductId = productId,
      ApplicationFolder = applicationFolder,
    };
  }

  public static LicenseService CreerLicenseService(MsiPackageProfile profile) {
    return new LicenseService(ConstruireLicenseOptions(profile));
  }

  public static bool VerifierLicenceAuDemarrage(MsiPackageProfile profile) {
    if (!TechnicalLicenseRequired) {
      return true;
    }
    LicenseService licenseService = CreerLicenseService(profile);
    return VerifierLicenceObligatoire(licenseService);
  }

  private static bool VerifierLicenceObligatoire(LicenseService licenseService) {
    LicenseValidationResult licenseResult = licenseService.ValidateInstalledLicense();
    if (licenseResult.IsValid) {
      return true;
    }
    using var activationForm = new LicenseActivationForm(
      licenseService,
      messageErreur: licenseResult.Message,
      activationObligatoire: true);
    if (activationForm.ShowDialog() != DialogResult.OK) {
      return false;
    }
    licenseResult = licenseService.ValidateInstalledLicense();
    return licenseResult.IsValid;
  }

  public static LicenseOptions ConstruireDisplayOptions(MsiPackageProfile profile) {
    return new LicenseOptions {
      LogoImage = profile.Product.LogoImage
    };
  }

  public static void AfficherLicence(IWin32Window owner,LicenseService licenseService,MsiPackageProfile profile) {

    LicenseValidationResult resultat = licenseService.ValidateInstalledLicense();

    if (!resultat.IsValid || resultat.License is null) {
      using var activationForm = new LicenseActivationForm(licenseService,resultat.Message,activationObligatoire: false);
      activationForm.ShowDialog(owner);
      return;
    }

    LicenseOptions displayOptions = ConstruireDisplayOptions(profile);

    using var formulaire = new LicenseInfoForm(resultat.License,displayOptions);
    formulaire.ShowDialog(owner);
  }

  public static void ImporterLicence(IWin32Window owner,LicenseService licenseService) {

    using var formulaire = new LicenseActivationForm(
      licenseService,
      messageErreur: "Importer une nouvelle licence.",
      activationObligatoire: false);

    formulaire.ShowDialog(owner);
  }
}