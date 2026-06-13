using System.Collections.Generic;

namespace Domain.Entities
{
    public class Asset : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string? Location { get; set; }
        public string Status { get; set; } = string.Empty;

        public int AssetTypeId { get; set; }
        public AssetType? AssetType { get; set; }

        public int? ResponsibleUserId { get; set; }
        public ApplicationUser? ResponsibleUser { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
