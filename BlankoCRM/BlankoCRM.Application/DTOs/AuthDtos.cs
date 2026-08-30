using System.ComponentModel.DataAnnotations;

namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// Запрос на регистрацию нового мастера.
    /// </summary>
    public record RegisterRequest(
        [MaxLength(100, ErrorMessage = "Имя не может быть длиннее 100 символов")]
        string? Name,

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [MaxLength(100, ErrorMessage = "Email не может быть длиннее 100 символов")]
        string Email,

        [Required(ErrorMessage = "Пароль обязателен")]
        [MinLength(6, ErrorMessage = "Пароль должен быть не менее 6 символов")]
        [MaxLength(100, ErrorMessage = "Пароль не может быть длиннее 100 символов")]
        string Password);

    /// <summary>
    /// Запрос на вход в систему.
    /// </summary>
    public record LoginRequest(
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        string Email,

        [Required(ErrorMessage = "Пароль обязателен")]
        string Password);

    /// <summary>
    /// Ответ с токеном аутентификации.
    /// </summary>
    public record AuthResponse(string Token, string Name, string Email);
}
