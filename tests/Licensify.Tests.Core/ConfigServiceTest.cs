using System.IO.Abstractions.TestingHelpers;
using Licensify.Core.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace Licensify.Tests.Core;

public class TomlConfigServiceTest
{
    private static string ConfigPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "licensify",
        "config.toml"
    );

    [Fact]
    public void SerializationTest()
    {
        var fileSystem = new MockFileSystem();
        var logger = new NullLogger<TomlConfig>();
        var configService = new TomlConfig(fileSystem, logger);

        configService.Settings["user.name"] = "John";
        configService.Remotes["spdx"] = new() { Url = "https://spdx.org/licenses/licenses.json" };
        configService.Remotes["example"] = new() { Url = "https://example.org/licenses" };

        configService.Save();

        var content = fileSystem.File.ReadAllText(ConfigPath);
        
        Assert.Contains("spdx", content);
        Assert.Contains("https://spdx.org/licenses/licenses.json", content);
        Assert.Contains("example", content);
        Assert.Contains("https://example.org/licenses", content);
        Assert.Contains("user", content);
        Assert.Contains("name", content);
        Assert.Contains("John", content);
    }

    [Fact]
    public void SerializationAndDeserializationTest()
    {
        var fileSystem = new MockFileSystem();
        var logger = new NullLogger<TomlConfig>();
        var configService = new TomlConfig(fileSystem, logger);

        configService.Settings["user.name"] = "John";
        configService.Settings["remote.spdx.url"] = "https://spdx.org/licenses/licenses.json";
        configService.Settings["remote.example.url"] = "https://example.org/licenses";
        configService.Remotes["spdx"] = new() { Url = "https://spdx.org/licenses/licenses.json" };
        configService.Remotes["example"] = new() { Url = "https://example.org/licenses" };

        configService.Save();

        var otherConfigService = new TomlConfig(fileSystem, logger);

        Assert.NotEmpty(configService.Settings);
        Assert.NotEmpty(otherConfigService.Settings);
        Assert.NotEmpty(configService.Remotes);
        Assert.NotEmpty(otherConfigService.Remotes);

        Assert.Equal(configService.Settings, otherConfigService.Settings);
        Assert.Equal(configService.Remotes.Keys.OrderBy(k => k), otherConfigService.Remotes.Keys.OrderBy(k => k));
        
        foreach (var key in configService.Remotes.Keys)
        {
            Assert.Equal(configService.Remotes[key].Url, otherConfigService.Remotes[key].Url);
        }
    }
}