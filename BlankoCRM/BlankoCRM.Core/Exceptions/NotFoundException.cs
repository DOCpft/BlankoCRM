namespace BlankoCRM.Core.Exceptions;

/// <summary>
/// Исключение отсутствующей сущности.
/// </summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message, 404)
    {
    }
}