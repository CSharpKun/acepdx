using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nerdbank.MessagePack;
using PolyType;

namespace Acepdx.Core.Services;

public class MessagePackLicenseCacher(IFileSystem fileSystem, ILogger<MessagePackLicenseCacher>? logger = null)// : ILicenseCacheService
{
    public List<LicenseList> CachedLists { get; set; } = [];
    public List<License> CachedLicenses { get; set; } = [];

    private readonly ILogger<MessagePackLicenseCacher> _logger = logger ?? NullLogger<MessagePackLicenseCacher>.Instance;
    private readonly MessagePackSerializer _serializer = new();

    private void WriteToCache<T>(T cache, string id) where T : IShapeable<T> 
    {
        var bytes = _serializer.Serialize(cache);
        var filePath = Path.Combine(
            AcepdxFolders.Cache,
            $"{typeof(T).Name}_{id}"
        );
        fileSystem.File.WriteAllBytes(filePath, bytes);
    }

    private bool TryGetFromCache<T>(string id, [MaybeNull] out T? result) where T : IShapeable<T>
    {
        var filePath = Path.Combine(
            AcepdxFolders.Cache,
            $"{typeof(T).Name}_{id}"
        );

        if (!fileSystem.File.Exists(filePath)) 
        {
            result = default;
            _logger.LogError("File {Path} does not exist", filePath);
            return false;
        }

        var isFileOld = fileSystem.File.GetLastWriteTime(filePath) < DateTime.Now - TimeSpan.FromHours(10);
        
        if (isFileOld)
        {
            result = default;
            _logger.LogInformation("Cache outdated, returning null");
            return false;
        }

        var bytes = fileSystem.File.ReadAllBytes(filePath);

        result = _serializer.Deserialize<T>(bytes);
        
        if (result is null) 
        {
            _logger.LogError("Couldn't parse cached data for file {Path}", filePath);
            return false;
        } 

        return true;
    }
}