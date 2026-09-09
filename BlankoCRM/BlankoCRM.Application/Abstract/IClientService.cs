using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.Abstract
{
    /// <summary>
    /// Сервис для работы с клиентами.
    /// </summary>
    public interface IClientService
    {
        /// <summary>
        /// Получить всех клиентов текущего мастера с возможностью поиска.
        /// </summary>
        Task<List<Client>> GetAllAsync(string? search);

        /// <summary>
        /// Получить клиента по ID.
        /// </summary>
        Task<Client?> GetByIdAsync(Guid id);

        /// <summary>
        /// Создать нового клиента.
        /// </summary>
        Task<Client> CreateAsync(string name, string phone, string? telegram, DateTime? birthDate, string? notes, Guid? salonId = null);

        /// <summary>
        /// Обновить существующего клиента.
        /// </summary>
        Task UpdateAsync(Guid id, string? name, string? phone, string? telegram, DateTime? birthDate, string? notes, Guid? salonId = null);

        /// <summary>
        /// Удалить клиента.
        /// </summary>
        Task DeleteAsync(Guid id);
    }
}

