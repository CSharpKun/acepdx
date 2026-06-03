namespace Acepdx.Core.Exceptions;

public abstract class AcepdxException : Exception
{
    public AcepdxException(string message, Exception ex)
        : base(message, ex) { }

    public AcepdxException(string message)
        : base(message) { }
}
