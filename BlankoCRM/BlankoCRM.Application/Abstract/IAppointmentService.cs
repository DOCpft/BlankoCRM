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
        Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить записи на указанный день.
        /// </summary>
        Task<List<Appointment>> GetForDayAsync(DateTime date, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить все записи мастера
        /// </summary>
        /// <returns></returns>
        Task<List<Appointment>> GetAllAppointmentsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Создать новую запись.
        /// </summary>
        Task<Appointment> CreateAsync(Guid clientId, List<ServiceInput> serviceInputs, DateTime startTime, string? notes = null, Guid? salonId = null, CancellationToken cancellationToken = default);
        Task<(Appointment Appointment, Schedule Schedule)> CreateWithScheduleAsync(
            Guid clientId,
            List<ServiceInput> serviceInputs,
            DateTime startTime,
            string? notes = null,
            Guid? salonId = null,
            DateTime? endTime = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить существующую запись.
        /// </summary>
        Task UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Удалить запись.
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task UpdateStatusAsync(Guid id, AppointmentStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Отменить запись (без удаления).
        /// </summary>
        Task CancelAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

