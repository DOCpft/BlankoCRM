namespace BlankoCRM.Core.Interfaces;

public interface IOrganizationEntity
{
    /// <summary>
    /// Идентификатор организации-владельца сущности.
    /// </summary>
    Guid OrganizationId { get; set; }
}

