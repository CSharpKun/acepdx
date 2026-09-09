using System.Text.Json.Serialization;

namespace Acepdx.Core.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LicenseList))]
[JsonSerializable(typeof(LicenseListEntry))]
[JsonSerializable(typeof(License))]
[JsonSerializable(typeof(CrossReference))]
internal partial class WebJsonSerializerContext : JsonSerializerContext;