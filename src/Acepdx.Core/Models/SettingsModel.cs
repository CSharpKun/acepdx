namespace Acepdx.Core.Models;

public enum VariableType
{
    Copyright,
    Other,
}

public record SpdxRemote
{
    public required Uri Url { get; set; }
}
