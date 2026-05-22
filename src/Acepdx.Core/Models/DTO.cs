using System.Text.Json.Serialization;
using Acepdx.Core.Interfaces;

namespace Acepdx.Core.Models;

public partial class LicenseList
{
    public required List<LicenseListEntry> Licenses { get; init; }
    [JsonPropertyName("licenseListVersion")] public string? Version { get; init; }
    public DateTime? ReleaseDate { get; init; }    
    [JsonIgnore] public string? Remote { get; set; }
}

public partial class LicenseListEntry : ILicense
{
    public required string Name { get; init; }
    public required string LicenseId { get; init; }
    
    /* 
        I want to switch to XML format as that would be much easier and more correctly 
        from the point of the SPDX Specification, according to it's annex C.
        But I still didn't figure out what the format of SPDX licenses list for internet will be in V3.
        I know only of a LicenseXml field so I decided to add it here at least for now.
        If you know what it would be, please contact me or write an issue.
        PRs are also welcomed.
    */

    public Uri? LicenseXml { get; init; }
    public required Uri DetailsUrl { get; init; }

    public Uri? Reference { get; init; }
    public int? ReferenceNumber { get; init; }

    public bool? IsDeprecatedLicenseId { get; init; }
    public bool? IsOsiApproved { get; init; }
    public bool? IsFsfLibre { get; init; }

    public IReadOnlyList<string>? SeeAlso { get; init; }

    [JsonIgnore] public string? Remote { get; set; }
};

public partial class License : ILicense
{
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
public partial class AcepdxJsonSerializerContext : JsonSerializerContext;