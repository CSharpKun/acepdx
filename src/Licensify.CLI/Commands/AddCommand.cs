using System.Text.RegularExpressions;
using DotMake.CommandLine;
using Licensify.Core;
using Licensify.Core.Interfaces;
using Spectre.Console;

namespace Licensify.CLI.Commands;

[CliCommand(
    Description = "Adds specified license to the specified project",
    Order = 3,
    Parent = typeof(RootCommand)
)]
public class AddCommand(ILicenseParser parser, ILicenseHttpService httpService) : IDataProvider
{
    [CliArgument(Description = "License Id", Required = true)]
    public string LicenseId { get; set; } = null!;

    [CliArgument(Description = "Path to the repository", Name = "repo")]
    public string RepositoryPath { get; set; } = "."; 

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
            return;
        }
        
        parser.Parse(this, license);
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

    private bool GetOptionalParts(string optionalText)
    {
        return AnsiConsole.Ask("Would you like to add this optional part in your license: \"" + optionalText + "\"?", true);
    }

    public string GetData(DataQueryType type, string? defaultValue = null, Regex? validation = null)
    {
        string message = type switch 
        {
            DataQueryType.Username => "Enter your name:",
            _ => "default"
        };
        var result = AnsiConsole.Ask<string>(message);
        while (!validation?.IsMatch(result) ?? false) 
        {
            AnsiConsole.Markup("[red]Incorrect data format. Please try again.");
            result = AnsiConsole.Ask<string>(message, defaultValue ?? "");
        }
        return result;
    }

    public bool GetFlag(FlagQueryType type, string? textToChange = null)
    {
        throw new NotImplementedException();
    }
}