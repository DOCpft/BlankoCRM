using Microsoft.AspNetCore.Identity;

namespace BlankoCRM.Core.Entities;

/// <summary>
/// Пользователь системы на базе ASP.NET Core Identity.
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    public Guid OrganizationId { get; set; }
    public Guid? SalonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? VkUserId { get; set; }

    public Organization Organization { get; set; } = null!;
    public Salon? Salon { get; set; }
    public MasterProfile? MasterProfile { get; set; }
}
