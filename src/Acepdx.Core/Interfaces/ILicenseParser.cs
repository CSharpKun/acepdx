using System.Diagnostics.CodeAnalysis;
using Acepdx.Core.Models;

namespace Acepdx.Core.Interfaces;

public interface ILicenseParser
{
    public string Parse(ITemplateProvider dataProvider, License license);
    bool CanParse(License license);
}
