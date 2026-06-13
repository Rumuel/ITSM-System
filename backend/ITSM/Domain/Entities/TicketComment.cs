using System;

namespace Domain.Entities
{
    public class TicketComment : BaseEntity
    {
        public int TicketId { get; set; }
        public Ticket? Ticket { get; set; }

        public int AuthorId { get; set; }
        public ApplicationUser? Author { get; set; }

        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
