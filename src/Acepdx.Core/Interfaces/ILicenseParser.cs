using Acepdx.Core.Models;

namespace Acepdx.Core.Interfaces;

public interface ILicenseParser
{
    public string Parse(ISpdxTemplateProvider dataProvider, License license);
}