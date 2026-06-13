using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class Ticket : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        public int? AssignedTechnicianId { get; set; }
        public Technician? AssignedTechnician { get; set; }

        public int? CategoryId { get; set; }
        public TicketCategory? Category { get; set; }

        public int StatusId { get; set; }
        public TicketStatus? Status { get; set; }

        public int PriorityId { get; set; }
        public Priority? Priority { get; set; }

        public int? AssetId { get; set; }
        public Asset? Asset { get; set; }

        public ICollection<TicketHistory> Histories { get; set; } = new HashSet<TicketHistory>();
    }
}
