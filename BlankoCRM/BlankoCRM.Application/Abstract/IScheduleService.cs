using BlankoCRM.Core.Entities;
using BlankoCRM.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Application.Abstract
{
    /// <summary>
    /// Интерфейс для расписаний мастера
    /// </summary>
    public interface IScheduleService
    {
        /// <summary>
        /// Методы для добавления слотов в расписание
        /// Item1 - Время слота, Item2 - Guid записи (appointment)
        /// </summary>
        /// <returns></returns>
        Task<List<Schedule>> AddSlotsAsync(List<CreateFreeSlotRequest> addedSlots, CancellationToken cancellationToken = default);
        Task<Schedule> AddFreeSlotAsync(CreateFreeSlotRequest request, CancellationToken cancellationToken = default);
        Task<Schedule> AddOccupiedSlotAsync(CreateOccupiedSlotRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Методы для удаления слотов
        /// </summary>
        /// <param name="selectedSlot"></param>
        /// <exception cref="NotFoundException"> Если слот с указаным ID не найден</exception>
        /// <returns></returns>
        Task<bool> RemoveSlotAsync(Guid selectedSlot, CancellationToken cancellationToken = default);
        Task<int> RemoveSlotsAsync(List<Guid> selectedSlots, CancellationToken cancellationToken = default);

        /// <summary>
        /// Методы для получения слотов
        /// </summary>
        /// <param name="SlotId"></param>
        /// <exception cref="NotFoundException"> Если слот с указаным ID не найден</exception>
        /// <returns></returns>
        Task<Schedule> GetSlotByIdAsync(Guid SlotId, CancellationToken cancellationToken = default);
        Task<List<Schedule>> GetScheduleByPeriodAsync(DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken = default);

        /// <summary>
        /// Метод обновления данных слота
        /// </summary>
        /// <param name="id"></param>
        /// <param name="slotTime"></param>
        /// <param name="appointmentId"></param>
        /// <exception cref="NotFoundException"> Если слот с указаным ID не найден</exception>
        /// <returns></returns>
        Task UpdateSlotAsync(Guid id, UpdateSlotRequest request, CancellationToken cancellationToken = default);

    }
}

