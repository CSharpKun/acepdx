using System.Buffers;

namespace Acepdx.Core.Services;

public sealed record HeaderPrefix(string Top, string Mid, string Bottom);

public sealed class FileHeadersService
{
    public HeaderPrefix GetHeaderPrefixes(string extension) =>
        extension.ToLowerInvariant() switch
        {
            ".c" or ".h" or ".java" or ".scala" or ".kt" or ".kts" => new("/*", " * ", " */"),

            ".js"
            or ".mjs"
            or ".cjs"
            or ".jsx"
            or ".tsx"
            or ".css"
            or ".scss"
            or ".sass"
            or ".ts" => new("/**", " * ", " */"),

            ".cc"
            or ".cpp"
            or ".cs"
            or ".go"
            or ".hcl"
            or ".hh"
            or ".hpp"
            or ".m"
            or ".mm"
            or ".proto"
            or ".rs"
            or ".swift"
            or ".dart"
            or ".groovy"
            or ".v"
            or ".sv"
            or ".php"
            or ".gv"
            or ".fs"
            or ".fsi" => new(string.Empty, "// ", string.Empty),

            ".py"
            or ".sh"
            or ".yaml"
            or ".yml"
            or ".dockerfile"
            or "dockerfile"
            or ".rb"
            or "gemfile"
            or ".tcl"
            or ".tf"
            or ".bzl"
            or ".pl"
            or ".pp"
            or "build"
            or ".build"
            or ".toml"
            or ".cmake" => new(string.Empty, "# ", string.Empty),

            ".el" or ".lisp" => new(string.Empty, ";; ", string.Empty),

            ".erl" or ".tex" or ".cls" or ".sty" => new(string.Empty, "% ", string.Empty),

            ".hs" or ".sql" or ".sdl" => new(string.Empty, "-- ", string.Empty),

            ".html" or ".xml" or ".xaml" or ".axaml" or ".vue" or ".wxi" or ".wxl" or ".wxs" => new(
                "<!--",
                " ",
                "-->"
            ),

            ".j2" => new("{#", " # ", "#}"),

            ".ml" or ".mli" or ".mll" or ".mly" => new("(**", " * ", "*)"),

            _ => throw new NotSupportedException("Unsupported file type"),
        };
}
