namespace Acepdx.Core.Exceptions;

public class AllRemotesTriedException : Exception
{
    public AllRemotesTriedException(string message, Exception ex)
        : base(message, ex) { }

    public AllRemotesTriedException(string message)
        : base(message) { }

    public AllRemotesTriedException()
    {
    }
}