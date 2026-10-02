using System.Linq.Expressions;

namespace BlankoCRM.Core.Interfaces;

/// <summary>
/// Базовый контракт доступа к сущностям.
/// </summary>
/// <typeparam name="T">Тип сущности.</typeparam>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    /// <summary>
    /// Добавляет сущность в текущую Unit of Work без сохранения изменений в БД.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Регистрирует изменения сущности в текущей Unit of Work без сохранения в БД.
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Регистрирует удаление сущности в текущей Unit of Work без сохранения в БД.
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
}
