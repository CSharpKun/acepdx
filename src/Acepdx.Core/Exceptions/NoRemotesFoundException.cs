namespace Acepdx.Core.Exceptions;

public sealed class NoRemotesFoundException : Exception
{
    public NoRemotesFoundException(string message, Exception ex)
        : base(message, ex) { }

    public NoRemotesFoundException(string message)
        : base(message) { }

    public NoRemotesFoundException()
    {
    }
}