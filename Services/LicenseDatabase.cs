using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using Spectre.Console;

namespace Licensify.Services;

public interface ICacher
{
    public Task<T?> GetData<T>(string name, CancellationToken token = default) where T : class;
}

[UnconditionalSuppressMessage("Trimming", "IL2026")]
[UnconditionalSuppressMessage("AOT", "IL3050")]
public class MessagePackCacher(JsonSerializerOptions options, CliGlobalFlags globalFlags, IHttpService httpService, IConfigService settings) : ICacher
{
    private static string ApplicationFolder { get; } = 
    Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "licensify"
    );

    private static string CacheFile { get; } = 
    Path.Combine(
        ApplicationFolder,
        "cache"
    );

    public async Task<T?> GetData<T>(string name, CancellationToken token = default) where T : class
    {
        var tName = typeof(T).Name + "_" + name;
        Dictionary<string, JsonElement>? cacheResult = [];

        if (!globalFlags.ForceNoCache && TryGetFromCache(out cacheResult) && (cacheResult?.TryGetValue(tName, out var json) ?? false))
        {
            var deserialized = json.Deserialize<T>(options);
            if (deserialized is not null)
            {
                if (globalFlags.Verbose) AnsiConsole.MarkupLine($"[grey]Using local copy of {tName}[/]");
                return deserialized;    
            }
            if (globalFlags.Verbose) AnsiConsole.MarkupLine($"[grey]Couldn't deserialize local copy of {tName}[/]");
        }

        var repo = settings.Settings["spdx.repo"] ?? "github";

        var url = repo == "github" && name != "licenses.json" ? "details/" + name : name;

        var fetchResult = await httpService.GetJsonRequest<T>(url, repo, token);
        if (fetchResult is null) return fetchResult;

        cacheResult ??= [];
        cacheResult[tName] = JsonSerializer.SerializeToElement(fetchResult, options);
        WriteToCache(cacheResult);
        return fetchResult;
    }

    private bool TryGetFromCache<T>(out T? result)
    {
        Directory.CreateDirectory(ApplicationFolder);
        var isFileOld = File.GetLastWriteTime(CacheFile) < DateTime.Now - TimeSpan.FromHours(10);
        if (!File.Exists(CacheFile) || isFileOld)
        {
            result = default;
            return false;
        } 

        var json = File.ReadAllText(CacheFile);

        try
        {
            result = JsonSerializer.Deserialize<T>(json, options); 
            if (result is null && globalFlags.Verbose) AnsiConsole.MarkupLine("[grey]Couldn't parse cache JSON[/]");
            return true;
        }   
        catch (JsonException ex)
        {
            if (globalFlags.Verbose) 
            { 
                AnsiConsole.MarkupLine("[grey]Couldn't parse cache JSON[/]");
                AnsiConsole.WriteException(ex);
            }
            File.Delete(CacheFile);
            result = default;
            return false;
        }   
    }

    private void WriteToCache(object obj) => File.WriteAllText(CacheFile, JsonSerializer.Serialize(obj, options));
}