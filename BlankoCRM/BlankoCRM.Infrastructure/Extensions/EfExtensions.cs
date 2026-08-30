using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Infrastructure.Extensions
{
    /// <summary>
    /// Содержит методы расширения для EF Core.
    /// </summary>
    public static class EfExtensions
    {
        /// <summary>
        /// Асинхронно находит сущность по первичному ключу.
        /// Если сущность не найдена, выбрасывает KeyNotFoundException.
        /// </summary>
        /// <typeparam name="T">Тип сущности</typeparam>
        /// <param name="dbSet">DbSet</param>
        /// <param name="keyValues">Значения первичного ключа</param>
        /// <returns>Найденная сущность</returns>
        /// <exception cref="KeyNotFoundException">Если сущность не найдена</exception>
        public static async ValueTask<T> FindOrThrowAsync<T>(this DbSet<T> dbSet, Guid id) where T : class
        {
            var entity = await dbSet.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
            if (entity is null)
            {
                // Можно сформировать сообщение с типом и ключами, если нужно
                throw new KeyNotFoundException($"Entity of type {typeof(T).Name} with the specified key was not found.");
            }
            return entity;
        }
    }
}
