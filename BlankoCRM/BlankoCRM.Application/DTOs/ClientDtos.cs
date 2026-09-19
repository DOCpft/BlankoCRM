using System.ComponentModel.DataAnnotations;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.DTOs
{
    public record ClientContactDto(
        ContactType Type,

        [Required(ErrorMessage = "Значение контакта обязательно")]
        [MaxLength(200, ErrorMessage = "Контакт не может быть длиннее 200 символов")]
        string Value);

    /// <summary>
    /// DTO для отображения клиента.
    /// </summary>
    /// <param name="Id">Идентификатор клиента.</param>
    /// <param name="Name">Имя клиента.</param>
    /// <param name="Phone">Телефон клиента (необязательно).</param>
    /// <param name="Contacts">Контакты клиента.</param>
    /// <param name="BirthDate">Дата рождения клиента (необязательно).</param>
    /// <param name="Notes">Заметки о клиенте.</param>
    /// <param name="SalonId">Идентификатор салона (необязательно).</param>
    public record ClientDto(
        Guid Id,
        string Name,
        string? Phone,
        List<ClientContactDto> Contacts,
        DateTime? BirthDate,
        string? Notes,
        Guid? SalonId = null);

    /// <summary>
    /// Запрос на создание нового клиента.
    /// </summary>
    /// <param name="Name">Имя клиента. Обязательно (не более 200 символов).</param>
    /// <param name="Phone">Телефон клиента (не более 20 символов).</param>
    /// <param name="Contacts">Контакты клиента (необязательно).</param>
    /// <param name="BirthDate">Дата рождения клиента (необязательно).</param>
    /// <param name="Notes">Заметки (не более 500 символов).</param>
    /// <param name="SalonId">Идентификатор салона (необязательно).</param>
    public record CreateClientRequest(
        [Required(ErrorMessage = "Имя клиента обязательно")]
        [MaxLength(200, ErrorMessage = "Имя не может быть длиннее 200 символов")]
        string Name,

        [MaxLength(20, ErrorMessage = "Телефон не может быть длиннее 20 символов")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        string? Phone,

        List<ClientContactDto>? Contacts,

        DateTime? BirthDate,

        [MaxLength(500, ErrorMessage = "Заметки не могут быть длиннее 500 символов")]
        string? Notes,
        Guid? SalonId = null);

    /// <summary>
    /// Запрос на обновление существующего клиента.
    /// </summary>
    /// <param name="Name">Имя клиента (не более 200 символов).</param>
    /// <param name="Phone">Телефон клиента (не более 20 символов).</param>
    /// <param name="Contacts">Если null, контакты не изменяются; пустой список удаляет все контакты.</param>
    /// <param name="BirthDate">Дата рождения клиента.</param>
    /// <param name="Notes">Заметки (не более 500 символов).</param>
    /// <param name="SalonId">Идентификатор салона.</param>
    public record UpdateClientRequest(
        [MaxLength(200, ErrorMessage = "Имя не может быть длиннее 200 символов")]
        string? Name,

        [MaxLength(20, ErrorMessage = "Телефон не может быть длиннее 20 символов")]
        [Phone(ErrorMessage = "Некорректный формат телефона")]
        string? Phone,

        List<ClientContactDto>? Contacts,

        DateTime? BirthDate,

        [MaxLength(500, ErrorMessage = "Заметки не могут быть длиннее 500 символов")]
        string? Notes,
        Guid? SalonId = null);
}