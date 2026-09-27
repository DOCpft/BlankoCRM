namespace BlankoCRM.Application.Abstract;

/// <summary>
/// Проверяет параметры запуска VK Mini App и возвращает подтверждённый VK user ID.
/// </summary>
public interface IVkMiniAppAuthenticator
{
    long Authenticate(IReadOnlyDictionary<string, string> vkParams);
}
