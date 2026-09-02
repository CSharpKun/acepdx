using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualBasic;

namespace Acepdx.Core.Services;

[UnconditionalSuppressMessage(
    "Trimming",
    "IL2026",
    Justification = "JsonSerializerContext provided in JsonOptions"
)]
[UnconditionalSuppressMessage(
    "AOT",
    "IL3050",
    Justification = "JsonSerializerContext provided in JsonOptions"
)]
public sealed partial class JsonConfig : IConfigService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<JsonConfig> _logger;

    private readonly JsonNode _rootNode;

    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private readonly string _configPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        TypeInfoResolver = AcepdxJsonSerializerContext.Default,
    };

    public JsonConfig(IFileSystem fileSystem, IFolders folders, ILogger<JsonConfig>? logger = null)
    {
        _fileSystem = fileSystem;
        _logger = logger ?? NullLogger<JsonConfig>.Instance;
        _configPath = Path.Combine(folders.Config, "config.json");

        if (!_fileSystem.File.Exists(_configPath))
        {
            _rootNode = new JsonObject();
            return;
        }

        using var stream = _fileSystem.File.OpenRead(_configPath);

        var root = JsonNode.Parse(stream);

        if (root is null)
        {
            _rootNode = new JsonObject();
            return;
        }

        _rootNode = root;
    }

    public async Task<T?> Get<T>(string path, T? defaultValue = default)
    {
        await _semaphore.WaitAsync();
        try
        {
            var keys = path.Split('.');
            JsonNode? current = _rootNode;

            foreach (var k in keys)
            {
                if (current is not JsonObject obj) return defaultValue;
                if (!obj.TryGetPropertyValue(k, out current)) return defaultValue;
            }

            return current.Deserialize<T>(JsonOptions) ?? defaultValue;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task Set<T>(string path, T value)
    {
        await _semaphore.WaitAsync();
        try
        {
            var keys = path.Split('.');
            JsonNode current = _rootNode;

            for (int i = 0; i < keys.Length - 1; i++)
            {
                var currentObj = current as JsonObject ?? throw new InvalidOperationException();
                if (!currentObj.TryGetPropertyValue(keys[i], out var next) || next is not JsonObject)
                {
                    next = new JsonObject();
                    currentObj[keys[i]] = next;
                }
                current = next;
            }

            var lastObj = current as JsonObject ?? throw new InvalidOperationException();
            lastObj[keys[^1]] = JsonNode.Parse(
                JsonSerializer.Serialize(value, JsonOptions)
            );

            await Save();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task Unset(string path)
    {
        await _semaphore.WaitAsync();
        try
        {
            var keys = path.Split('.');

            if (keys.Length == 1)
            {
                if (_rootNode is JsonObject rootObj)
                {
                    rootObj.Remove(keys[0]);
                    await Save();
                }
                return;
            }

            JsonNode? current = _rootNode;
            for (int i = 0; i < keys.Length - 1; i++)
            {
                if (current is not JsonObject obj)
                    throw new KeyNotFoundException();

                if (!obj.TryGetPropertyValue(keys[i], out current))
                    throw new KeyNotFoundException();
            }

            if (current is JsonObject parentObj)
            {
                parentObj.Remove(keys[^1]);
                await Save();
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task Save(CancellationToken token = default)
    {
        var json = _rootNode.ToJsonString();
        await _fileSystem.File.WriteAllTextAsync(_configPath, json, token);
    }
}