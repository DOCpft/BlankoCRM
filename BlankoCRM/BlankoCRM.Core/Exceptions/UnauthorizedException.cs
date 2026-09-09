namespace BlankoCRM.Core.Exceptions;

/// <summary>
/// Исключение ошибки аутентификации.
/// </summary>
public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message) : base(message, 401)
    {
    }
}