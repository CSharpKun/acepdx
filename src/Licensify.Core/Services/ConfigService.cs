using System.IO.Abstractions;
using Licensify.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tommy;

namespace Licensify.Core.Services;

public class TomlConfig : IConfigService
{
    public Dictionary<string, SpdxRemote> Remotes { get; set; } = [];
    public Dictionary<string, string> Settings { get; set; } = [];

    private readonly IFileSystem _fileSystem;
    private readonly ILogger<TomlConfig> _logger;

    private static readonly string _configPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "licensify",
        "config.toml"
    );

    public TomlConfig(IFileSystem fileSystem, ILogger<TomlConfig>? logger = null) 
    {
        _fileSystem = fileSystem;
        _logger = logger ?? NullLogger<TomlConfig>.Instance;
        Load();
    }

    public void Load()
    {
        using var stream = _fileSystem.File.OpenText(_configPath);

        var root = TOML.Parse(stream);

        Settings = FlattenToml(root);

        if (root.TryGetNode("remote", out var remoteNode) || remoteNode is not TomlTable remoteTable) return;

        foreach (var kwp in remoteTable.RawTable) 
        {
            var valueTable = kwp.Value.AsTable;
            if (!valueTable.TryGetNode("url", out var url)) continue;

            Remotes.Add(kwp.Key, new() {
                Url = url
            });
        }
    }

    public static Dictionary<string, string> FlattenToml(TomlTable table, string prefix = "")
    {
        var result = new Dictionary<string, string>();

        foreach (var (key, child) in table.RawTable)
        {
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
                currentTable[key] = currentTable = [];
            }

            currentTable[keys[^1]] = kwp.Value;
        }

        if (!table.TryGetNode("remote", out var remotesTable))
        {
            remotesTable = table["remote"] = new TomlTable();
        }

        foreach (var remote in Remotes)
        {
            remotesTable[remote.Key] = new TomlTable()
            {
                [nameof(SpdxRemote.Url)] = remote.Value.Url
            };
        }

        using var stream = _fileSystem.File.CreateText(_configPath);
        table.WriteTo(stream);
        stream.Flush();
    }
}