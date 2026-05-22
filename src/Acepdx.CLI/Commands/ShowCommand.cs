using System.Collections;
using System.Reflection.Metadata.Ecma335;
using Acepdx.Core;
using Acepdx.Core.Exceptions;
using Acepdx.Core.Extensions;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;
using DotMake.CommandLine;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Acepdx.CLI.Commands;

[CliCommand(
    Description = "Shows information about specified license",
    Order = 2,
    Parent = typeof(MainCommand),
    Alias = "get"
)]
public class ShowCommand(ILicenseHttpService httpService, ILogger<ShowCommand> logger)
{
    [CliOption(Description = "Include licenses with deprecated ids")]
    public bool DeprecatedId { get; set; }

    [CliArgument(Description = "License Id", Required = true)]
    public string LicenseId { get; set; } = null!;

    public async Task RunAsync()
    {
        List<LicenseList>? lists;

        try
        {
            lists = await httpService.GetLicenseLists();
        }
        catch (NoRemotesFoundException ex)
        {
            logger.LogWarning(ex, "There are no existing remotes.");
            AnsiConsole.Markup($"[bold yellow]{ex.UserMessage}[/]");
            return;
        }
        catch (AllRemotesTriedException ex)
        {
            logger.LogCritical(ex, "All remotes were tried.");
            AnsiConsole.MarkupLine($"[bold red]{ex.UserMessage}[/]");
            return;
        }

        var entries = lists
                .SelectMany(list => list.Licenses)
                .GetRelevantLicenses(LicenseId, DeprecatedId)
                .ToList();

        var entry = entries.Count switch
        {
            1 => entries.First(),
            > 1 => AskForLicense(entries),
            _ => null
        };

        if (entry is null)
        {
            logger.LogCritical("The result licenses list contains zero or less items");
            AnsiConsole.MarkupLine("[bold red]The license id does not exist in remotes.[/]");
            return;
        }

        var license = await httpService.GetLicense(entry);

        if (license is null)
        {
            AnsiConsole.MarkupLine($"[bold red]Couldn't get {LicenseId} license. Check your internet connection.[/]");
            return;
        }

        foreach (var prop in typeof(License).GetProperties()
            .Where(p => p.PropertyType == typeof(string) && p.CanWrite))
        {
            if (prop.GetValue(license) is string value)
                prop.SetValue(license, Markup.Escape(value));
        }

        var renderList = new List<IRenderable>
        {
            new Panel(license.LicenseText)
            {
                Border = BoxBorder.Double
            }
        };

        if (license.IsDeprecatedLicenseId is not null)
            renderList.Add(new Markup($"Deprecated License Id: {GetStatusColorTag(entry.IsDeprecatedLicenseId ?? false, reverse: true) + entry.IsDeprecatedLicenseId}[/]"));

        if (license.IsOsiApproved is not null)
            renderList.Add(new Markup($"Osi Approved: {GetStatusColorTag(license.IsOsiApproved ?? false) + license.IsOsiApproved}[/]"));

        if (license.CrossRef is not null)
            foreach (var reference in license.CrossRef)
            {
                var rows = new List<Markup>() {
                    new($"[bold]Reference URL:[/] [link]{reference.Url}[/]")
                };

                if (reference.IsLive is not null)
                    rows.Add(new($"[bold]Live:[/] {GetStatusColorTag(reference.IsLive ?? false) + reference.IsLive}[/]"));

                if (reference.IsValid is not null)
                    rows.Add(new($"[bold]Valid:[/] {GetStatusColorTag(reference.IsValid ?? false) + reference.IsValid}[/]"));

                rows.AddRange([
                    new Markup($"[bold]Match:[/] \"{reference.Match}\""),
                    new Markup($"[bold]Timestamp:[/] {reference.Timestamp}")
                ]);

                renderList.Add(new Panel(new Rows(rows)));
            }

        var panel = new Panel(new Rows(renderList))
        {
            Header = new PanelHeader($"[bold]{entry.Name}[/] ({entry.LicenseId})"),
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }

    private static LicenseListEntry AskForLicense(List<LicenseListEntry> entries)
    {
        var duplicates = entries
            .GroupBy(license => license.LicenseId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToHashSet();

        return AnsiConsole.Prompt<LicenseListEntry>(
            new SelectionPrompt<LicenseListEntry>()
                .Title("Multiple licenses found:")
                .UseConverter(entry => duplicates.Contains(entry.LicenseId) 
                    ? $"{entry.LicenseId} (from {entry.Remote})" 
                    : entry.LicenseId)
                .AddChoices(entries)
        );
    }

    private static string GetStatusColorTag(bool condition, bool reverse = false) => condition ^ reverse ? "[green]" : "[red]";
}