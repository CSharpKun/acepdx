namespace Licensify.Core.Interfaces;

public interface ILicenseHttpService 
{
    public Task<List<LicenseList>> GetLicenseLists(CancellationToken token = default);
    public Task<License?> GetLicense(LicenseListEntry licenseEntry, CancellationToken token = default);
}