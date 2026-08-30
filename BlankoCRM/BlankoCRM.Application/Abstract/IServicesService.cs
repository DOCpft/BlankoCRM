using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.Abstract;

/// <summary>
/// Сервис для работы с услугами.
/// </summary>
public interface IServicesService
{
    /// <summary>
    /// Получить все услуги текущего мастера.
    /// </summary>
    Task<List<Service>> GetAllAsync();

    /// <summary>
    /// Получает услугу по ID
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="KeyNotFoundException"> Если услуга с указаным ID не найдена</exception>
    /// <returns></returns>
    Task<Service?> GetByIdAsync(Guid id);

    /// <summary>
    /// Создать новую услугу.
    /// </summary>
    Task<Service> CreateAsync(string name, int durationMinutes, decimal price, string? description);

    /// <summary>
    /// Обновляет услугу по ID
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="durationMinutes"></param>
    /// <param name="price"></param>
    /// <param name="description"></param>
    /// <exception cref="KeyNotFoundException"> Если услуга с указаным ID не найдена</exception>
    /// <returns></returns>
    Task UpdateAsync(Guid id, string? name, int? durationMinutes, decimal? price, string? description);

    /// <summary>
    /// Удаляет услугу по ее ID (только если нет связанных записей)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"> Если услуга с указаным ID не найдена</exception>
    /// <exception cref="InvalidOperationException"> Если удаляем услугу которая используется в записях</exception>
    Task DeleteAsync(Guid id);
}
