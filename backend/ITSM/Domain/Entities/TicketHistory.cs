using System;

namespace Domain.Entities
{
    public class TicketHistory : BaseEntity
    {
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        public int? ChangedByUserId { get; set; }
        public ApplicationUser? ChangedByUser { get; set; }

        public string? OldStatus { get; set; }
        public string? NewStatus { get; set; }
        public string? Comment { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
