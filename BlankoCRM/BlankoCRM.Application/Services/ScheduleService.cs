using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Infrastructure;
using BlankoCRM.Infrastructure.Data;
using BlankoCRM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Application.Services
{
    public class ScheduleService: IScheduleService
    {
        private readonly AppDbContext _dbContext;

        public ScheduleService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Schedule> AddSlotAsync(DateTime slotTime, Guid? appointmentId)
        {
            
            // Надо заполнять навигационное свойство, т.к. контрорллер возвращает ScheduleDto, и в нем есть AppointmentDto,
            // и если присвоить чисто внешний ключ, то мы не сможем замапить Schedule к ScheduleDto в контроллере.
            // Вернее сможем, но AppointmentDto там будет null, т.к. при заполнении внешнего ключа навигационное свойство не заполняется автоматом
            var appointment = await _dbContext.Appointments.FindAsync(appointmentId);
            
            var slot = new Schedule
            {
                SlotTime = slotTime,
                AppointmentId = appointment?.Id,
                Appointment = appointment,
                TenantId = TenantContext.CurrentTenantId!.Value,
                CreatedAt = DateTime.UtcNow,
            };
            
            // На всякий случай пересчитываем старт и конец самой записи, если слот на другое время поставили
            if (appointment != null)
            {
                appointment.StartTime = DateTime.UtcNow;
                appointment.EndTime = appointment.StartTime.AddMinutes((double)appointment.TotalDuration);
            }
                
            await _dbContext.AddAsync(slot);
            await _dbContext.SaveChangesAsync();
            return slot;
        }

        public async Task<List<Schedule>> AddSlotsAsync(List<Tuple<DateTime, Guid?>> addedSlots)
        {
            // 1. Собираем все Id назначений, которые не null
            var appointmentIds = addedSlots
                .Where(t => t.Item2 != null)
                .Select(t => t.Item2!)
                .Distinct()
                .ToList();

            // 2. Один запрос в БД для получения всех нужных Appointment
            var appointments = await _dbContext.Appointments
                .Where(a => appointmentIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id); // Словарь для быстрого доступа по Id

            List<Schedule> slots = new List<Schedule>();

            foreach (var (slotTime, appointmentId) in addedSlots)
            {
                var slot = new Schedule
                {
                    SlotTime = slotTime,
                    CreatedAt = DateTime.UtcNow,
                    TenantId = TenantContext.CurrentTenantId!.Value
                };
                
                if (appointmentId != null)
                {
                    if (appointments.TryGetValue(appointmentId.Value, out var appointment))
                    {
                        slot.AppointmentId = appointmentId.Value;
                        slot.Appointment = appointment;
                    }
                }
                // иначе Appointment остаётся null, а AppointmentId – null

                slots.Add(slot);
            }

            await _dbContext.AddRangeAsync(slots);
            await _dbContext.SaveChangesAsync();
            return slots;
        }

        public async Task<List<Schedule>> GetScheduleByPeriodAsync(DateTime dateFrom, DateTime dateTo)
        {
            var slotsByPeriod = await _dbContext.Schedules
                .Include(s => s.Appointment).ThenInclude(a => a.AppointmentServices)
                .Where(s => s.SlotTime >= dateFrom &&  s.SlotTime <= dateTo)
                .ToListAsync();
            return slotsByPeriod;

        }

        public async Task<Schedule> GetSlotByIdAsync(Guid SlotId)
        {
            var slot = await _dbContext.Schedules.FindOrThrowAsync(SlotId); // надо .Include() и .ThenInclude() Как по периоду
            return slot;
        }

        public async Task<bool> RemoveSlotAsync(Guid selectedSlot)
        {
            var slot = await _dbContext.Schedules.FindOrThrowAsync(selectedSlot);

            _dbContext.Schedules.Remove(slot);

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> RemoveSlotsAsync(List<Guid> selectedSlots)
        {
            // Не надо бросать исключение, если какой-то записи не хватает, просто в запрос на удаление эти guids не войдут
            return await _dbContext.Schedules
                    .Where(s => selectedSlots.Contains(s.Id))
                    .ExecuteDeleteAsync();
        }

        public async Task UpdateSlotAsync(Guid id, DateTime? slotTime, Guid? AppointmentId)
        {
            var slot = await _dbContext.Schedules.FindOrThrowAsync(id);

            if(slotTime.HasValue)
            {
                // ! т.к. в глобальном middleware есть обработчик NotFound
                slot!.SlotTime = slotTime.Value;
            }

            if(AppointmentId.HasValue)
            {
                slot!.AppointmentId = AppointmentId.Value;
            }

            slot!.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }
    }
}
