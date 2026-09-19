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
    public class Client: IOrganizationEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }          // Привязка к мастеру (мультитенантность)
        public Guid? SalonId { get; set; }
        public string Name { get; set; }             // Имя клиента
        public string? Phone { get; set; }           // Телефон (для связи и напоминаний)
        public DateTime? BirthDate { get; set; }    // Для персональных предложений
        public string? Notes { get; set; }           // Заметки мастера (аллергии, предпочтения)
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационное свойство (не обязательно, но удобно)
        public Organization Organization { get; set; }
        public Salon? Salon { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = [];
        public ICollection<ClientContact> Contacts { get; set; } = [];
    }
}

