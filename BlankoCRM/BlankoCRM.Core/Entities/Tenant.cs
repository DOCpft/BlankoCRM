using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Core.Entities
{
    /// <summary>
    /// Учетные записи пользователей системы.
    /// Идентификация мастера, аутентификация, мультитенантность
    /// </summary>
    public class Tenant
    {
        public Guid Id { get; set; }               // Первичный ключ
        public string Name { get; set; }            // Имя мастера или название салона
        public string Email { get; set; }           // Логин (уникальный)
        public string PasswordHash { get; set; }    // Хеш пароля
        public string? TelegramChatId { get; set; } // ID чата в Telegram для напоминаний
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационные коллекции (для удобства)
        public ICollection<Client> Clients { get; set; }
        public ICollection<Service> Services { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}
