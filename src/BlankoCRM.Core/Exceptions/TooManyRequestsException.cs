namespace BlankoCRM.Core.Exceptions;

/// <summary>
/// Превышен лимит публичных запросов.
/// </summary>
public sealed class TooManyRequestsException : DomainException
{
    public TooManyRequestsException(string message) : base(message, 429)
    {
    }
}
