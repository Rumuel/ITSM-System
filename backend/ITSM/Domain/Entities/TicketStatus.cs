using System.Collections.Generic;

namespace Domain.Entities
{
    public class TicketStatus : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
