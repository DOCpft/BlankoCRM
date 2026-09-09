using System.ComponentModel.DataAnnotations;

namespace BlankoCRM.Application.DTOs;

/// <summary>
/// DTO РґР»СЏ РѕС‚РѕР±СЂР°Р¶РµРЅРёСЏ СѓСЃР»СѓРіРё.
/// </summary>
public record ServiceDto(
    Guid Id,
    string Name,
    int DurationMinutes,
    decimal Price,
    string? Description,
    Guid? SalonId = null);

/// <summary>
/// Р—Р°РїСЂРѕСЃ РЅР° СЃРѕР·РґР°РЅРёРµ РЅРѕРІРѕР№ СѓСЃР»СѓРіРё.
/// </summary>
public record CreateServiceRequest(
    [Required(ErrorMessage = "РќР°Р·РІР°РЅРёРµ СѓСЃР»СѓРіРё РѕР±СЏР·Р°С‚РµР»СЊРЅРѕ")]
    [MaxLength(100, ErrorMessage = "РќР°Р·РІР°РЅРёРµ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 100 СЃРёРјРІРѕР»РѕРІ")]
    string Name,

    [Required(ErrorMessage = "Р”Р»РёС‚РµР»СЊРЅРѕСЃС‚СЊ РѕР±СЏР·Р°С‚РµР»СЊРЅР°")]
    [Range(1, 1440, ErrorMessage = "Р”Р»РёС‚РµР»СЊРЅРѕСЃС‚СЊ РґРѕР»Р¶РЅР° Р±С‹С‚СЊ РѕС‚ 1 РґРѕ 1440 РјРёРЅСѓС‚")]
    int DurationMinutes,

    [Required(ErrorMessage = "Р¦РµРЅР° РѕР±СЏР·Р°С‚РµР»СЊРЅР°")]
    [Range(0, 999999.99, ErrorMessage = "Р¦РµРЅР° РґРѕР»Р¶РЅР° Р±С‹С‚СЊ РѕС‚ 0 РґРѕ 999999.99")]
    decimal Price,

    [MaxLength(500, ErrorMessage = "РћРїРёСЃР°РЅРёРµ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 500 СЃРёРјРІРѕР»РѕРІ")]
    string? Description,
    Guid? SalonId = null);

/// <summary>
/// Р—Р°РїСЂРѕСЃ РЅР° РѕР±РЅРѕРІР»РµРЅРёРµ СЃСѓС‰РµСЃС‚РІСѓСЋС‰РµР№ СѓСЃР»СѓРіРё.
/// </summary>
public record UpdateServiceRequest(
    [MaxLength(100, ErrorMessage = "РќР°Р·РІР°РЅРёРµ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 100 СЃРёРјРІРѕР»РѕРІ")]
    string? Name,

    [Range(1, 1440, ErrorMessage = "Р”Р»РёС‚РµР»СЊРЅРѕСЃС‚СЊ РґРѕР»Р¶РЅР° Р±С‹С‚СЊ РѕС‚ 1 РґРѕ 1440 РјРёРЅСѓС‚")]
    int? DurationMinutes,

    [Range(0, 999999.99, ErrorMessage = "Р¦РµРЅР° РґРѕР»Р¶РЅР° Р±С‹С‚СЊ РѕС‚ 0 РґРѕ 999999.99")]
    decimal? Price,

    [MaxLength(500, ErrorMessage = "РћРїРёСЃР°РЅРёРµ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 500 СЃРёРјРІРѕР»РѕРІ")]
    string? Description,
    Guid? SalonId = null);

