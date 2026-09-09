using System.ComponentModel.DataAnnotations;

namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// Р—Р°РїСЂРѕСЃ РЅР° СЂРµРіРёСЃС‚СЂР°С†РёСЋ РЅРѕРІРѕРіРѕ РјР°СЃС‚РµСЂР°.
    /// </summary>
    public record RegisterUserRequest(
        [MaxLength(100, ErrorMessage = "РРјСЏ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 100 СЃРёРјРІРѕР»РѕРІ")]
        string? Name,

        [Required(ErrorMessage = "Email РѕР±СЏР·Р°С‚РµР»РµРЅ")]
        [EmailAddress(ErrorMessage = "РќРµРєРѕСЂСЂРµРєС‚РЅС‹Р№ С„РѕСЂРјР°С‚ email")]
        [MaxLength(100, ErrorMessage = "Email РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 100 СЃРёРјРІРѕР»РѕРІ")]
        string Email,

        [Required(ErrorMessage = "РџР°СЂРѕР»СЊ РѕР±СЏР·Р°С‚РµР»РµРЅ")]
        [MinLength(6, ErrorMessage = "РџР°СЂРѕР»СЊ РґРѕР»Р¶РµРЅ Р±С‹С‚СЊ РЅРµ РјРµРЅРµРµ 6 СЃРёРјРІРѕР»РѕРІ")]
        [MaxLength(100, ErrorMessage = "РџР°СЂРѕР»СЊ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РґР»РёРЅРЅРµРµ 100 СЃРёРјРІРѕР»РѕРІ")]
        string Password,

        [MaxLength(100, ErrorMessage = "Название организации не может быть длиннее 100 символов")]
        string? OrganizationName);

    /// <summary>
    /// Р—Р°РїСЂРѕСЃ РЅР° РІС…РѕРґ РІ СЃРёСЃС‚РµРјСѓ.
    /// </summary>
    public record LoginUserRequest(
        [Required(ErrorMessage = "Email РѕР±СЏР·Р°С‚РµР»РµРЅ")]
        [EmailAddress(ErrorMessage = "РќРµРєРѕСЂСЂРµРєС‚РЅС‹Р№ С„РѕСЂРјР°С‚ email")]
        string Email,

        [Required(ErrorMessage = "РџР°СЂРѕР»СЊ РѕР±СЏР·Р°С‚РµР»РµРЅ")]
        string Password);

    /// <summary>
    /// РћС‚РІРµС‚ СЃ С‚РѕРєРµРЅРѕРј Р°СѓС‚РµРЅС‚РёС„РёРєР°С†РёРё.
    /// </summary>
    public record AuthResponse(string Token, string Name, string Email);
}

