using System.IO.Abstractions.TestingHelpers;

using Acepdx.Core;
using Acepdx.Core.Models;
using Acepdx.Core.Licensing;

using Microsoft.Extensions.Logging;

namespace Acepdx.Tests.Core;

public class JsonConfigServiceTest
{
    [Test]
    public async Task GivenBaseElements_WhenConfigSerialized_ThenFileWrittenCorrectly()
    {
        var fileSystem = new MockFileSystem();
        var folders = new AcepdxFolders(fileSystem);
        var logger = Mock.Of<ILogger<JsonConfig>>();
        var configService = await JsonConfig.LoadConfig(fileSystem, folders, logger.Object);


        await configService.Set("user.name", "John");
        await configService.Set<SpdxRemote>("remote.spdx", new()
        {
            Url = new("https://spdx.org/licenses/licenses.json"),
        });

        await configService.Set<SpdxRemote>("remote.example", new()
        {
            Url = new("https://example.org/licenses"),
        });

        var content = fileSystem.File.ReadAllText(Path.Combine(folders.Config, "config.json"));


        await Assert.That(content).Contains("spdx")
            .And.Contains("https://spdx.org/licenses/licenses.json")
            .And.Contains("example")
            .And.Contains("https://example.org/licenses")
            .And.Contains("user")
            .And.Contains("name")
            .And.Contains("John");
    }

    [Test]
    public async Task GivenBaseElements_WhenConfigSerializedAndDeserialized_ThenDataConsistent()
    {
        var fileSystem = new MockFileSystem();
        var folders = new AcepdxFolders(fileSystem);
        var logger = Mock.Of<ILogger<JsonConfig>>();
        var configService = await JsonConfig.LoadConfig(fileSystem, folders, logger.Object);

        await configService.Set("user.name", "John");
        await configService.Set<SpdxRemote>("remote.spdx", new()
        {
            Url = new("https://spdx.org/licenses/licenses.json"),
        });

        await configService.Set<SpdxRemote>("remote.example", new()
        {
            Url = new("https://example.org/licenses"),
        });

        var otherConfigService = await JsonConfig.LoadConfig(fileSystem, folders, logger.Object);

        var originalRemotes = await configService.Get<Dictionary<string, SpdxRemote>>("remote");
        var targetRemotes = await otherConfigService.Get<Dictionary<string, SpdxRemote>>("remote");

        var originalUsername = await configService.Get<string>("user.name");
        var targetUsername = await otherConfigService.Get<string>("user.name");

        await Assert.That(originalRemotes).IsNotNull();
        await Assert.That(targetRemotes).IsNotNull();

        await Assert.That(targetUsername).IsEqualTo(originalUsername);

        foreach (var key in originalRemotes.Keys)
        {
            await Assert.That(targetRemotes[key]).IsEqualTo(originalRemotes[key]);
        }
    }
}
