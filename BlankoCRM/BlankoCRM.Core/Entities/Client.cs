using BlankoCRM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Core.Entities
{
    /// <summary>
    /// Данные о клиентах мастера
    /// </summary>
    public class Client: IMultiTenant
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }          // Привязка к мастеру (мультитенантность)
        public string Name { get; set; }             // Имя клиента
        public string Phone { get; set; }            // Телефон (для связи и напоминаний)
        public string? Telegram { get; set; }           // Опционально
        public DateTime? BirthDate { get; set; }    // Для персональных предложений
        public string? Notes { get; set; }           // Заметки мастера (аллергии, предпочтения)
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационное свойство (не обязательно, но удобно)
        public Tenant Tenant { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
