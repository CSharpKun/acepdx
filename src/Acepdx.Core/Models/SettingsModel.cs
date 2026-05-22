namespace Acepdx.Core.Models;

public enum VariableType 
{
    Copyright,
    Other
}

public class SpdxRemote
{
    public required Uri Url { get; set; }
}