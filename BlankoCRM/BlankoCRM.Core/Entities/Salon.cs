using BlankoCRM.Core.Interfaces;

namespace BlankoCRM.Core.Entities;

/// <summary>
/// Салон, принадлежащий организации.
/// </summary>
public class Salon : IOrganizationEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public ICollection<MasterProfile> MasterProfiles { get; set; } = new List<MasterProfile>();
}
