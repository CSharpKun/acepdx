using System.Text.Json.Serialization;

namespace Acepdx.Core.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(SpdxRemote))]
[JsonSerializable(typeof(Dictionary<string, SpdxRemote>))]
internal partial class ConfigJsonSerializerContext : JsonSerializerContext;