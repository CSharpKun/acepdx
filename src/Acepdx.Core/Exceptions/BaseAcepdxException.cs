namespace Acepdx.Core.Exceptions;

public abstract class AcepdxException : Exception 
{
    public string UserMessage { get; }

    public AcepdxException(string userMessage, string technicalMessage) 
        : base(technicalMessage)
    {
        UserMessage = userMessage;
    }

    public AcepdxException(string userMessage, string technicalMessage, Exception inner) 
        : base(technicalMessage, inner)
    {
        UserMessage = userMessage;
    }
}