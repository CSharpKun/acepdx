using Licensify.Core.Models;

namespace Licensify.Core.Interfaces;

public interface ILicenseParser
{
    public string Parse(ISpdxTemplateProvider dataProvider, License license);
}