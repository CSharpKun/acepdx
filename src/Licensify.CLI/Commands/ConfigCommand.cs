using DotMake.CommandLine;
using Licensify.Core.Interfaces;
using Spectre.Console;

namespace Licensify.CLI.Commands;

[CliCommand(
    Description = "Manages config",
    Order = 4,
    Parent = typeof(RootCommand)
)]
public class ConfigCommand(IConfigService configService) : ConfigKeyArgument
{
    public async Task RunAsync() => await new GetCommand(configService) { Key = this.Key }.RunAsync();

    [CliCommand(Description = "Set or create specified key with value.")]
    public class SetCommand(IConfigService configService) : ConfigKeyArgument
    {
        [CliArgument(
            Description = "Config Value",
            Order = 2,
            Required = true
        )]
        public string Value { get; set; } = null!;

        public async Task RunAsync() 
        {
            
            configService.Settings[Key] = Value;
            configService.Save();
        }
    }

    [CliCommand(Description = "Remove specified key.")]
    public class UnsetCommand(IConfigService configService) : ConfigKeyArgument 
    {
        public async Task RunAsync() 
        { 
            if (configService.Settings.Remove(Key)) 
            {
                configService.Save();
                return;
            }  
            AnsiConsole.MarkupLine("[red]Key does not exist.[/]"); 

        }
    }

    [CliCommand(Description = "Get value by specified key.")]
    public class GetCommand(IConfigService configService) : ConfigKeyArgument
    {
        public async Task RunAsync() 
        {
            if (!configService.Settings.TryGetValue(Key, out var value)) AnsiConsole.MarkupLine("[red]Key does not exist.[/]");
            else if (value is null) value = string.Empty;
            else AnsiConsole.WriteLine(value);
        }
    }
}

public abstract class ConfigKeyArgument 
{
    [CliArgument(
        Description = "Config Key",
        Required = true,
        Order = 1,
        ValidationPattern = @".*?\..{1,}"
    )]
    public string Key { get; set; } = null!;
}