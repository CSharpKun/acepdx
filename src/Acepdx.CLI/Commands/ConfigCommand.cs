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
        await new GetCommand(configService) { Key = Key }.RunAsync();

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
            await configService.Set(Key, Value);
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
            await configService.Unset(Key);
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
            var value = await configService.Get<string>(Key);
            
            if (value is null)
            {
                AnsiConsole.MarkupLine("[red]Key does not exist.[/]");
            }
            else
            {
                AnsiConsole.WriteLine(value);
            }
        }
    }
}
