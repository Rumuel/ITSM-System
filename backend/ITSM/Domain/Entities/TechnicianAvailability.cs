using System;

namespace Domain.Entities
{
    public class TechnicianAvailability : BaseEntity
    {
        public int TechnicianId { get; set; }
        public Technician? Technician { get; set; }

        public DayOfWeek WeekDay { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
