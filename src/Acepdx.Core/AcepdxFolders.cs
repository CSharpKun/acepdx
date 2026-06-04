using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Acepdx.Core.Interfaces;

namespace Acepdx.Core;

public class AcepdxFolders : IFolders
{
    private const string AcepdxName = "acepdx";

    public string Config { get; }
    public string Cache { get; }

    public AcepdxFolders(IFileSystem fileSystem)
    {
        string cacheFolder = Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData
        );

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var xdgCache = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
            if (!string.IsNullOrEmpty(xdgCache))
            {
                cacheFolder = xdgCache;
            }
            else
            {
                cacheFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".cache"
                );
            }
        }

        Config = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            AcepdxName
        );
        Cache = Path.Combine(cacheFolder, AcepdxName);

        if (!fileSystem.Directory.Exists(Config))
            fileSystem.Directory.CreateDirectory(Config);

        if (!fileSystem.Directory.Exists(Cache))
            fileSystem.Directory.CreateDirectory(Cache);

        return;
    }
}
