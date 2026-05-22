namespace Acepdx.Core.Exceptions;

public class NoRemotesFoundException : AcepdxException 
{
    public NoRemotesFoundException(string userMessage, string technicalMessage) 
        : base(userMessage, technicalMessage) {}

    public NoRemotesFoundException(string userMessage, string technicalMessage, Exception inner) 
        : base(userMessage, technicalMessage, inner) {}
}