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
        public Task<List<Schedule>> AddSlotsAsync(List<CreateSlotRequest> addedSlots);
        public Task<Schedule> AddSlotAsync(DateTime slotTime, Guid? appointment);

        /// <summary>
        /// Методы для удаления слотов
        /// </summary>
        /// <param name="selectedSlot"></param>
        /// <exception cref="NotFoundException"> Если слот с указаным ID не найден</exception>
        /// <returns></returns>
        public Task<bool> RemoveSlotAsync(Guid selectedSlot);
        public Task<int> RemoveSlotsAsync(List<Guid> selectedSlots);

        /// <summary>
        /// Методы для получения слотов
        /// </summary>
        /// <param name="SlotId"></param>
        /// <exception cref="NotFoundException"> Если слот с указаным ID не найден</exception>
        /// <returns></returns>
        public Task<Schedule> GetSlotByIdAsync(Guid SlotId);
        public Task<List<Schedule>> GetScheduleByPeriodAsync(DateTime dateFrom, DateTime dateTo);

        /// <summary>
        /// Метод обновления данных слота
        /// </summary>
        /// <param name="id"></param>
        /// <param name="slotTime"></param>
        /// <param name="appointmentId"></param>
        /// <exception cref="NotFoundException"> Если слот с указаным ID не найден</exception>
        /// <returns></returns>
        public Task UpdateSlotAsync(Guid id, DateTime? slotTime,  Guid? appointmentId);

    }
}

