using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using Spectre.Console;

namespace Licensify.Services;

public interface IHttpService 
{
    public Task<T?> GetJsonRequest<T>(string url, string client, CancellationToken token = default);
}

[UnconditionalSuppressMessage("Trimming", "IL2026")]
[UnconditionalSuppressMessage("AOT", "IL3050")]
public class SpdxHttpService(IHttpClientFactory httpFactory, JsonSerializerOptions jsonOptions, CliGlobalFlags globalFlags) : IHttpService 
{
    public async Task<T?> GetJsonRequest<T>(string url, string clientName, CancellationToken token = default)
    {
        var client = httpFactory.CreateClient(clientName);
        
        try
        {    
            return await client.GetFromJsonAsync<T>(url, jsonOptions, token);
        }
        catch (TaskCanceledException ex) when (!token.IsCancellationRequested)
        {
            if (globalFlags.Verbose)
            {
                AnsiConsole.MarkupLine($"[red]Fetch to url {url} failed because of the timeout[/]");
                AnsiConsole.WriteException(ex);  
            } 
        }
        catch (HttpRequestException ex)
        {
            if (globalFlags.Verbose)
            {
                AnsiConsole.MarkupLine($"[red]Failed to fetch data from url {url}[/]");
                AnsiConsole.WriteException(ex); 
            }
        }
        catch (JsonException ex)
        {
            if (globalFlags.Verbose)
            {
                AnsiConsole.MarkupLine($"[red]Failed to deserialize data from url {url}[/]");
                AnsiConsole.WriteException(ex); 
            }
        }

        return default;
    }
}