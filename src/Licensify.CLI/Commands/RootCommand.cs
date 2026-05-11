using DotMake.CommandLine;

namespace Licensify.CLI.Commands;

[CliCommand( 
    Description = "SPDX Client that can automatically manage licenses for any projects",
    ShortFormAutoGenerate = CliNameAutoGenerate.Options
)]
public class RootCommand
{
    [CliOption(Description = "Enable verbose logging")]
    public bool Verbose { get; set; }

    [CliOption(Description = "Force download and update for operation")]
    public bool NoCache { get; set; }
}