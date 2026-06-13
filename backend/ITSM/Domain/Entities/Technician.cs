using System.Collections.Generic;

namespace Domain.Entities
{
    public class Technician : BaseEntity
    {
        public int UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int MaxActiveTickets { get; set; } = 5;
        public bool IsAvailable { get; set; } = true;

        public ICollection<TechnicianSkill> TechnicianSkills { get; set; } = new HashSet<TechnicianSkill>();
        public ICollection<TechnicianAvailability> Availabilities { get; set; } = new HashSet<TechnicianAvailability>();
        public ICollection<Ticket> AssignedTickets { get; set; } = new HashSet<Ticket>();
    }
}
