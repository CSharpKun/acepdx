namespace Acepdx.Core.Exceptions;

<<<<<<< Updated upstream
public sealed class NoRemotesFoundException : AcepdxException
{
    public NoRemotesFoundException(string message, Exception ex)
        : base(message, ex) { }

    public NoRemotesFoundException(string message)
        : base(message) { }
=======
public class NoRemotesFoundException : AcepdxException
{
    public NoRemotesFoundException(string userMessage, string technicalMessage)
        : base(userMessage, technicalMessage) { }

    public NoRemotesFoundException(string userMessage, string technicalMessage, Exception inner)
        : base(userMessage, technicalMessage, inner) { }
>>>>>>> Stashed changes
}
