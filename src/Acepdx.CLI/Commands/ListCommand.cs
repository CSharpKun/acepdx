using Acepdx.Core.Exceptions;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;
using DotMake.CommandLine;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Acepdx.CLI.Commands;

[CliCommand(Description = "Lists all licenses in a table", Order = 1, Parent = typeof(MainCommand))]
public class ListCommand(ILogger<ListCommand> logger, ILicenseHttpService httpService)
{
    public async Task RunAsync()
    {
        List<LicenseList>? lists = null;

        try
        {
            lists = await httpService.GetLicenseLists();
        }
        catch (NoRemotesFoundException ex)
        {
            logger.LogWarning(ex, "There are no remotes in the config.");
            AnsiConsole.Markup(
                $"[bold yellow]No remotes are assigned in the config file. Please add at least one.[/]"
            );
            return;
        }
        catch (AllRemotesTriedException ex)
        {
            logger.LogCritical(ex, "Every remote in the config failed to respond.");
            AnsiConsole.MarkupLine(
                $"[bold red]All remotes failed to respond, check your internet connection and try again.[/]"
            );
            return;
        }

        if (lists is null)
        {
            AnsiConsole.MarkupLine(
                "[bold red]Couldn't get list of licenses. Check your internet connection.[/]"
            );
            return;
        }

        List<Table> tables = [];

        foreach (var list in lists)
        {
            if (list.Remote is null)
            {
                logger.LogError(
                    "The list does not contain a name, but still exists. Using default name."
                );
                list.Remote = "Unknown";
            }

            var titleName = Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(list.Remote);

            var table = new Table().RoundedBorder().Title($"{titleName} Licenses");

            var tableData = list
                .Licenses.Where(license => license.IsDeprecatedLicenseId is false)
                .OrderBy(license => license.LicenseId);

            table.AddColumns(
                new TableColumn(nameof(LicenseListEntry.LicenseId)),
                new TableColumn(nameof(LicenseListEntry.Name)),
                new TableColumn(nameof(LicenseListEntry.Reference)).NoWrap()
            );

            foreach (var entry in tableData)
            {
                table.AddRow(
                    entry.LicenseId,
                    entry.Name,
                    entry?.Reference?.ToString() ?? "Unknown"
                );
            }

            tables.Add(table);
        }

        var result = new Rows(tables);

        AnsiConsole.Write(result);
    }
}
