namespace BlankoCRM.Core.Exceptions;

/// <summary>
/// Исключение нарушения бизнес-конфликта.
/// </summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message, 409)
    {
    }
}
