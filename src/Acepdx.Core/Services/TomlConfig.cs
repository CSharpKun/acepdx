using System.IO.Abstractions;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tomlyn;
using Tomlyn.Model;
using Tomlyn.Serialization;

namespace Acepdx.Core.Services;

public class TomlConfig : IConfigService
{
    public Dictionary<string, SpdxRemote> Remotes { get; set; } = [];
    public Dictionary<string, string> Settings { get; set; } = [];

    private readonly IFileSystem _fileSystem;
    private readonly ILogger<TomlConfig> _logger;

    private readonly string _configPath;
    private readonly TomlMetadataStore? _lastMetadata = new();
    private readonly TomlSerializerOptions _tomlOptions;

    public TomlConfig(IFileSystem fileSystem, IFolders folders, ILogger<TomlConfig>? logger = null)
    {
        _fileSystem = fileSystem;
        _logger = logger ?? NullLogger<TomlConfig>.Instance;
        _configPath = Path.Combine(folders.Config, "config.toml");
        _tomlOptions = new() { MetadataStore = _lastMetadata };

        if (!_fileSystem.File.Exists(_configPath))
        {
            Settings = [];
            Remotes = [];
            Save();
            return;
        }

        Load();
    }

    public void Load()
    {
        using var stream = _fileSystem.File.OpenText(_configPath);

        var root = TomlSerializer.Deserialize<TomlTable>(stream, _tomlOptions);

        if (root is null)
            return;

        Settings = FlattenToml(root);

        if (
            !root.TryGetValue("remote", out var remoteNode)
            || remoteNode is not TomlTable remoteTable
        )
            return;

        foreach (var (key, value) in remoteTable)
        {
            if (
                value is not TomlTable valueTable
                || !valueTable.TryGetValue(nameof(SpdxRemote.Url).ToLower(), out var url)
            )
                continue;

            if (!Uri.TryCreate(url as string, UriKind.Absolute, out var typedUrl))
            {
                _logger.LogWarning(
                    "Url {Url} of remote {Remote} is incorrectly formatted - skipping",
                    url,
                    key
                );
                continue;
            }

            Remotes[key] = new() { Url = typedUrl };
        }
    }

    public static Dictionary<string, string> FlattenToml(TomlTable table, string prefix = "")
    {
        var result = new Dictionary<string, string>();

        foreach (var (key, child) in table)
        {
            if (key == "remote" && prefix == string.Empty)
                continue;

            var newKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";

            if (child is TomlTable childTable)
            {
                foreach (var kv in FlattenToml(childTable, newKey))
                    result[kv.Key] = kv.Value;
            }
            else if (child is string stringChild)
            {
                result[newKey] = stringChild;
            }
        }

        return result;
    }

    public void Save()
    {
        var table = new TomlTable();

        foreach (var kwp in Settings)
        {
            var keys = kwp.Key.Split('.');
            TomlTable currentTable = table;

            if (keys.Length < 2)
            {
                continue;
            }

            foreach (var key in keys[..^1])
            {
                if (currentTable.TryGetValue(key, out var node) && node is TomlTable tomlTable)
                {
                    currentTable = tomlTable;
                    continue;
                }
                currentTable[key] = new TomlTable();
                currentTable = (TomlTable)currentTable[key];
            }

            currentTable[keys[^1]] = kwp.Value;
        }

        if (!table.TryGetValue("remote", out var remotesTable))
        {
            table["remote"] = new TomlTable();
            remotesTable = table["remote"];
        }

        foreach (var remote in Remotes)
        {
            ((TomlTable)remotesTable)[remote.Key] = new TomlTable()
            {
                [nameof(SpdxRemote.Url).ToLower()] = remote.Value.Url.AbsoluteUri,
            };
        }

        using var stream = _fileSystem.File.CreateText(_configPath);
        TomlSerializer.Serialize(stream, table, _tomlOptions);
    }
}
