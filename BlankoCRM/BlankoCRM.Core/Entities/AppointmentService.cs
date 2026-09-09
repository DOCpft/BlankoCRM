using BlankoCRM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Core.Entities
{
    public class AppointmentService: IOrganizationEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? SalonId { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid ServiceId { get; set; }
        public decimal Price { get; set; }           // Цена на момент записи (копируется из услуги, но может быть изменена)
        public int DurationMinutes { get; set; }     // Длительность на момент записи (копируется из услуги)
        public string? ServiceName { get; set; }     // Денормализованное название (чтобы не терять историю, если услуга потом переименуется)
        public int Quantity { get; set; } = 1;      // Количество (например, 2 часа массажа = 2 единицы по 60 минут)

        // Навигационные свойства
        public Organization Organization { get; set; }
        public Salon? Salon { get; set; }
        public Appointment Appointment { get; set; }
        public Service Service { get; set; }
    }
}

