using Acepdx.Core.Interfaces;

using DotMake.CommandLine;

using Spectre.Console;

using SpdxRemotes = System.Collections.Generic.Dictionary<string, Acepdx.Core.Models.SpdxRemote>;


namespace Acepdx.CLI.Commands;

[CliCommand(Description = "Manages remote vaults", Order = 5, Parent = typeof(MainCommand))]
public class RemoteCommand(IConfigService config)
{
    public async Task RunAsync()
    {
        var remotes = await config.Get<SpdxRemotes>("remote");

        if (remotes is null) 
        {
            AnsiConsole.MarkupLine($"[red]There are no remotes.[/]");
            return;
        }

        foreach (var remote in remotes)
        {
            AnsiConsole.MarkupLine(remote.Key);
        }
    }

    [CliCommand(Description = "Add remote vault")]
    public class AddCommand(IConfigService config)
    {
        [CliArgument(Description = "Name of the remote", Required = true)]
        public string Name { get; set; } = null!;

        [CliArgument(Description = "Url of the remote", Required = true)]
        public Uri Url { get; set; } = null!;

        public async Task RunAsync()
        {
            var remotes = await config.Get<SpdxRemotes>("remote") ?? [];

            if (remotes.ContainsKey(Name))
            {
                AnsiConsole.MarkupLine($"[red]Remote {Name} already exists.[/]");
                return;
            }

            if (!Url.IsAbsoluteUri)
            {
                AnsiConsole.MarkupLine("[red]Incorrect Url.[/]");
                return;
            }

            remotes[Name] = new() 
            { 
                Url = Url 
            };
            await config.Set<SpdxRemotes>("remote", remotes);
        }
    }

    [CliCommand(Description = "Rename remote vault")]
    public class RenameCommand(IConfigService config)
    {
        [CliArgument(Description = "Old name of the remote", Required = true)]
        public string OldName { get; set; } = null!;

        [CliArgument(Description = "New name of the remote", Required = true)]
        public string NewName { get; set; } = null!;

        public async Task RunAsync()
        {
            var remotes = await config.Get<SpdxRemotes>("remote") ?? [];

            if (!remotes.TryGetValue(OldName, out var remote))
            {
                AnsiConsole.MarkupLine($"[red]Remote {OldName} does not exist.[/]");
                return;
            }

            remotes[NewName] = remote;
            remotes.Remove(OldName);
            await config.Set<SpdxRemotes>("remote", remotes);
        }
    }

    [CliCommand(Description = "Removes remote vault")]
    public class RemoveCommand(IConfigService config)
    {
        [CliArgument(Description = "Name of the remote", Required = true)]
        public string Name { get; set; } = null!;

        public async Task RunAsync()
        {
            var remotes = await config.Get<SpdxRemotes>("remote") ?? [];

            if (!remotes.Remove(Name))
            {
                AnsiConsole.MarkupLine($"[red]Remote {Name} does not exist.[/]");
                return;
            }

            await config.Set<SpdxRemotes>("remote", remotes);
        }
    }
}
