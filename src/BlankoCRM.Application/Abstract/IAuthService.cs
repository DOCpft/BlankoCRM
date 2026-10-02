namespace BlankoCRM.Application.Abstract
{
    /// <summary>
    /// Сервис аутентификации.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Зарегистрировать нового мастера.
        /// </summary>
        Task RegisterAsync(string? name, string email, string password, string? organizationName);

        /// <summary>
        /// Выполнить вход и получить JWT-токен.
        /// </summary>
        Task<(string Token, string Name, string Email)> LoginAsync(string email, string password);

        /// <summary>
        /// Аутентификация через VK Mini App. Приоритетный метод для регистрации и авторизации на данный момент.
        /// При первом входе автоматически регистрирует пользователя.
        /// </summary>
        Task<(string Token, string Name, string Email)> AuthVkMiniAppAsync(
            IReadOnlyDictionary<string, string> vkParams,
            CancellationToken cancellationToken = default);
    }
}

