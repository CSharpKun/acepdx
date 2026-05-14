using System.Text.RegularExpressions;

namespace Licensify.Core.Interfaces;

public interface ISpdxTemplateProvider 
{
    public string GetVariable(VariableType type, string? defaultValue, Regex? validation);
    public bool GetOptional(ReadOnlySpan<char> optionalText);
}