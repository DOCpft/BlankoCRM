using System.ComponentModel.DataAnnotations;

namespace BlankoCRM.Application.DTOs;

/// <summary>
/// DTO для отображения услуги.
/// </summary>
public record ServiceDto(
    Guid Id,
    string Name,
    int DurationMinutes,
    decimal Price,
    string? Description);

/// <summary>
/// Запрос на создание новой услуги.
/// </summary>
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
    string? Description);

/// <summary>
/// Запрос на обновление существующей услуги.
/// </summary>
public record UpdateServiceRequest(
    [MaxLength(100, ErrorMessage = "Название не может быть длиннее 100 символов")]
    string? Name,

    [Range(1, 1440, ErrorMessage = "Длительность должна быть от 1 до 1440 минут")]
    int? DurationMinutes,

    [Range(0, 999999.99, ErrorMessage = "Цена должна быть от 0 до 999999.99")]
    decimal? Price,

    [MaxLength(500, ErrorMessage = "Описание не может быть длиннее 500 символов")]
    string? Description);
