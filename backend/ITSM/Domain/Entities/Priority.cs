using System.Collections.Generic;

namespace Domain.Entities
{
    public class Priority : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Weight { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
