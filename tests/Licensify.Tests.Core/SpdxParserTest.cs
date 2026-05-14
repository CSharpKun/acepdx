using Licensify.Core;
using Licensify.Core.Services;
using Licensify.Tests.Core.Mocks;

namespace Licensify.Tests.Core;

public class SpdxParserTest 
{
    private const string MitTemplate = """
    <<beginOptional>>MIT License<<endOptional>> 
    <<var;name="copyright";original="Copyright (c) <year> <copyright holders>  ";match=".{0,5000}">>
    Permission is hereby granted, free of charge, to any person obtaining a copy of <<var;name="files";original="this software and associated documentation files";match="this\s+software\s+and\s+associated\s+documentation\s+files|this\s+source\s+file">> (the "<<var;name="Software1";original="Software";match="Software|Materials">>"), to deal in the <<var;name="Software2";original="Software";match="Software|Materials">> without restriction, including without limitation<<beginOptional>> on<<endOptional>> the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the <<var;name="Software3";original="Software";match="Software|Materials">>, and to permit persons to whom the <<var;name="Software4";original="Software";match="Software|Materials">> is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice<<beginOptional>> (including the next paragraph)<<endOptional>> shall be included in all copies or substantial portions of the <<var;name="Software5";original="Software";match="Software|Materials">>.

    THE <<var;name="Software-verb";original="SOFTWARE IS";match="SOFTWARE IS|MATERIALS ARE">> PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL <<var;name="copyrightHolder";original="THE AUTHORS OR COPYRIGHT HOLDERS";match=".+">> BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE <<var;name="Software7";original="SOFTWARE";match="SOFTWARE|MATERIALS">> OR THE USE OR OTHER DEALINGS IN THE <<var;name="Software8";original="SOFTWARE";match="SOFTWARE|MATERIALS">>.
    """;

    private static License _license = new() {
        Name = "MIT License",
        LicenseId = "MIT",
        LicenseText = string.Empty,
        StandardLicenseTemplate = MitTemplate
    };

    [Fact]
    public void AppendOptionalParts() 
    {
        var mockData = new MockDataProvider() {
            AddOptional = true
        };
        var mockConfig = new MockConfig();
        var parser = new SpdxLicenseParser(mockConfig);

        var result = parser.Parse(mockData, _license);

        Assert.DoesNotContain("<<startOptional>>", result);
        Assert.DoesNotContain("<<endOptional>>", result);
        Assert.Contains("MIT License", result);
        Assert.Contains("(including the next paragraph)", result);
        Assert.Contains("including without limitation on the rights", result);
    }

    [Fact]
    public void RemoveOptionalParts() 
    {
        var mockData = new MockDataProvider() {
            AddOptional = false
        };
        var mockConfig = new MockConfig();
        var parser = new SpdxLicenseParser(mockConfig);

        var result = parser.Parse(mockData, _license);

        Assert.DoesNotContain("<<startOptional>>", result);
        Assert.DoesNotContain("<<endOptional>>", result);
        Assert.DoesNotContain("MIT License", result);
        Assert.DoesNotContain("(including the next paragraph)", result);
        Assert.DoesNotContain("including without limitation on the rights", result);
    } 
}