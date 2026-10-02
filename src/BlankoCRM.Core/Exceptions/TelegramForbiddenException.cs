namespace BlankoCRM.Core.Exceptions;

/// <summary>
/// Исключение, сигнализирующее, что Telegram-бот заблокирован пользователем (HTTP 403).
/// </summary>
public sealed class TelegramForbiddenException : DomainException
{
    public TelegramForbiddenException(string message) : base(message, 403)
    {
    }
}
