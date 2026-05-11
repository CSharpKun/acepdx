using DotMake.CommandLine;
using Licensify.Core.Interfaces;
using Spectre.Console;

namespace Licensify.CLI.Commands;

[CliCommand(
    Description = "Manages remote vaults",
    Order = 5,
    Parent = typeof(RootCommand)
)]
public class RemoteCommand(IConfigService config)
{
    public async Task RunAsync()
    { 
        foreach (var remote in config.Remotes) AnsiConsole.MarkupLine(remote.Key);
    }

    [CliCommand(Description = "Add remote vault")]
    public class AddCommand(IConfigService config) 
    {
        [CliArgument(
            Description = "Name of the remote",
            Required = true
        )]
        public string Name { get; set; } = null!;

        [CliArgument(
            Description = "Url of the remote",
            Required = true
        )]
        public Uri Url { get; set; } = null!;

        public async Task RunAsync() 
        {
            if (config.Remotes.ContainsKey(Name)) 
            {
                AnsiConsole.MarkupLine($"[red]Remote {Name} already exists.[/]");
                return;
            }

            if (!Url.IsAbsoluteUri) 
            {
                AnsiConsole.MarkupLine("[red]Incorrect Url.[/]");
                return;
            }

            config.Remotes[Name] = new() 
            { 
                Url = this.Url.AbsoluteUri
            }; 
            config.Save();
        }
    }

    [CliCommand(
        Description = "Rename remote vault"
    )]
    public class RenameCommand(IConfigService config) 
    {
        [CliArgument(
            Description = "Old name of the remote",
            Required = true
        )]
        public string OldName { get; set; } = null!;

        [CliArgument(
            Description = "New name of the remote",
            Required = true
        )]
        public string NewName { get; set; } = null!;

        public async Task RunAsync() 
        {
            if (!config.Remotes.TryGetValue(OldName, out var remote)) 
            {
                AnsiConsole.MarkupLine($"[red]Remote {OldName} does not exist.[/]");
                return;
            }

            config.Remotes[NewName] = remote;
            config.Remotes.Remove(OldName);
            config.Save();
        }
    }

    [CliCommand(
        Description = "Removes remote vault"
    )]
    public class RemoveCommand(IConfigService config) 
    {
        [CliArgument(
            Description = "Name of the remote",
            Required = true
        )]
        public string Name { get; set; } = null!;

        public async Task RunAsync() 
        {
            if (!config.Remotes.Remove(Name)) 
            {
                AnsiConsole.MarkupLine($"[red]Remote {Name} does not exist.[/]");
                return;
            }

            config.Save();
        }
    }
}