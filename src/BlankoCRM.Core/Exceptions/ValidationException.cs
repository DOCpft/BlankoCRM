namespace BlankoCRM.Core.Exceptions;

/// <summary>
/// Исключение нарушения бизнес-валидации.
/// </summary>
public sealed class ValidationException : DomainException
{
    public ValidationException(string message) : base(message, 400)
    {
    }
}