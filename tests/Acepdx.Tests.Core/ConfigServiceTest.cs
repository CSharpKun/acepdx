using System.IO.Abstractions.TestingHelpers;
using Acepdx.Core;
using Acepdx.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Acepdx.Tests.Core;

public class TomlConfigServiceTest
{
    [Fact]
    public void SerializationTest()
    {
        var fileSystem = new MockFileSystem();
        var folders = new AcepdxFolders(fileSystem);
        var logger = new Mock<ILogger<TomlConfig>>();
        var configService = new TomlConfig(fileSystem, folders, logger.Object);

        configService.Settings["user.name"] = "John";
        configService.Remotes["spdx"] = new()
        {
            Url = new("https://spdx.org/licenses/licenses.json"),
        };
        configService.Remotes["example"] = new() { Url = new("https://example.org/licenses") };

        configService.Save();

        var content = fileSystem.File.ReadAllText(Path.Combine(folders.Config, "config.toml"));

        Assert.Contains("spdx", content);
        Assert.Contains("https://spdx.org/licenses/licenses.json", content);
        Assert.Contains("example", content);
        Assert.Contains("https://example.org/licenses", content);
        Assert.Contains("user", content);
        Assert.Contains("name", content);
        Assert.Contains("John", content);
    }

    [Fact]
    public void CommentariesAndDeserializationTest()
    {
        var fileSystem = new MockFileSystem();
        var folders = new AcepdxFolders(fileSystem);
        var configPath = Path.Combine(folders.Config, "config.toml");
        var logger = new Mock<ILogger<TomlConfig>>();

        fileSystem.AddFile(
            configPath,
            new(
                """
                [user]
                # Some comments here
                name = "John"

                [remote.spdx]
                # Just for fun
                url = "https://spdx.org/licenses/licenses.json"
                # It won't work, will it?

                [remote.example]
                url = "https://example.org/licenses"
                """
            )
        );

        var configService = new TomlConfig(fileSystem, folders, logger.Object);

        configService.Save();

        var content = fileSystem.File.ReadAllText(configPath);

        Assert.Contains("spdx", content);
        Assert.Contains("https://spdx.org/licenses/licenses.json", content);
        Assert.Contains("example", content);
        Assert.Contains("https://example.org/licenses", content);
        Assert.Contains("user", content);
        Assert.Contains("name", content);
        Assert.Contains("John", content);
        Assert.Contains("# Some comments here", content);
        Assert.Contains("# Just for fun", content);
        Assert.Contains("# It won't work, will it?", content);
    }

    [Fact]
    public void SerializationAndDeserializationTest()
    {
        var fileSystem = new MockFileSystem();
        var folders = new AcepdxFolders(fileSystem);
        var logger = new Mock<ILogger<TomlConfig>>();
        var configService = new TomlConfig(fileSystem, folders, logger.Object);

        configService.Settings["user.name"] = "John";
        configService.Remotes["spdx"] = new()
        {
            Url = new("https://spdx.org/licenses/licenses.json"),
        };
        configService.Remotes["example"] = new() { Url = new("https://example.org/licenses") };

        configService.Save();

        var otherConfigService = new TomlConfig(fileSystem, folders, logger.Object);

        Assert.NotEmpty(configService.Settings);
        Assert.NotEmpty(otherConfigService.Settings);
        Assert.NotEmpty(configService.Remotes);
        Assert.NotEmpty(otherConfigService.Remotes);

        Assert.Equal(configService.Settings, otherConfigService.Settings);
        Assert.Equal(
            configService.Remotes.Keys.OrderBy(k => k),
            otherConfigService.Remotes.Keys.OrderBy(k => k)
        );

        Assert.Equal(
            configService.Remotes.Keys.OrderBy(k => k),
            otherConfigService.Remotes.Keys.OrderBy(k => k)
        );

        foreach (var key in configService.Remotes.Keys)
        {
            Assert.Equal(configService.Remotes[key].Url, otherConfigService.Remotes[key].Url);
        }
    }
}
