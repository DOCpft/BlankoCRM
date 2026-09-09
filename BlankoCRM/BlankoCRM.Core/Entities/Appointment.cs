using BlankoCRM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Core.Entities
{
    /// <summary>
    /// Записи клиентов. Связывает клиента, услугу и мастера в конкретное время
    /// </summary>
    public class Appointment: IOrganizationEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }          // Мастер, которому принадлежит запись
        public Guid? SalonId { get; set; }
        public Guid ClientId { get; set; }           // Какой клиент
        public DateTime StartTime { get; set; }      // Начало (важно для календаря)
        public DateTime EndTime { get; set; }        // Конец (можно вычислить из длительности услуги, но хранить удобнее)
        public AppointmentStatus Status { get; set; } // Статус: запланировано, выполнено, отменено, не пришла
        public string? Notes { get; set; }            // Доп. заметки к визиту
        public bool ReminderSent { get; set; } // Напоминания в телеграм
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства
        public Organization Organization { get; set; }
        public Salon? Salon { get; set; }
        public Client Client { get; set; }
        public ICollection<AppointmentService> AppointmentServices { get; set; }


                // Вычисляемые поля.
        // ВАЖНО: Эти поля вычисляются в памяти на основе загруженных AppointmentServices.
        // Если AppointmentServices не загружены (через .Include()), значения будут 0.
        // Для надёжности используйте эти поля ТОЛЬКО в DTO после загрузки связанных данных,
        // либо храните их как отдельные столбцы в БД (если производительность критична).
        public decimal TotalPrice => AppointmentServices?.Sum(aps => aps.Price) ?? 0;
        public decimal TotalDuration => AppointmentServices?.Sum(aps => aps.DurationMinutes) ?? 0;

    }

    public enum AppointmentStatus
    {
        Scheduled,   // Запланировано
        Completed,   // Выполнено
        Cancelled,   // Отменено мастером или клиентом
        NoShow       // Клиент не пришёл
    }
}

