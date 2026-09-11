using System.ComponentModel.DataAnnotations;

namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// Запрос на регистрацию нового мастера (пользователя).
    /// </summary>
    /// <param name="Name">Имя пользователя (не более 100 символов).</param>
    /// <param name="Email">Email. Обязателен, должен быть корректным.</param>
    /// <param name="Password">Пароль (не менее 6 символов).</param>
    /// <param name="OrganizationName">Название организации (не более 100 символов).</param>
    public record RegisterUserRequest(
        [MaxLength(100, ErrorMessage = "Имя не может быть длиннее 100 символов")]
        string? Name,

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [MaxLength(100, ErrorMessage = "Email не может быть длиннее 100 символов")]
        string Email,

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        [MaxLength(100, ErrorMessage = "Пароль не может быть длиннее 100 символов")]
        string Password,

        [MaxLength(100, ErrorMessage = "Название организации не может быть длиннее 100 символов")]
        string? OrganizationName);

    /// <summary>
    /// Запрос на вход в систему.
    /// </summary>
    /// <param name="Email">Email пользователя. Обязателен.</param>
    /// <param name="Password">Пароль пользователя. Обязателен.</param>
    public record LoginUserRequest(
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        string Email,

        [Required(ErrorMessage = "Пароль обязателен")]
        string Password);

    /// <summary>
    /// Ответ с токеном аутентификации.
    /// </summary>
    /// <param name="Token">JWT-токен доступа.</param>
    /// <param name="Name">Имя пользователя.</param>
    /// <param name="Email">Email пользователя.</param>
    public record AuthResponse(string Token, string Name, string Email);
}