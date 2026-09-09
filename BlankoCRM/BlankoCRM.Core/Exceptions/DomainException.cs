namespace BlankoCRM.Core.Exceptions;

/// <summary>
/// Базовое исключение предметной области.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// HTTP-статус ответа для исключения.
    /// </summary>
    public int StatusCode { get; }
}
