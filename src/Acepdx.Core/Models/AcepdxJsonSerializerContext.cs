using System.Text.Json.Serialization;

using Acepdx.Core.Models;

namespace Acepdx.Models;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LicenseList))]
[JsonSerializable(typeof(LicenseListEntry))]
[JsonSerializable(typeof(License))]
[JsonSerializable(typeof(CrossReference))]
public partial class AcepdxJsonSerializerContext : JsonSerializerContext;