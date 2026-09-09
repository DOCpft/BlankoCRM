using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Application.Services
{
    public class ScheduleService: IScheduleService
    {
        private readonly IScheduleRepository _scheduleRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IOrganizationContext _organizationContext;

        public ScheduleService(
            IScheduleRepository scheduleRepository,
            IAppointmentRepository appointmentRepository,
            IOrganizationContext organizationContext)
        {
            _scheduleRepository = scheduleRepository;
            _appointmentRepository = appointmentRepository;
            _organizationContext = organizationContext;
        }

        public async Task<Schedule> AddSlotAsync(DateTime slotTime, Guid? appointmentId)
        {
            
            // Надо заполнять навигационное свойство, т.к. контрорллер возвращает ScheduleDto, и в нем есть AppointmentDto,
            // и если присвоить чисто внешний ключ, то мы не сможем замапить Schedule к ScheduleDto в контроллере.
            // Вернее сможем, но AppointmentDto там будет null, т.к. при заполнении внешнего ключа навигационное свойство не заполняется автоматом
            var appointment = appointmentId.HasValue
                ? await _appointmentRepository.GetByIdWithDetailsAsync(appointmentId.Value)
                : null;
            
            var slot = new Schedule
            {
                SlotTime = slotTime,
                AppointmentId = appointment?.Id,
                Appointment = appointment,
                OrganizationId = _organizationContext.GetCurrentOrganizationId()!.Value,
                CreatedAt = DateTime.UtcNow,
            };
            
            // На всякий случай пересчитываем старт и конец самой записи, если слот на другое время поставили
            if (appointment != null)
            {
                var newEndTime = slotTime.AddMinutes((double)appointment.TotalDuration);
                if (await _appointmentRepository.HasOverlapAsync(
                    slotTime,
                    newEndTime,
                    appointment.OrganizationId,
                    appointment.SalonId,
                    appointment.Id))
                {
                    throw new ConflictException("Это время уже занято другой записью.");
                }

                appointment.StartTime = slotTime;
                appointment.EndTime = newEndTime;
            }
                
            return await _scheduleRepository.AddAsync(slot);
        }

        public async Task<List<Schedule>> AddSlotsAsync(List<CreateSlotRequest> addedSlots)
        {
            // 1. Собираем все Id назначений, которые не null
            var appointmentIds = addedSlots
                .Where(slot => slot.AppointmentId != null)
                .Select(slot => slot.AppointmentId!.Value)
                .Distinct()
                .ToList();

            // 2. Один запрос в БД для получения всех нужных Appointment
            var appointments = (await _appointmentRepository.FindAsync(appointment => appointmentIds.Contains(appointment.Id)))
                .ToDictionary(appointment => appointment.Id);

            List<Schedule> slots = new List<Schedule>();

            foreach (var request in addedSlots)
            {
                var slot = new Schedule
                {
                    SlotTime = request.slotTime,
                    CreatedAt = DateTime.UtcNow,
                    OrganizationId = _organizationContext.GetCurrentOrganizationId()!.Value
                };
                
                if (request.AppointmentId != null)
                {
                    if (appointments.TryGetValue(request.AppointmentId.Value, out var appointment))
                    {
                        slot.AppointmentId = request.AppointmentId.Value;
                    }
                }
                // иначе Appointment остаётся null, а AppointmentId – null

                slots.Add(slot);
            }

            await _scheduleRepository.AddRangeAsync(slots);
            return slots;
        }

        public async Task<List<Schedule>> GetScheduleByPeriodAsync(DateTime dateFrom, DateTime dateTo)
        {
            return (await _scheduleRepository.GetByPeriodAsync(dateFrom, dateTo)).ToList();

        }

        public async Task<Schedule> GetSlotByIdAsync(Guid SlotId)
        {
            return await _scheduleRepository.GetByIdWithAppointmentAsync(SlotId)
                ?? throw new NotFoundException($"Schedule with ID {SlotId} was not found.");
        }

        public async Task<bool> RemoveSlotAsync(Guid selectedSlot)
        {
            var slot = await _scheduleRepository.GetByIdAsync(selectedSlot)
                ?? throw new NotFoundException($"Schedule with ID {selectedSlot} was not found.");

            await _scheduleRepository.DeleteAsync(slot);
            return true;
        }

        public async Task<int> RemoveSlotsAsync(List<Guid> selectedSlots)
        {
            var slots = await _scheduleRepository.FindAsync(slot => selectedSlots.Contains(slot.Id));
            foreach (var slot in slots)
            {
                await _scheduleRepository.DeleteAsync(slot);
            }

            return slots.Count;
        }

        public async Task UpdateSlotAsync(Guid id, DateTime? slotTime, Guid? AppointmentId)
        {
            var slot = await _scheduleRepository.GetByIdWithAppointmentAsync(id)
                ?? throw new NotFoundException($"Schedule with ID {id} was not found.");

            if(slotTime.HasValue)
            {
                // ! т.к. в глобальном middleware есть обработчик NotFound
                slot!.SlotTime = slotTime.Value;
                if (slot.Appointment is not null)
                {
                    var newEndTime = slotTime.Value.AddMinutes((double)slot.Appointment.TotalDuration);
                    if (await _appointmentRepository.HasOverlapAsync(
                        slotTime.Value,
                        newEndTime,
                        slot.Appointment.OrganizationId,
                        slot.Appointment.SalonId,
                        slot.Appointment.Id))
                    {
                        throw new ConflictException("Это время уже занято другой записью.");
                    }

                    slot.Appointment.StartTime = slotTime.Value;
                    slot.Appointment.EndTime = newEndTime;
                }
            }

            if(AppointmentId.HasValue)
            {
                var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(AppointmentId.Value)
                    ?? throw new NotFoundException($"Appointment with ID {AppointmentId.Value} was not found.");
                slot!.AppointmentId = appointment.Id;
                slot.Appointment = appointment;
                if (slotTime.HasValue)
                {
                    var newEndTime = slotTime.Value.AddMinutes((double)appointment.TotalDuration);
                    if (await _appointmentRepository.HasOverlapAsync(
                        slotTime.Value,
                        newEndTime,
                        appointment.OrganizationId,
                        appointment.SalonId,
                        appointment.Id))
                    {
                        throw new ConflictException("Это время уже занято другой записью.");
                    }

                    appointment.StartTime = slotTime.Value;
                    appointment.EndTime = newEndTime;
                }
            }

            slot!.UpdatedAt = DateTime.UtcNow;

            await _scheduleRepository.UpdateAsync(slot);
        }
    }
}

