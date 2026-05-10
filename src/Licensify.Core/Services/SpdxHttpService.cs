using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using Licensify.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Licensify.Core.Services;

[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JsonSerializerContext should be provided in jsonOptions")]
[UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JsonSerializerContext should be provided in jsonOptions")]
public class SpdxHttpService(HttpClient client, IConfigService config, ILogger<SpdxHttpService>? logger = null) : ILicenseHttpService
{
    private readonly ILogger<SpdxHttpService> _logger = logger ?? NullLogger<SpdxHttpService>.Instance;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = LicensifyJsonSerializerContext.Default
    };

    public async Task<List<LicenseList>> GetLicenseLists(CancellationToken token = default)
    {
        List<LicenseList> licenses = [];
        foreach (var remote in config.SpdxRemotes)
        {
            if (!Uri.TryCreate(remote.Value.Url, UriKind.Absolute, out var uri))
            {
                continue;
            }
            var list = await GetJsonRequest<LicenseList>(uri, token);
            if (list is null) continue;
            list.Licenses.ForEach(license => license.Remote = remote.Key);
            licenses.Add(list);
        }
        return licenses;
    }

    public async Task<License?> GetLicense(LicenseListEntry licenseEntry, CancellationToken token = default)
    {
        if (!Uri.TryCreate(licenseEntry.DetailsUrl, UriKind.Absolute, out var result))
        {
            return null;
        }
        return await GetJsonRequest<License>(result, token);
    }

    private async Task<T?> GetJsonRequest<T>(Uri url, CancellationToken token = default)
    {
        try
        {
            return await client.GetFromJsonAsync<T>(url, _jsonOptions, token);
        }
        catch (TaskCanceledException ex) when (!token.IsCancellationRequested)
        {
            _logger.LogError(eventId: new(), exception: ex, "Fetch to url {Url} failed because of the timeout", url);
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(eventId: new(), exception: ex, "Failed to fetch data from url {Url}", url);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(eventId: new(), exception: ex, "Failed to deserialize data from url {Url}", url);
            throw;
        }
    }
}