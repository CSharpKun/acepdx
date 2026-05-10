using Licensify.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace Licensify.Core.Services;

public class YamlConfigService : IConfigService
{
    public Dictionary<string, object> Settings { get; set; } = [];

    public Dictionary<string, SpdxRemote> SpdxRemotes
    {
        get
        {
            if (!Settings.TryGetValue("remote", out var value) || value is not Dictionary<string, SpdxRemote>)
            {
                Settings["remote"] = new Dictionary<string, SpdxRemote>();
            }

            return Settings["remote"] as Dictionary<string, SpdxRemote> ?? [];
        }
        set
        {
            Settings["remote"] = value;
        }
    }

    private static string ConfigFile { get; } =
    Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "licensify",
        "config.yml"
    );

    private readonly IFileService _fileService;
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;
    private readonly ILogger<YamlConfigService> _logger;

    private const int DotSymbol = 1;
    private const int ColonSymbol = 1;

    public YamlConfigService(IFileService? fileService = null, ILogger<YamlConfigService>? logger = null)
    {
        _fileService = fileService ?? new FileService(new(ConfigFile));
        _deserializer = new StaticDeserializerBuilder(new LicensifyYamlContext()).Build();
        _serializer = new StaticSerializerBuilder(new LicensifyYamlContext()).Build();
        _logger = logger ?? NullLogger<YamlConfigService>.Instance;
        LoadFromDisk();
    }

    public void LoadFromDisk()
    {
        if (!_fileService.FileExists)
        {
            Settings = [];
            return;
        }

        var yaml = new YamlStream();
        yaml.Load(new StringReader(_fileService.ReadStringFromFile()));
        var root = yaml.Documents.FirstOrDefault()?.RootNode;

        if (root is null) 
        {
            Settings = [];
            return;
        }

        Settings = ConvertYamlNode(root) as Dictionary<string, object> ?? [];
    }

    private object? ConvertYamlNode(YamlNode node)
    {
        return node switch
        {
            YamlScalarNode scalar => scalar.Value,
            YamlMappingNode mapping => mapping.Children
                .ToDictionary(
                    kvp => ((YamlScalarNode)kvp.Key).Value!,
                    kvp => ConvertYamlNode(kvp.Value)
                ),
            YamlSequenceNode sequence => sequence.Children.Select(ConvertYamlNode).ToList(),
            _ => null
        };
    }

    public void UpdateSettings() => _fileService.WriteToFile(_serializer.Serialize(Settings));

}