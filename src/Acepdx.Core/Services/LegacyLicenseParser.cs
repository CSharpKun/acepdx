using System.Text;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;

namespace Acepdx.Core.Services;

public class LegacyLicenseParser(IConfigService configService) : ILicenseParser
{
    private ITemplateProvider Provider { get; set; } = null!;
    private StringBuilder Builder { get; set; } = new();

    public bool CanParse(License license) => license.StandardLicenseTemplate is not null;

    public string Parse(ITemplateProvider dataProvider, License license)
    {
        Builder = new();
        Provider = dataProvider;
        var licenseSpan = license.StandardLicenseTemplate.AsSpan();

        while (true)
        {
            int tagPos = licenseSpan.IndexOf("<<");
            if (tagPos == -1)
            {
                Builder.Append(licenseSpan);
                break;
            }

            Builder.Append(licenseSpan[..tagPos]);

            licenseSpan = licenseSpan[tagPos..];

            int endPos = licenseSpan.IndexOf(">>");
            if (endPos == -1)
                throw new FormatException($"Unexcepted unclosed tag at the end of the template.");

            switch (licenseSpan[2..endPos])
            {
                case "beginOptional":
                    ParseOptional(ref licenseSpan, endPos + 2);
                    break;

                case var n when n.StartsWith("var"):
                    ParseVariable(ref licenseSpan, endPos + 2);
                    break;
            }
        }

        return Builder.ToString();
    }

    private void ParseOptional(ref ReadOnlySpan<char> licenseSpan, int tagEndPos)
    {
        var closingTagString = "<<endOptional>>";
        licenseSpan = licenseSpan[tagEndPos..];
        var closingTag = licenseSpan.IndexOf(closingTagString);
        var result = Provider.GetOptional(licenseSpan[..closingTag]);
        if (result)
            Builder.Append(licenseSpan[..closingTag]);
        licenseSpan = licenseSpan[(closingTag + closingTagString.Length)..];
    }

    private void ParseVariable(ref ReadOnlySpan<char> licenseSpan, int tagEndPos)
    {
        var nextField = licenseSpan.IndexOf(';');

        licenseSpan = licenseSpan[tagEndPos..];
    }
}
