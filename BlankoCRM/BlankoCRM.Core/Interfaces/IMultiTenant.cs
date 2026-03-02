namespace BlankoCRM.Core.Interfaces;

public interface IMultiTenant
{
    Guid TenantId { get; set; }
}
