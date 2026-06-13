namespace Domain.Entities
{
    public class TechnicianSkill
    {
        public int TechnicianId { get; set; }
        public Technician? Technician { get; set; }

        public int SkillId { get; set; }
        public Skill? Skill { get; set; }

        public int Level { get; set; }
    }
}
