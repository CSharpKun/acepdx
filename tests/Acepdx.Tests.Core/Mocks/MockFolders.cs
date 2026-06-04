using System.IO.Abstractions;
using Acepdx.Core.Interfaces;

namespace Acepdx.Tests.Core.Mocks;

public class MockFolders : IFolders
{
    public string Config { get; } = "/config/";
    public string Cache { get; } = "/cache/";

    public MockFolders(IFileSystem fileSystem)
    {
        if (!fileSystem.Directory.Exists(Config))
            fileSystem.Directory.CreateDirectory(Config);

        if (!fileSystem.Directory.Exists(Cache))
            fileSystem.Directory.CreateDirectory(Cache);
    }
}
