using System.ComponentModel.DataAnnotations;

namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// DTO РґР»СЏ РѕС‚РѕР±СЂР°Р¶РµРЅРёСЏ РєР»РёРµРЅС‚Р°.
    /// </summary>
    public record ClientDto(
        Guid Id,
        string Name,
        string Phone,
        string? Telegram,
        DateTime? BirthDate,
        string? Notes,
        Guid? SalonId = null);

    /// <summary>
    /// Р—Р°РїСЂРѕСЃ РЅР° СЃРѕР·РґР°РЅРёРµ РЅРѕРІРѕРіРѕ РєР»РёРµРЅС‚Р°.
    /// </summary>
    public record CreateClientRequest(
        [Required(ErrorMessage = "РРјСЏ РєР»РёРµРЅС‚Р° РѕР±СЏР·Р°С‚РµР»СЊРЅРѕ")]
        [MaxLength(200, ErrorMessage = "РРјСЏ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 200 СЃРёРјРІРѕР»РѕРІ")]
        string Name,

        [Required(ErrorMessage = "РўРµР»РµС„РѕРЅ РѕР±СЏР·Р°С‚РµР»РµРЅ")]
        [MaxLength(20, ErrorMessage = "РўРµР»РµС„РѕРЅ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 20 СЃРёРјРІРѕР»РѕРІ")]
        [Phone(ErrorMessage = "РќРµРєРѕСЂСЂРµРєС‚РЅС‹Р№ С„РѕСЂРјР°С‚ С‚РµР»РµС„РѕРЅР°")]
        string Phone,

        [MaxLength(100, ErrorMessage = "Telegram РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 100 СЃРёРјРІРѕР»РѕРІ")]
        string? Telegram,

        DateTime? BirthDate,

        [MaxLength(500, ErrorMessage = "Р—Р°РјРµС‚РєРё РЅРµ РјРѕРіСѓС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 500 СЃРёРјРІРѕР»РѕРІ")]
        string? Notes,
        Guid? SalonId = null);

    /// <summary>
    /// Р—Р°РїСЂРѕСЃ РЅР° РѕР±РЅРѕРІР»РµРЅРёРµ СЃСѓС‰РµСЃС‚РІСѓСЋС‰РµРіРѕ РєР»РёРµРЅС‚Р°.
    /// </summary>
    public record UpdateClientRequest(
        [MaxLength(200, ErrorMessage = "РРјСЏ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 200 СЃРёРјРІРѕР»РѕРІ")]
        string? Name,

        [MaxLength(20, ErrorMessage = "РўРµР»РµС„РѕРЅ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 20 СЃРёРјРІРѕР»РѕРІ")]
        [Phone(ErrorMessage = "РќРµРєРѕСЂСЂРµРєС‚РЅС‹Р№ С„РѕСЂРјР°С‚ С‚РµР»РµС„РѕРЅР°")]
        string? Phone,

        [MaxLength(100, ErrorMessage = "Telegram РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 100 СЃРёРјРІРѕР»РѕРІ")]
        string? Telegram,

        DateTime? BirthDate,

        [MaxLength(500, ErrorMessage = "Р—Р°РјРµС‚РєРё РЅРµ РјРѕРіСѓС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 500 СЃРёРјРІРѕР»РѕРІ")]
        string? Notes,
        Guid? SalonId = null);
}

