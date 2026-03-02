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
    public class Service: IMultiTenant
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }          // У каждого мастера свой набор услуг
        public string Name { get; set; }             // Например, "Маникюр комбинированный"
        public int DurationMinutes { get; set; }     // Длительность в минутах (для календаря)
        public decimal Price { get; set; }           // Стоимость
        public string? Description { get; set; }     // Описание (необязательно)
        public string? Color { get; set; }           // Цвет в календаре (опционально)
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Tenant Tenant { get; set; }
        public ICollection<AppointmentService> AppointmentServices { get; set; }
    }
}
