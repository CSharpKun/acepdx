using System.Diagnostics;
using Licensify.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Licensify.Tests.Core;

public class YamlConfigServiceTest
{
    [Fact]
    public void SerializationTest()
    {
        var fileService = new MockFileService(null, "");
        var logger = new NullLogger<YamlConfigService>();
        var configService = new YamlConfigService(fileService, logger)
        {
            SpdxRemotes = new()
            {
                ["spdx"] = new()
                {
                    Url = new("https://spdx.org/licenses/licenses.json")
                },
                ["example"] = new()
                {
                    Url = new("https://example.org/licenses")
                }
            }
        };

        configService.UpdateSettings();

        Assert.NotNull(fileService.FileString);
        Assert.Contains("spdx", fileService.FileString);
        Assert.Contains("https://spdx.org/licenses/licenses.json", fileService.FileString);
        Assert.Contains("example", fileService.FileString);
        Assert.Contains("https://example.org/licenses", fileService.FileString);
    }

    [Fact]
    public void SerializationAndDeserializationTest()
    {
        var fileService = new MockFileService(null, "");
        var logger = new NullLogger<YamlConfigService>();
        var configService = new YamlConfigService(fileService, logger)
        {
            SpdxRemotes = new()
            {
                ["spdx"] = new()
                {
                    Url = new("https://spdx.org/licenses/licenses.json")
                },
                ["example"] = new()
                {
                    Url = new("https://example.org/licenses")
                }
            },
            Settings = new()
            {
                ["user"] = new Dictionary<string, string>()
                {
                    ["name"] = "John",
                    ["surname"] = "Doe"
                }
            }
        };

        configService.UpdateSettings();
        var otherConfigService = new YamlConfigService(fileService, logger);

        Assert.Equal(
            configService.Settings.Keys.OrderBy(k => k),
            otherConfigService.Settings.Keys.OrderBy(k => k)
        );

        Assert.Equal(otherConfigService.SpdxRemotes, configService.SpdxRemotes);

        foreach (var key in configService.Settings.Keys)
        {
            Assert.Equal(configService.Settings[key], otherConfigService.Settings[key]);
        }
    }
}
