using System.Text.RegularExpressions;
using Acepdx.Core.Models;

namespace Acepdx.Core.Interfaces;

public interface ISpdxTemplateProvider 
{
    public string GetVariable(VariableType type, string? defaultValue, Regex? validation);
    public bool GetOptional(ReadOnlySpan<char> optionalText);
}