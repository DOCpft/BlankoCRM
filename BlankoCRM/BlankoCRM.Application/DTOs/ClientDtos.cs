using System.ComponentModel.DataAnnotations;

namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// DTO для отображения клиента.
    /// </summary>
    public record ClientDto(
        Guid Id,
        string Name,
        string Phone,
        string? Telegram,
        DateTime? BirthDate,
        string? Notes);

    /// <summary>
    /// Запрос на создание нового клиента.
    /// </summary>
    public record CreateClientRequest(
        [Required(ErrorMessage = "Имя клиента обязательно")]
        [MaxLength(200, ErrorMessage = "Имя не может быть длиннее 200 символов")]
        string Name,

        [Required(ErrorMessage = "Телефон обязателен")]
        [MaxLength(20, ErrorMessage = "Телефон не может быть длиннее 20 символов")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        string Phone,

        [MaxLength(100, ErrorMessage = "Telegram не может быть длиннее 100 символов")]
        string? Telegram,

        DateTime? BirthDate,

        [MaxLength(500, ErrorMessage = "Заметки не могут быть длиннее 500 символов")]
        string? Notes);

    /// <summary>
    /// Запрос на обновление существующего клиента.
    /// </summary>
    public record UpdateClientRequest(
        [MaxLength(200, ErrorMessage = "Имя не может быть длиннее 200 символов")]
        string? Name,

        [MaxLength(20, ErrorMessage = "Телефон не может быть длиннее 20 символов")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        string? Phone,

        [MaxLength(100, ErrorMessage = "Telegram не может быть длиннее 100 символов")]
        string? Telegram,

        DateTime? BirthDate,

        [MaxLength(500, ErrorMessage = "Заметки не могут быть длиннее 500 символов")]
        string? Notes);
}
