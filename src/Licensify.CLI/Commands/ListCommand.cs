using DotMake.CommandLine;
using Licensify.Core;
using Licensify.Core.Interfaces;
using Licensify.Core.Services;
using Spectre.Console;

namespace Licensify.Commands;

[CliCommand(
    Description = "Lists all licenses in a table."
)]
public class ListCommand(ILicenseHttpService httpService)
{
    public async Task RunAsync()
    {
        var lists = await httpService.GetLicenseLists();

        if (lists is null)
        {
            AnsiConsole.MarkupLine("[bold red]Couldn't get list of licenses. Check your internet connection.[/]");
            return;
        }

        var table = new Table().RoundedBorder().Title("SPDX Licenses");
        
        table.AddColumns(
            new TableColumn(nameof(LicenseListEntry.LicenseId)).NoWrap(),
            new TableColumn(nameof(LicenseListEntry.Name)), 
            new TableColumn(nameof(LicenseListEntry.Reference)).NoWrap()
        );

        List<LicenseListEntry> mergedList = [];

        foreach (var list in lists) 
        {
            mergedList.AddRange(list.Licenses);
        }

        var tableData = mergedList
            .Where(license => license.IsDeprecatedLicenseId is false)
            .OrderBy(license => license.Name);

        foreach (var entry in tableData)
        {
            table.AddRow(
                entry.Name,
                entry.LicenseId,
                entry.Reference
            );
        }

        AnsiConsole.Write(table);
    }
}