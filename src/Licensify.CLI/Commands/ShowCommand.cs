using DotMake.CommandLine;
using Licensify.Core;
using Licensify.Core.Interfaces;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Licensify.CLI.Commands;

[CliCommand(
    Description = "Shows information about specified license",
    Order = 2,
    Parent = typeof(RootCommand),
    Alias = "get"
)]
public class ShowCommand(ILicenseHttpService httpService)
{
    [CliArgument(Description = "License Id", Required = true)]
    public string LicenseId { get; set; } = null!;

    public async Task RunAsync()
    {
        var lists = await httpService.GetLicenseLists();

        List<LicenseListEntry> entries = [];

        foreach (var list in lists) 
        {
            entries.AddRange(list.Licenses.Where(license => license.LicenseId == LicenseId));
        }

        LicenseListEntry entry;

        if (entries.Count < 1) 
        {
            return;
        }
        else if (entries.Count > 1) entry = AskForLicense(entries);
        else entry = entries.First();

        var license = await httpService.GetLicense(entry);

        if (license is null)
        {
            AnsiConsole.MarkupLine($"[bold red]Couldn't get {LicenseId} license. Check your internet connection.[/]");
            return;
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
        return AnsiConsole.Prompt<LicenseListEntry>(
            new SelectionPrompt<LicenseListEntry>()
                .Title("Multiple licenses found under the same name, choose:")
                .UseConverter(entry => $"{entry.LicenseId} (from {entry.Remote})")
                .AddChoices(entries)
        );
    }

    private static string GetStatusColorTag(bool condition, bool reverse = false) => condition ^ reverse ? "[green]" : "[red]";
}