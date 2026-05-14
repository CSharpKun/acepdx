namespace Licensify.Core.Exceptions;

public abstract class BaseApplicationException : Exception 
{
    public string UserMessage { get; }

    public BaseApplicationException(string userMessage, string technicalMessage) 
        : base(technicalMessage)
    {
        UserMessage = userMessage;
    }

    public BaseApplicationException(string userMessage, string technicalMessage, Exception inner) 
        : base(technicalMessage, inner)
    {
        UserMessage = userMessage;
    }
}