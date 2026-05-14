using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using Licensify.Core.Exceptions;
using Licensify.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Licensify.Core.Services;

[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JsonSerializerContext should be provided in jsonOptions")]
[UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JsonSerializerContext should be provided in jsonOptions")]
public class SpdxHttpService(IHttpClientFactory clientFactory, IConfigService config, ILogger<SpdxHttpService>? logger = null) : ILicenseHttpService
{
    private readonly ILogger<SpdxHttpService> _logger = logger ?? NullLogger<SpdxHttpService>.Instance;

    private readonly HttpClient _httpClient = clientFactory.CreateClient();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = LicensifyJsonSerializerContext.Default
    };

    public async Task<List<LicenseList>> GetLicenseLists(CancellationToken token = default)
    {
        List<LicenseList> licenses = [];
        foreach (var remote in config.Remotes)
        { 
            if (!Uri.TryCreate(remote.Value.Url, UriKind.Absolute, out var url))
            {
                _logger.LogError("List URL of remote {Remote} is incorrectly formatted - skipping", remote.Key);
                continue;
            }
        
            var list = await GetJsonRequest<LicenseList>(url, token);
            if (list is null) 
            {
                _logger.LogError("Couldn't get license list for url {Url}", url);
                continue;
            } 
            list.Licenses.ForEach(license => license.Remote = remote.Key);
            licenses.Add(list);
        }

        if (licenses.Count == 0) 
        {
            throw new AllRemotesTriedException(
                userMessage: "All remotes were tried, but none answered. Check your internet connection and try again.",
                technicalMessage: "Licenses list is empty"
            );
        }

        return licenses;
    }

    public async Task<License?> GetLicense(LicenseListEntry licenseEntry, CancellationToken token = default)
    {
        return await GetJsonRequest<License>(licenseEntry.DetailsUrl, token);
    }

    private async Task<T?> GetJsonRequest<T>(Uri url, CancellationToken token = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<T>(url, _jsonOptions, token);
        }
        catch (TaskCanceledException ex) when (!token.IsCancellationRequested)
        {
            _logger.LogError(exception: ex, "Fetch to url {Url} failed because of the timeout", url);
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(exception: ex, "Failed to fetch data from url {Url}", url);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(exception: ex, "Failed to deserialize data from url {Url}", url);
            throw;
        }
    }
}