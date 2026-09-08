using System.Text;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;

namespace Acepdx.Core.Licensing;

public class LegacyLicenseParser(IConfigService configService) : ILicenseParser
{
    private readonly StringBuilder _builder = new();

    public bool CanParse(License license) => license.StandardLicenseTemplate is not null;

    public string Parse(ITemplateProvider dataProvider, License license)
    {
        _builder.Clear();
        var licenseSpan = license.StandardLicenseTemplate.AsSpan();

        while (true)
        {
            int tagPos = licenseSpan.IndexOf("<<");
            if (tagPos == -1)
            {
                _builder.Append(licenseSpan);
                break;
            }

            _builder.Append(licenseSpan[..tagPos]);

            licenseSpan = licenseSpan[tagPos..];

            var secondTagPos = licenseSpan.IndexOf("<<");

            int endPos = licenseSpan.IndexOf(">>");
            if (endPos == -1)
            {
                throw new FormatException($"Unexpected unclosed tag at the end of the template.");
            }

            if (secondTagPos > endPos)
            {
                throw new FormatException($"Unexpected unclosed tag.");
            }

            switch (licenseSpan[2..endPos])
            {
                case "beginOptional":
                    ParseOptional(ref licenseSpan, endPos + 2, dataProvider);
                    break;

                case var n when n.StartsWith("var"):
                    ParseVariable(ref licenseSpan, endPos + 2, dataProvider);
                    break;
            }
        }

        return _builder.ToString();
    }

    private void ParseOptional(ref ReadOnlySpan<char> licenseSpan, int tagEndPos, ITemplateProvider templateProvider)
    {
        var closingTagString = "<<endOptional>>";
        licenseSpan = licenseSpan[tagEndPos..];
        var closingTag = licenseSpan.IndexOf(closingTagString);
        var result = templateProvider.GetOptional(licenseSpan[..closingTag]);
        if (result)
            _builder.Append(licenseSpan[..closingTag]);
        licenseSpan = licenseSpan[(closingTag + closingTagString.Length)..];
    }

    private void ParseVariable(ref ReadOnlySpan<char> licenseSpan, int tagEndPos, ITemplateProvider templateProvider)
    {
        var nextField = licenseSpan.IndexOf(';');

        licenseSpan = licenseSpan[tagEndPos..];
    }
}
