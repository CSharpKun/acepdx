using DotMake.CommandLine;
using Licensify.Services;
using Spectre.Console;

namespace Licensify.Commands;

[CliCommand(
    Description = "Manages Config."
)]
public class ConfigCommand(IConfigService configService) : ConfigKeyArgument
{
    public async Task RunAsync()
    {
        await new GetCommand(configService) { Key = this.Key }.RunAsync();
    }

    public class SetCommand(IConfigService configService) : ConfigKeyArgument
    {
        [CliArgument(
            Description = "Config Value",
            Required = true
        )]
        public string Value { get; set; } = null!;

        public async Task RunAsync() 
        {
            configService.Settings[Key] = Value;
            configService.UpdateSettings();
        }
    }

    public class GetCommand(IConfigService configService) : ConfigKeyArgument
    {
        public async Task RunAsync() 
        {
            if (!configService.Settings.TryGetValue(Key, out var value) || value is null) value = "";
            AnsiConsole.WriteLine(value);
        }
    }
}

public abstract class ConfigKeyArgument 
{
    [CliArgument(
        Description = "Config Key",
        Required = true,
        ValidationPattern = @".*?\..{1,}"
    )]
    public string Key { get; set; } = null!;
}