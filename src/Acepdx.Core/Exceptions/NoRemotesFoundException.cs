namespace Acepdx.Core.Exceptions;

public sealed class NoRemotesFoundException : AcepdxException
{
    public NoRemotesFoundException(string message, Exception ex)
        : base(message, ex) { }

    public NoRemotesFoundException(string message)
        : base(message) { }
}
