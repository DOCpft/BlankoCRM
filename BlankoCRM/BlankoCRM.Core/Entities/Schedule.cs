using BlankoCRM.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlankoCRM.Core.Entities
{
    /// <summary>
    /// Расписание мастера. Содержит записи о слотах мастера
    /// </summary>
    public class Schedule : IOrganizationEntity
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? SalonId { get; set; }
        public Guid? AppointmentId { get; set; }
        public DateTime SlotTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Organization Organization { get; set; }
        public Salon? Salon { get; set; }
        public Appointment? Appointment { get; set; }
    }
}

