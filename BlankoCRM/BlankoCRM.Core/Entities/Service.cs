using BlankoCRM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Core.Entities
{
    /// <summary>
    /// Данные об услугах мастера
    /// </summary>
    public class Service: IOrganizationEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }          // У каждого мастера свой набор услуг
        public Guid? SalonId { get; set; }
        public string Name { get; set; }             // Например, "Маникюр комбинированный"
        public int DurationMinutes { get; set; }     // Длительность в минутах (для календаря)
        public decimal Price { get; set; }           // Стоимость
        public string? Description { get; set; }     // Описание (необязательно)
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Organization Organization { get; set; }
        public Salon? Salon { get; set; }
        public ICollection<AppointmentService> AppointmentServices { get; set; }
    }
}

