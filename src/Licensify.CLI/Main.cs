using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using DotMake.CommandLine;
using Licensify.Commands;
using Licensify;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Licensify.Core.Services;
using Licensify.Core;
using Licensify.Core.Interfaces;

var rootCommand = Cli.Parse<RootCommand>().Bind<RootCommand>();

Cli.Ext.ConfigureServices(services =>
{
    services
    //.AddSingleton<ICacher, MessagePackCacher>()
    .AddSingleton<ILicenseHttpService, SpdxHttpService>()
    .AddSingleton<IConfigService, YamlConfigService>()
    .AddSingleton<ILicenseParser, LicenseParser>();

    var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
    var clientInfo = new ProductInfoHeaderValue("Licensify", version);

    /*services.AddHttpClient("spdx", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.BaseAddress = new("https://spdx.org/licenses/");
        client.DefaultRequestHeaders.UserAgent.Add(clientInfo);
    });

    services.AddHttpClient("github", client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.BaseAddress = new("https://raw.githubusercontent.com/spdx/license-list-data/main/json/");
        client.DefaultRequestHeaders.UserAgent.Add(clientInfo);
    }); */

    services.AddSingleton<HttpClient>();

    /*
    if (result is null) return;

    services.AddHttpClient(result.Host, client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.BaseAddress = result;
        client.DefaultRequestHeaders.UserAgent.Add(clientInfo);
    });
    */
});

await Cli.RunAsync<RootCommand>();