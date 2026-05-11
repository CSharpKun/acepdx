using System.Text;
using Licensify.Core.Interfaces;

namespace Licensify.Core.Services;

public class LicenseParser(IConfigService configService) : ILicenseParser
{
    private StringBuilder Builder { get; } = new();

    public string Parse(IDataProvider dataProvider, License license)
    {
        var licenseSpan = license.StandardLicenseTemplate.AsSpan();
        
        while (true) 
        {
            int tagPos = licenseSpan.IndexOf("<<");
            if (tagPos == -1) {
                Builder.Append(licenseSpan);
                break;
            } 
            
            Builder.Append(licenseSpan[..tagPos]);

            licenseSpan = licenseSpan[tagPos..];

            int endPos = licenseSpan.IndexOf(">>");
            if (endPos == -1) throw new FormatException($"Unexcepted unclosed tag at the end of the template.");
        
            switch (licenseSpan[2..endPos]) 
            {
                case "beginOptional":
                    ParseOptional(dataProvider, ref licenseSpan, endPos + 2);
                    break;

                case var n when n.StartsWith("var"):
                    ParseVariable(dataProvider, ref licenseSpan, endPos + 2);
                    break;
            }
        }

        

        return licenseSpan.ToString();
    }

    private void ParseOptional(IDataProvider dataProvider, ref ReadOnlySpan<char> licenseSpan, int tagEndPos) 
    {
        var closingTagString = "<<endOptional>>";
        licenseSpan = licenseSpan[tagEndPos..];
        var closingTag = licenseSpan.IndexOf(closingTagString);
        var result = dataProvider.GetFlag(FlagQueryType.OptionalPart, licenseSpan[..closingTag].ToString());
        if (result) Builder.Append(licenseSpan[..closingTag]);
        licenseSpan = licenseSpan[(closingTag + closingTagString.Length)..];
    }

    private void ParseVariable(IDataProvider dataProvider, ref ReadOnlySpan<char> licenseSpan, int tagEndPos) 
    {

    }
}