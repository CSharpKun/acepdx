using System.Text.Json;
using DotMake.CommandLine;
using Licensify.Core;
using Licensify.Core.Exceptions;
using Licensify.Core.Interfaces;
using Licensify.Core.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using static System.Threading.Thread;

namespace Licensify.CLI.Commands;

[CliCommand(
    Description = "Lists all licenses in a table",
    Order = 1,
    Parent = typeof(MainCommand)
)]
public class ListCommand(ILogger<ListCommand> logger, ILicenseHttpService httpService)
{
    public async Task RunAsync()
    {
        List<LicenseList>? lists = null;
        
        try 
        {
            lists = await httpService.GetLicenseLists();
        }
        catch (AllRemotesTriedException ex) 
        {
            logger.LogCritical(ex, "All remotes were tried.");
            AnsiConsole.MarkupLine($"[bold red]{ex.UserMessage}[/]");
            return;
        } 

        if (lists is null)
        {
            AnsiConsole.MarkupLine("[bold red]Couldn't get list of licenses. Check your internet connection.[/]");
            return;
        }

        List<Table> tables = [];

        foreach (var list in lists)
        {
            if (list.Remote is null) 
            {
                logger.LogError("The list does not contain a name, but still exists. Using default name.");
                list.Remote = "Unknown";
            }

            var titleName = CurrentThread.CurrentCulture.TextInfo.ToTitleCase(list.Remote);

            var table = new Table().RoundedBorder().Title($"{titleName} Licenses");

            var tableData = list.Licenses
                .Where(license => license.IsDeprecatedLicenseId is false)
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