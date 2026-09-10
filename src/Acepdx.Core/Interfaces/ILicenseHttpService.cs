using Acepdx.Core.Models;

namespace Acepdx.Core.Interfaces;

public interface ILicenseHttpService
{
    public Task<LicenseList[]> GetLicenseLists(bool noCache = false, CancellationToken token = default);
    public Task<License?> GetLicense(
        LicenseListEntry licenseEntry,
        bool noCache = false,
        CancellationToken token = default
    );
}
