using Acepdx.Core.Interfaces;
using DotMake.CommandLine;
using Spectre.Console;

namespace Acepdx.CLI.Commands;

[CliCommand(Description = "Manages config", Order = 4, Parent = typeof(MainCommand))]
public class ConfigCommand(IConfigService configService)
{
    [CliArgument(
        Description = "Config Key",
        Required = true,
        Order = 1,
        ValidationPattern = @".*?\..{1,}"
    )]
    public string Key { get; set; } = null!;

    public async Task RunAsync() =>
        await new GetCommand(configService) { Key = this.Key }.RunAsync();

    [CliCommand(Description = "Set or create specified key with value.")]
    public class SetCommand(IConfigService configService)
    {
        [CliArgument(
            Description = "Config Key",
            Required = true,
            Order = 1,
            ValidationPattern = @".*?\..{1,}"
        )]
        public string Key { get; set; } = null!;

        [CliArgument(Description = "Config Value", Order = 2, Required = true)]
        public string Value { get; set; } = null!;

        public async Task RunAsync()
        {
            configService.Settings[Key] = Value;
            configService.Save();
        }
    }

    [CliCommand(Description = "Remove specified key.")]
    public class UnsetCommand(IConfigService configService)
    {
        [CliArgument(
            Description = "Config Key",
            Required = true,
            Order = 1,
            ValidationPattern = @".*?\..{1,}"
        )]
        public string Key { get; set; } = null!;

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
    public class GetCommand(IConfigService configService)
    {
        [CliArgument(
            Description = "Config Key",
            Required = true,
            Order = 1,
            ValidationPattern = @".*?\..{1,}"
        )]
        public string Key { get; set; } = null!;

        public async Task RunAsync()
        {
            if (!configService.Settings.TryGetValue(Key, out var value))
                AnsiConsole.MarkupLine("[red]Key does not exist.[/]");
            else if (value is null)
                value = string.Empty;
            else
                AnsiConsole.WriteLine(value);
        }
    }
}
