using System.Net.Http.Headers;
using System.IO.Abstractions;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DotMake.CommandLine;
using Serilog.Sinks.Spectre;
using Serilog.Events;
using Serilog;

using Licensify.Core.Interfaces;
using Licensify.Core.Services;
using Licensify.CLI.Commands;

var mainCommand = Cli.Parse<MainCommand>().Bind<MainCommand>();

var logFile = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "licensify",
    "licensify.log"
);

#pragma warning disable CS8604

var logDirectory = Path.GetDirectoryName(logFile);
if (!Directory.Exists(logDirectory))
    Directory.CreateDirectory(logDirectory);

#pragma warning restore CS8604

var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logFile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                restrictedToMinimumLevel: LogEventLevel.Information
            );

if (mainCommand.Verbose) loggerConfig.WriteTo.Spectre();
Log.Logger = loggerConfig.CreateLogger();

var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3);
var clientInfo = new ProductInfoHeaderValue("Licensify", version);

Cli.Ext.ConfigureServices(services =>
{
    services
    //.AddSingleton<ICacher, MessagePackCacher>()
    .AddSingleton<IConfigService, TomlConfig>()
    .AddSingleton<ILicenseParser, SpdxLicenseParser>()
    .AddSingleton<IFileSystem, FileSystem>()
    .AddLogging(builder =>
        builder.ClearProviders().AddSerilog()
    )
    .AddHttpClient<ILicenseHttpService, SpdxHttpService>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.UserAgent.Add(clientInfo);
    });
});

await Cli.RunAsync<MainCommand>();