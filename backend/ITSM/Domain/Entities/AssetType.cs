using System.Collections.Generic;

namespace Domain.Entities
{
    public class AssetType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<Asset> Assets { get; set; } = new HashSet<Asset>();
    }
}
