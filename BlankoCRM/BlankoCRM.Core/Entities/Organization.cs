using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Core.Entities
{
    /// <summary>
    /// Организация верхнего уровня в иерархии мультитенантности.
    /// </summary>
    public class Organization
    {
        public Guid Id { get; set; }               // Первичный ключ
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Навигационные коллекции (для удобства)
        public ICollection<Client> Clients { get; set; } = new List<Client>();
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Salon> Salons { get; set; } = new List<Salon>();
        public ICollection<MasterProfile> MasterProfiles { get; set; } = new List<MasterProfile>();
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    }
}

