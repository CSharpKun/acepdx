using System.Text.Json.Serialization;

namespace Licensify.Core;

public enum DataQueryType 
{
    Username,
    Other
}

public enum FlagQueryType 
{
    OptionalPart
}

public class SpdxRemote
{
    public string Url { get; set; } = null!;
}

public record LicenseList(
    [property: JsonPropertyName("licenseListVersion")] string Version,
    List<LicenseListEntry> Licenses
);

public partial record LicenseListEntry(
    string Reference,
    bool IsDeprecatedLicenseId,
    string DetailsUrl,
    int ReferenceNumber,
    string Name,
    string LicenseId,
    IReadOnlyList<string> SeeAlso,
    bool? IsOsiApproved,
    bool? IsFsfLibre
) 
{
    [JsonIgnore] public string? Remote { get; set; }
};

public partial record License(
    bool IsDeprecatedLicenseId,
    string LicenseText,
    string StandardLicenseTemplate,
    string Name,
    string LicenseId,
    IReadOnlyList<CrossReference> CrossRef,
    IReadOnlyList<string> SeeAlso,
    bool? IsOsiApproved,
    string LicenseTextHtml
);

public partial record CrossReference(
    string Match,
    string Url,
    bool IsValid,
    bool IsLive,
    DateTime Timestamp,
    bool IsWayBackLink,
    int Order
);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LicenseList))]
[JsonSerializable(typeof(LicenseListEntry))]
[JsonSerializable(typeof(License))]
[JsonSerializable(typeof(CrossReference))]
public partial class LicensifyJsonSerializerContext : JsonSerializerContext;