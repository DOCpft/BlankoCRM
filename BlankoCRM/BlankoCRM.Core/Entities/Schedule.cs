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
    public class Schedule : IMultiTenant
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid? AppointmentId { get; set; }
        public DateTime SlotTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Tenant Tenant { get; set; }
        public Appointment? Appointment { get; set; }
    }
}
