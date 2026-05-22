using System.IO.Abstractions;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tommy;

namespace Acepdx.Core.Services;

public class TomlConfig : IConfigService
{
    public Dictionary<string, SpdxRemote> Remotes { get; set; } = [];
    public Dictionary<string, string> Settings { get; set; } = [];

    private readonly IFileSystem _fileSystem;
    private readonly ILogger<TomlConfig> _logger;

    private static readonly string _configPath = Path.Combine(
        AcepdxFolders.Config,
        "config.toml"
    );

    public TomlConfig(IFileSystem fileSystem, ILogger<TomlConfig>? logger = null)
    {
        _fileSystem = fileSystem;
        _logger = logger ?? NullLogger<TomlConfig>.Instance;

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

        var root = TOML.Parse(stream);

        Settings = FlattenToml(root);

        if (!root.TryGetNode("remote", out var remoteNode) || remoteNode is not TomlTable remoteTable) return;

        foreach (var kwp in remoteTable.RawTable)
        {
            if (kwp.Value is not TomlTable valueTable || !valueTable.TryGetNode(nameof(SpdxRemote.Url).ToLower(), out var url)) continue;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var typedUrl))
            {
                _logger.LogWarning("Url {Url} of remote {Remote} is incorrectly formatted - skipping", url, kwp.Key);
                continue;
            }

            Remotes[kwp.Key] = new()
            {
                Url = typedUrl
            };
        }
    }

    public static Dictionary<string, string> FlattenToml(TomlTable table, string prefix = "")
    {
        var result = new Dictionary<string, string>();

        foreach (var (key, child) in table.RawTable)
        {
            if (key == "remote" && prefix == string.Empty) continue;

            var newKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}.{key}";

            if (child is TomlTable childTable)
            {
                foreach (var kv in FlattenToml(childTable, newKey))
                    result[kv.Key] = kv.Value;
            }
            else if (child is not TomlArray)
            {
                result[newKey] = child;
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
                if (currentTable.TryGetNode(key, out var node) && node is TomlTable tomlTable)
                {
                    currentTable = tomlTable;
                    continue;
                }
                currentTable[key] = new TomlTable(); 
                currentTable = (TomlTable)currentTable[key];
            }

            currentTable[keys[^1]] = kwp.Value;
        }

        if (!table.TryGetNode("remote", out var remotesTable))
        {
            table["remote"] = new TomlTable();
            remotesTable = table["remote"];
        }

        foreach (var remote in Remotes)
        {
            remotesTable[remote.Key] = new TomlTable()
            {
                [nameof(SpdxRemote.Url).ToLower()] = remote.Value.Url.AbsoluteUri
            };
        }

        using var stream = _fileSystem.File.CreateText(_configPath);
        table.WriteTo(stream);
        stream.Flush();
    }
}