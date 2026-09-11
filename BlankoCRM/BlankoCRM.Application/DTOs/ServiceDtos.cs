using System.ComponentModel.DataAnnotations;

namespace BlankoCRM.Application.DTOs;

/// <summary>
/// DTO для отображения услуги.
/// </summary>
/// <param name="Id">Идентификатор услуги.</param>
/// <param name="Name">Название услуги.</param>
/// <param name="DurationMinutes">Длительность услуги в минутах.</param>
/// <param name="Price">Цена услуги.</param>
/// <param name="Description">Описание услуги.</param>
/// <param name="SalonId">Идентификатор салона (необязательно).</param>
public record ServiceDto(
    Guid Id,
    string Name,
    int DurationMinutes,
    decimal Price,
    string? Description,
    Guid? SalonId = null);

/// <summary>
/// Запрос на создание новой услуги.
/// </summary>
/// <param name="Name">Название услуги. Обязательно (не более 100 символов).</param>
/// <param name="DurationMinutes">Длительность в минутах (от 1 до 1440).</param>
/// <param name="Price">Цена (от 0 до 999999.99).</param>
/// <param name="Description">Описание (не более 500 символов).</param>
/// <param name="SalonId">Идентификатор салона (необязательно).</param>
public record CreateServiceRequest(
    [Required(ErrorMessage = "Название услуги обязательно")]
    [MaxLength(100, ErrorMessage = "Название не может быть длиннее 100 символов")]
    string Name,

    [Required(ErrorMessage = "Длительность обязательна")]
    [Range(1, 1440, ErrorMessage = "Длительность должна быть от 1 до 1440 минут")]
    int DurationMinutes,

    [Required(ErrorMessage = "Цена обязательна")]
    [Range(0, 999999.99, ErrorMessage = "Цена должна быть от 0 до 999999.99")]
    decimal Price,

    [MaxLength(500, ErrorMessage = "Описание не может быть длиннее 500 символов")]
    string? Description,
    Guid? SalonId = null);

/// <summary>
/// Запрос на обновление существующей услуги.
/// </summary>
/// <param name="Name">Название услуги (не более 100 символов).</param>
/// <param name="DurationMinutes">Длительность в минутах (от 1 до 1440).</param>
/// <param name="Price">Цена (от 0 до 999999.99).</param>
/// <param name="Description">Описание (не более 500 символов).</param>
/// <param name="SalonId">Идентификатор салона.</param>
public record UpdateServiceRequest(
    [MaxLength(100, ErrorMessage = "Название не может быть длиннее 100 символов")]
    string? Name,

    [Range(1, 1440, ErrorMessage = "Длительность должна быть от 1 до 1440 минут")]
    int? DurationMinutes,

    [Range(0, 999999.99, ErrorMessage = "Цена должна быть от 0 до 999999.99")]
    decimal? Price,

    [MaxLength(500, ErrorMessage = "Описание не может быть длиннее 500 символов")]
    string? Description,
    Guid? SalonId = null);