using System.IO.Abstractions;
using System.Net.Http.Headers;
using System.Reflection;
using Acepdx.CLI.Commands;
using Acepdx.Core;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Services;
using DotMake.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Spectre;

var mainCommand = Cli.Parse<MainCommand>().Bind<MainCommand>();

var logFile = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "acepdx",
    "acepdx.log"
);

var logDirectory = Path.GetDirectoryName(logFile);
if (logDirectory is not null && !Directory.Exists(logDirectory))
    Directory.CreateDirectory(logDirectory);

var loggerConfig = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File(
        logFile,
        outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        restrictedToMinimumLevel: LogEventLevel.Information
    );

if (mainCommand.Verbose)
    loggerConfig.WriteTo.Spectre(
        outputTemplate: "[{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}"
    );
Log.Logger = loggerConfig.CreateLogger();

var httpHandler = new HttpClientHandler();

if (mainCommand.Proxy is not null)
{
    httpHandler.UseProxy = true;
    httpHandler.Proxy = mainCommand.Proxy;
}

var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
var clientInfo = new ProductInfoHeaderValue("Acepdx", version);

Cli.Ext.ConfigureServices(services =>
{
    services
        //.AddSingleton<ICacher, MessagePackCacher>()
        .AddSingleton<IConfigService, TomlConfig>()
        .AddTransient<ILicenseParser, LegacyLicenseParser>()
        .AddSingleton<IFileSystem, FileSystem>()
        .AddSingleton<IFolders, AcepdxFolders>()
        .AddLogging(builder => builder.ClearProviders().AddSerilog())
        .AddHttpClient<ILicenseHttpService, SpdxHttpService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.Add(clientInfo);
        })
        .ConfigurePrimaryHttpMessageHandler(services =>
        {
            var handler = new HttpClientHandler();
            if (mainCommand.Proxy is not null)
            {
                handler.UseProxy = true;
                handler.Proxy = mainCommand.Proxy;
            }
            return handler;
        });
});

await Cli.RunAsync<MainCommand>();
