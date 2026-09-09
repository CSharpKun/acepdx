using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

using Acepdx.Core.Exceptions;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Acepdx.Core.Licensing;

[UnconditionalSuppressMessage(
    "Trimming",
    "IL2026",
    Justification = "JsonSerializerContext provided in JsonOptions"
)]
[UnconditionalSuppressMessage(
    "AOT",
    "IL3050",
    Justification = "JsonSerializerContext provided in JsonOptions"
)]
public class SpdxHttpService(HttpClient httpClient, IConfigService config, ICacheProvider cacheProvider, ILogger<SpdxHttpService>? logger = null) : ILicenseHttpService
{
    private readonly ILogger<SpdxHttpService> _logger =
        logger ?? NullLogger<SpdxHttpService>.Instance;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = WebJsonSerializerContext.Default,
    };

    public async Task<License?> GetLicense(
        LicenseListEntry licenseEntry,
        CancellationToken token = default
    )
    {
        return await httpClient.GetFromJsonAsync<License>(
            licenseEntry.DetailsUrl,
            JsonOptions,
            token
        );
    }

    public async Task<LicenseList[]> GetLicenseLists(CancellationToken token = default)
    {
        var remotes = await config.Get<Dictionary<string, SpdxRemote>>("remote");

        if (remotes?.Count is 0 or null)
        {
            throw new NoRemotesFoundException("Count of the remotes equals to zero");
        }

        var tasks = remotes.Select(remote => GetLicenseList(remote, token));

        var results = await Task.WhenAll(tasks);

        var lists = results.OfType<LicenseList>().ToArray();

        return lists.Length == 0 ? throw new AllRemotesTriedException("All remotes returned null") : lists;
    }

    public async Task<XDocument?> GetXml(ILicense license, CancellationToken token = default)
    {
        var stream = await httpClient.GetStreamAsync(license.LicenseXml, token);
        return XDocument.Load(stream);
    }

    private async Task<LicenseList?> GetLicenseList(KeyValuePair<string, SpdxRemote> remote, CancellationToken token = default)
    {
        LicenseList? list;

        try
        {
            list = await DownloadAndCache<LicenseList?>(remote.Value.Url, token);
        }
        catch (TaskCanceledException ex) when (!token.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                "Couldn't get license list for remote {Remote} with url {Url} because of the timeout {Timeout}",
                remote.Key,
                remote.Value.Url,
                httpClient.Timeout
            );
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Couldn't get license list for remote {Remote} with url {Url} because of the internet connectivity",
                remote.Key,
                remote.Value.Url
            );
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Couldn't get license list for remote {Remote} with url {Url} because of the JSON serialization error",
                remote.Key,
                remote.Value.Url
            );
            return null;
        }

        if (list is null)
        {
            _logger.LogWarning(
                "Couldn't get license list for remote {Remote} with url {Url} for unknown reason",
                remote.Key,
                remote.Value.Url
            );
            return null;
        }

        list.Remote = remote.Key;

        foreach (var license in list.Licenses)
        {
            license.Remote = remote.Key;
        }

        return list;
    }

    private async Task<T?> DownloadAndCache<T>(Uri targetUri, CancellationToken token = default)
    {
        var key = GetCacheKey(targetUri);

        var cachedData = await cacheProvider.GetAsync<CachedData<T>>(key, token);

        var request = new HttpRequestMessage(HttpMethod.Get, targetUri);

        if (cachedData is not null)
        {
            request.Headers.IfModifiedSince = cachedData.LastModified;

            if (cachedData.ETag is not null)
            {
                request.Headers.IfNoneMatch.Add(cachedData.ETag);
            }
        }

        var response = await httpClient.SendAsync(request, token);

        if (response is null || response.StatusCode is HttpStatusCode.NotModified)
        {
            return cachedData is not null ? cachedData.Value : default;
        }

        var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, token);

        if (data is null)
        {
            return default;
        }

        CachedData<T> cacheData = new(data, response.Headers.ETag, response.Content.Headers.LastModified);
        await cacheProvider.SetAsync<CachedData<T>>(key, cacheData, token);

        return data;
    }

    private static string GetCacheKey(Uri uri)
    {
        var bytes = Encoding.UTF8.GetBytes(uri.AbsoluteUri);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant() + ".cache";
    }

    private sealed record CachedData<T>(T Value, EntityTagHeaderValue? ETag, DateTimeOffset? LastModified);
}
