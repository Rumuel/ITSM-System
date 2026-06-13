using System;

namespace Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public int? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string Action { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
