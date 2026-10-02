namespace BlankoCRM.Core.Interfaces;

/// <summary>
/// Контекст организации текущего асинхронного запроса.
/// </summary>
public interface IOrganizationContext
{
    /// <summary>
    /// Возвращает идентификатор текущей организации или null для анонимного запроса.
    /// </summary>
    Guid? GetCurrentOrganizationId();

    /// <summary>
    /// Устанавливает организацию текущего запроса.
    /// </summary>
    void SetCurrentOrganizationId(Guid organizationId);

    /// <summary>
    /// Очищает организацию текущего запроса.
    /// </summary>
    void Clear();
}
