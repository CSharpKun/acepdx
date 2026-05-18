using System.Text.RegularExpressions;
using DotMake.CommandLine;
using Acepdx.Core;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;
using Spectre.Console;

namespace Acepdx.CLI.Commands;

[CliCommand(
    Description = "Applies specified license to the specified project",
    Order = 3,
    Parent = typeof(MainCommand)
)]
public class ApplyCommand(ILicenseParser parser, ILicenseHttpService httpService) : ISpdxTemplateProvider
{
    [CliOption(
        Description = "Assume yes",
        Group = "assume",
        Alias = "-y"
    )]
    public bool AssumeYes { get; set; }

    [CliOption(
        Description = "Assume no",
        Group = "assume",
        Alias = "-n"
    )]
    public bool AssumeNo { get; set; }

    [CliArgument(Description = "License Id", Required = true)]
    public required string LicenseId { get; set; } 

    [CliArgument(Description = "Path to the repository", Name = "repo")]
    public DirectoryInfo RepositoryPath { get; set; } = new(Environment.CurrentDirectory); 

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

    public string GetVariable(VariableType type, string? defaultValue, Regex? validation)
    { 
        string message = type switch 
        {
            VariableType.Copyright => "Enter your name:",
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
        var result = AnsiConsole.Ask<string>(message);
        while (!validation?.IsMatch(result) ?? false) 
        {
            AnsiConsole.Markup("[red]Incorrect data format. Please try again.[/]");
            result = AnsiConsole.Ask<string>(message, defaultValue ?? "");
        }
        return result;
    }

    public bool GetOptional(ReadOnlySpan<char> optionalText)
    {
        if (AssumeYes) return true;
        if (AssumeNo) return false;
        return AnsiConsole.Ask("Would you like to add this optional part in your license: \"" + optionalText.ToString() + "\"?", true);
    }
}