using BlankoCRM.Core.Interfaces;

namespace BlankoCRM.Core.Entities;

public class ClientContact : IOrganizationEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid ClientId { get; set; }
    public ContactType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public Client Client { get; set; } = null!;
}
