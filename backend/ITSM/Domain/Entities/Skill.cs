using System.Collections.Generic;

namespace Domain.Entities
{
    public class Skill : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<TechnicianSkill> TechnicianSkills { get; set; } = new HashSet<TechnicianSkill>();
    }
}
