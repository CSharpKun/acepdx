using System.Text.Json.Serialization;

namespace Licensify.Core;

public enum VariableType 
{
    Copyright,
    Other
}

public class SpdxRemote
{
    public required string Url { get; set; }
}

public record LicenseList(
    List<LicenseListEntry> Licenses,
    [property: JsonPropertyName("licenseListVersion")] string? Version,
    DateTime? ReleaseDate
);

public partial class LicenseListEntry
{
    public required string Name { get; init; }
    public required string LicenseId { get; init; }
    public required Uri DetailsUrl { get; init; }

    public string? Comments { get; init; }
    public string? DeprecatedVersion { get; init; }
    
    public Uri? Reference { get; init; }
    public int? ReferenceNumber { get; init; }

    public bool? IsDeprecatedLicenseId { get; init; }
    public bool? IsOsiApproved { get; init; }
    public bool? IsFsfLibre { get; init; }

    public IReadOnlyList<string>? SeeAlso { get; init; }

    [JsonIgnore] public string? Remote { get; set; }
};

public partial class License {
    public required string Name { get; init; }
    public required string LicenseId { get; init; }
    public required string LicenseText { get; init; }
    public required string StandardLicenseTemplate { get; init; }

    public string? StandardLicenseHeaderTemplate { get; init; }
    public string? LicenseTextHtml { get; init; } 
    
    public bool? IsDeprecatedLicenseId { get; init; } 
    public bool? IsOsiApproved { get; init; } 
    
    public IReadOnlyList<CrossReference>? CrossRef { get; init; } 
    public IReadOnlyList<string>? SeeAlso { get; init; } 
};

public partial class CrossReference 
{
    public required Uri Url { get; init; }
    
    public DateTime? Timestamp { get; init; }
    public string? Match { get; init; }
    public int? Order { get; init; }

    public bool? IsValid { get; init; }
    public bool? IsLive { get; init; }
    public bool? IsWayBackLink { get; init; } 
}
    

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(LicenseList))]
[JsonSerializable(typeof(LicenseListEntry))]
[JsonSerializable(typeof(License))]
[JsonSerializable(typeof(CrossReference))]
public partial class LicensifyJsonSerializerContext : JsonSerializerContext;