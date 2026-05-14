using System.Text.RegularExpressions;
using Licensify.Core;
using Licensify.Core.Interfaces;

namespace Licensify.Tests.Core.Mocks;

public class MockDataProvider : ISpdxTemplateProvider
{
    public Dictionary<VariableType, string> VariableAnswers { get; set; } = [];
    public bool AddOptional { get; set; }

    public bool GetOptional(ReadOnlySpan<char> optionalText) => AddOptional;

    public string GetVariable(VariableType type, string? defaultValue, Regex? validation) => VariableAnswers.GetValueOrDefault(type) ?? "default";
}