namespace Licensify.Core.Interfaces;

public interface ILicenseParser
{
    public string Parse(IDataProvider dataProvider, string license);
}