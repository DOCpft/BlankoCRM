using BlankoCRM.Core.Interfaces;

namespace BlankoCRM.Core.Entities;

/// <summary>
/// Профиль мастера, связанный с пользователем Identity.
/// </summary>
public class MasterProfile : IOrganizationEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public Guid? SalonId { get; set; }
    public string? Specialization { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? BirthDate { get; set; }

    public Organization Organization { get; set; } = null!;
    public Salon? Salon { get; set; }
    public AppUser User { get; set; } = null!;
}
