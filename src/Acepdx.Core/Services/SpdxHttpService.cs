using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using Acepdx.Core.Exceptions;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Acepdx.Core.Services;

[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JsonSerializerContext should be provided in jsonOptions")]
[UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JsonSerializerContext should be provided in jsonOptions")]
public class SpdxHttpService(HttpClient httpClient, IConfigService config, ILogger<SpdxHttpService>? logger = null) : ILicenseHttpService
{
    private readonly ILogger<SpdxHttpService> _logger = logger ?? NullLogger<SpdxHttpService>.Instance;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = AcepdxJsonSerializerContext.Default
    };

    public async Task<List<LicenseList>> GetLicenseLists(CancellationToken token = default)
    {
        List<LicenseList> licenses = [];
        foreach (var remote in config.Remotes)
        {
            if (!Uri.TryCreate(remote.Value.Url, UriKind.Absolute, out var url))
            {
                _logger.LogWarning("List url {Url} of remote {Remote} is incorrectly formatted - skipping", remote.Value.Url, remote.Key);
                continue;
            }

            LicenseList? list = null;
            try
            {
                list = await httpClient.GetFromJsonAsync<LicenseList>(url, _jsonOptions, token);
            }
            catch (TaskCanceledException ex) when (!token.IsCancellationRequested) 
            {
                _logger.LogWarning(ex, "Couldn't get license list for remote {Remote} with url {Url} because of the timeout {Timeout}", remote.Key, url, httpClient.Timeout);
                continue;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Couldn't get license list for remote {Remote} with url {Url} because of the internet connectivity", remote.Key, url);
                continue;
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Couldn't get license list for remote {Remote} with url {Url} because of the JSON serialization error", remote.Key, url);
                throw;
            }

            if (list is null)
            {
                _logger.LogWarning("Couldn't get license list for remote {Remote} with url {Url} for unknown reason", remote.Key, url);
                continue;
            }
            list.Remote = remote.Key;
            list.Licenses.ForEach(license => license.Remote = remote.Key);
            licenses.Add(list);
        }

        if (licenses.Count == 0)
        {
            throw new AllRemotesTriedException(
                userMessage: "All remotes were tried, check your internet connection and try again.",
                technicalMessage: "Licenses list is empty"
            );
        }

        return licenses;
    }

    public async Task<License?> GetLicense(LicenseListEntry licenseEntry, CancellationToken token = default)
    {
        return await httpClient.GetFromJsonAsync<License>(licenseEntry.DetailsUrl, _jsonOptions, token);
    }
}