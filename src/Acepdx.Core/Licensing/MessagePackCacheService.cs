using System.IO.Abstractions;
using System.Text;

using Acepdx.Core.Interfaces;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Nerdbank.MessagePack;

using PolyType;

namespace Acepdx.Core.Licensing;

public sealed partial class MessagePackCacheService(IFileSystem fileSystem, IFolders folders, ILogger<MessagePackCacheService>? logger = null) : ICacheProvider
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Cache for key {Key} is not found")]
    private partial void LogKeyNotFound(string key);

    private readonly ILogger<MessagePackCacheService> _logger = logger ?? NullLogger<MessagePackCacheService>.Instance;
    private readonly MessagePackSerializer _serializer = new();

    public async Task<T?> GetAsync<T>(string key, CancellationToken token = default) where T : class
    {
        var path = GetNormalizedPath(key);

        try
        {
            using var stream = fileSystem.File.OpenRead(path);
            return await _serializer.DeserializeAsync<T>(stream, cancellationToken: token);
        }
        catch (FileNotFoundException)
        {
            LogKeyNotFound(key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken token = default) where T : class
    {
        var path = GetNormalizedPath(key);
        using var stream = fileSystem.File.OpenWrite(path);
        await _serializer.SerializeAsync<T>(stream, in value, cancellationToken: token);
    }

    private string GetNormalizedPath(string key)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(key.Length);

        foreach (char c in key)
        {
            if (!invalidChars.Contains(c))
                sb.Append(c);
        }

        return Path.Combine(folders.Cache, sb.ToString());
    }
}
