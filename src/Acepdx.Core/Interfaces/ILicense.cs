namespace Acepdx.Core.Interfaces;

public interface ILicense 
{
    public string LicenseId { get; }
    public bool? IsDeprecatedLicenseId { get; }
}