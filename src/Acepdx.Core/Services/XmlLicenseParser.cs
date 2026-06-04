using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;

namespace Acepdx.Core.Services;

public class XmlLicenseParser(IConfigService config) : ILicenseParser
{
    public bool CanParse(License license) => license.LicenseXml is not null;

    public string Parse(ITemplateProvider dataProvider, License license)
    {
        throw new NotImplementedException();
    }
}
