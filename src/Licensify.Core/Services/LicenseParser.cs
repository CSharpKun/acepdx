using System.Text;
using Licensify.Core.Interfaces;

namespace Licensify.Core.Services;

public class LicenseParser(IConfigService configService) : ILicenseParser
{
    private StringBuilder Builder { get; } = new();

    public string Parse(IDataProvider dataProvider, string license)
    {
        var licenseSpan = license.AsSpan();
        
        var startPos = 0;
        var tagPos = 0;
        var endPos = 0;

        while (true) 
        {
            licenseSpan = licenseSpan[startPos..];
            tagPos = licenseSpan.IndexOf("<<");
            if (tagPos == -1) {
                Builder.Append(licenseSpan);
                break;
            } 
            
            Builder.Append(licenseSpan[startPos..tagPos]);
            
            startPos += tagPos;

            endPos = startPos + licenseSpan[startPos..].IndexOf(">>");
            if (endPos - startPos == -1) throw new FormatException($"Unexcepted unclosed tag at position {startPos}");
        
            switch (licenseSpan[(startPos+2)..endPos]) 
            {
                case "beginOptional":
                    ParseOptional(dataProvider, ref licenseSpan, endPos + 2, ref startPos);
                    break;

                case var n when n.StartsWith("var"):
                    //ParseVariable(licenseSpan, ref startPos);
                    break;
            }
        }

        

        return licenseSpan.ToString();
    }

    private void ParseOptional(IDataProvider dataProvider, ref ReadOnlySpan<char> licenseSpan, int tagEndPos, ref int cursor) 
    {
        var closingTagString = "<<endOptional>>";
        licenseSpan = licenseSpan[tagEndPos..];
        var closingTag = licenseSpan.IndexOf(closingTagString);
        var result = dataProvider.GetFlag(FlagQueryType.OptionalPart, licenseSpan[..closingTag].ToString());
        if (result) Builder.Append(licenseSpan[..closingTag]);
        licenseSpan = licenseSpan[(closingTag + closingTagString.Length)..];
    }

    private void ParseVariable(IDataProvider dataProvider, ref ReadOnlySpan<char> licenseSpan, ref int startPos) 
    {

    }
}