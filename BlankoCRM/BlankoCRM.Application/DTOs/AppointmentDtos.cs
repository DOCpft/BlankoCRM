using System.ComponentModel.DataAnnotations;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// DTO РґР»СЏ РѕС‚РѕР±СЂР°Р¶РµРЅРёСЏ Р·Р°РїРёСЃРё РЅР° РїСЂРёС‘Рј.
    /// </summary>
    public record AppointmentServiceDto(
        Guid ServiceId,
        string? ServiceName,
        decimal Price,
        int DurationMinutes,
        int Quantity);

    public record AppointmentDto(
        Guid Id,
        Guid ClientId,
        string ClientName,
        List<AppointmentServiceDto> Services,
        DateTime StartTime,
        DateTime EndTime,
        AppointmentStatus Status,
        string? Notes,
        decimal TotalPrice,
        int TotalDurationMinutes);

    /// <summary>
    /// Р—Р°РїСЂРѕСЃ РЅР° СЃРѕР·РґР°РЅРёРµ РЅРѕРІРѕР№ Р·Р°РїРёСЃРё.
    /// </summary>
    public record CreateAppointmentRequest(
        [Required(ErrorMessage = "ID РєР»РёРµРЅС‚Р° РѕР±СЏР·Р°С‚РµР»РµРЅ")]
        Guid ClientId,

        [Required(ErrorMessage = "РќРµРѕР±С…РѕРґРёРјРѕ СѓРєР°Р·Р°С‚СЊ С…РѕС‚СЏ Р±С‹ РѕРґРЅСѓ СѓСЃР»СѓРіСѓ")]
        [MinLength(1, ErrorMessage = "Р”РѕР»Р¶РЅР° Р±С‹С‚СЊ СѓРєР°Р·Р°РЅР° С…РѕС‚СЏ Р±С‹ РѕРґРЅР° СѓСЃР»СѓРіР°")]
        List<ServiceInput> Services,   // в†ђ СЃРїРёСЃРѕРє СЃ РёРЅРґРёРІРёРґСѓР°Р»СЊРЅС‹РјРё РїР°СЂР°РјРµС‚СЂР°РјРё

        [Required(ErrorMessage = "Р’СЂРµРјСЏ РЅР°С‡Р°Р»Р° РѕР±СЏР·Р°С‚РµР»СЊРЅРѕ")]
        DateTime StartTime,

        [MaxLength(500, ErrorMessage = "Р—Р°РјРµС‚РєРё РЅРµ РјРѕРіСѓС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 500 СЃРёРјРІРѕР»РѕРІ")]
        string? Notes,

        Guid? SalonId = null
    );

    public record ServiceInput(
        Guid ServiceId,
        int? DurationMinutes = null,
        decimal? Price = null,
        int Quantity = 1
    );

    /// <summary>
    /// Р—Р°РїСЂРѕСЃ РЅР° РѕР±РЅРѕРІР»РµРЅРёРµ СЃСѓС‰РµСЃС‚РІСѓСЋС‰РµР№ Р·Р°РїРёСЃРё.
    /// </summary>
    public record UpdateAppointmentRequest(
        DateTime? StartTime,
        AppointmentStatus? Status,

        [MaxLength(500, ErrorMessage = "Р—Р°РјРµС‚РєРё РЅРµ РјРѕРіСѓС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 500 СЃРёРјРІРѕР»РѕРІ")]
        string? Notes);
}

