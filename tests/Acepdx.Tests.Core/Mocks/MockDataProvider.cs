using System.Text.RegularExpressions;
using Acepdx.Core.Interfaces;
using Acepdx.Core.Models;

namespace Acepdx.Tests.Core.Mocks;

public class MockDataProvider : ISpdxTemplateProvider
{
    public Dictionary<VariableType, string> VariableAnswers { get; set; } = [];
    public bool AddOptional { get; set; }

    public bool GetOptional(ReadOnlySpan<char> optionalText) => AddOptional;

    public string GetVariable(VariableType type, string? defaultValue, Regex? validation) => VariableAnswers.GetValueOrDefault(type) ?? "default";
}