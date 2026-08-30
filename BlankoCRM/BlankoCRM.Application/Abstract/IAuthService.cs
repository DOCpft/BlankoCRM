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
        Task RegisterAsync(string? name, string email, string password);

        /// <summary>
        /// Выполнить вход и получить JWT-токен.
        /// </summary>
        Task<(string Token, string Name, string Email)> LoginAsync(string email, string password);
    }
}
