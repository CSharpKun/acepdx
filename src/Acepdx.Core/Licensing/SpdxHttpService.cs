using System.Collections.Frozen;
using System.Net;
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

public class SpdxHttpService(HttpClient httpClient, IConfigService config, ICacheProvider cacheProvider, ILogger<SpdxHttpService>? logger = null) : ILicenseHttpService
{
    private readonly ILogger<SpdxHttpService> _logger =
        logger ?? NullLogger<SpdxHttpService>.Instance;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = AcepdxJsonSerializerContext.Default,
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
        var key = GetCacheKey(remote.Value.Url);

        try
        {
            var cachedList = await cacheProvider.GetAsync<CachedData<LicenseList>>(key, token);

            var response = await Probe(remote.Value.Url, cachedList, token);

            if (response is null)
            {
                return cachedList.Value;
            }

            list = await response.Content.ReadFromJsonAsync<LicenseList>(JsonOptions, token);


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

    private async Task<HttpResponseMessage?> Probe(Uri url, CacheInfo cacheInfo, CancellationToken token = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (cacheInfo is not null)
        {
            request.Headers.Add("If-Modified-Since", cacheInfo.LastModified.ToString("R"));
            request.Headers.Add("If-None-Match", cacheInfo.ETag);
        }

        var response = await httpClient.SendAsync(request, token);

        return response;
    }

    private static string GetCacheKey(Uri uri)
    {
        var bytes = Encoding.UTF8.GetBytes(uri.AbsoluteUri);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant() + ".cache";
    }

    private record CachedData<T>(T Value, string ETag, DateTime LastModified) : CacheInfo(ETag, LastModified);

    private record CacheInfo(string ETag, DateTime LastModified);
}
