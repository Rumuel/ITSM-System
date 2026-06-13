using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public Technician? Technician { get; set; }
        public ICollection<Ticket> CreatedTickets { get; set; } = new HashSet<Ticket>();
        public ICollection<Asset> ResponsibleAssets { get; set; } = new HashSet<Asset>();
        public ICollection<TicketHistory> TicketHistories { get; set; } = new HashSet<TicketHistory>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new HashSet<AuditLog>();
    }
}
