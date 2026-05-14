namespace Licensify.Core.Exceptions;

public class AllRemotesTriedException : BaseApplicationException 
{
    public AllRemotesTriedException(string userMessage, string technicalMessage) 
        : base(userMessage, technicalMessage) {}

    public AllRemotesTriedException(string userMessage, string technicalMessage, Exception inner) 
        : base(userMessage, technicalMessage, inner) {}
}