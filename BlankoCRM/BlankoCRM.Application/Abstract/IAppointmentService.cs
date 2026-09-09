using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.Abstract
{
    /// <summary>
    /// Сервис для работы с записями на приём.
    /// </summary>
    public interface IAppointmentService
    {
        /// <summary>
        /// Получить запись по ID.
        /// </summary>
        Task<Appointment?> GetByIdAsync(Guid id);

        /// <summary>
        /// Получить записи на указанный день.
        /// </summary>
        Task<List<Appointment>> GetForDayAsync(DateTime date);

        /// <summary>
        /// Получить все записи мастера
        /// </summary>
        /// <returns></returns>
        Task<List<Appointment>> GetAllAppointmentsAsync();

        /// <summary>
        /// Создать новую запись.
        /// </summary>
        Task<Appointment> CreateAsync(Guid clientId, List<ServiceInput> serviceInputs, DateTime startTime, string? notes = null, Guid? salonId = null);

        /// <summary>
        /// Обновить существующую запись.
        /// </summary>
        Task UpdateAsync(Guid id, DateTime? startTime, AppointmentStatus? status, string? notes);

        /// <summary>
        /// Удалить запись.
        /// </summary>
        Task DeleteAsync(Guid id);

        /// <summary>
        /// Отменить запись (без удаления).
        /// </summary>
        Task CancelAsync(Guid id);
    }
}

