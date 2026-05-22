using System.Runtime.InteropServices;

namespace Acepdx.Core;

public static class AcepdxFolders
{
    private const string AcepdxName = "acepdx";

    public static string Config { get; } 
    public static string Cache { get; }

    static AcepdxFolders()
    {
        string cacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) 
        {
            var xdgCache = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
            if (!string.IsNullOrEmpty(xdgCache)) 
            {
                cacheFolder = xdgCache;    
            }
            else 
            {
                cacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
            }
        }

        Config = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AcepdxName);
        Cache = Path.Combine(cacheFolder, AcepdxName);

        if (!Directory.Exists(Config))
            Directory.CreateDirectory(Config);
            
        if (!Directory.Exists(Cache))
            Directory.CreateDirectory(Config);

        return;
    }
}