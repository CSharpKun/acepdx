using Acepdx.Core.Models;

namespace Acepdx.Core.Interfaces;

public interface ILicenseHttpService 
{
    public Task<List<LicenseList>> GetLicenseLists(CancellationToken token = default);
    public Task<License?> GetLicense(LicenseListEntry licenseEntry, CancellationToken token = default);
}