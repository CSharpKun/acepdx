using System.Text.Json.Serialization;

namespace Acepdx.Core.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LicenseList))]
[JsonSerializable(typeof(LicenseListEntry))]
[JsonSerializable(typeof(License))]
[JsonSerializable(typeof(CrossReference))]
[JsonSerializable(typeof(SpdxRemote))]
[JsonSerializable(typeof(Dictionary<string, SpdxRemote>))]
internal partial class AcepdxJsonSerializerContext : JsonSerializerContext;