namespace Application.Models
{
    public sealed record TechnicianOperationalData(
        int TechnicianId,
        string DisplayName,
        int MaxActiveTickets,
        bool IsAvailable,
        int ActiveTicketCount,
        IReadOnlyDictionary<string, int> Skills,
        IReadOnlyCollection<TechnicianAvailabilityData> Availabilities);

    public sealed record TechnicianAvailabilityData(
        DayOfWeek WeekDay,
        TimeSpan StartTime,
        TimeSpan EndTime);

    public sealed record OperationalDataSnapshot(
        IReadOnlyDictionary<int, TechnicianOperationalData> Technicians,
        IReadOnlyDictionary<int, string> Categories,
        IReadOnlyDictionary<int, string> Statuses,
        IReadOnlyDictionary<int, int> PriorityWeights,
        DateTime LoadedAtUtc)
    {
        public static OperationalDataSnapshot Empty { get; } = new(
            new Dictionary<int, TechnicianOperationalData>(),
            new Dictionary<int, string>(),
            new Dictionary<int, string>(),
            new Dictionary<int, int>(),
            DateTime.MinValue);
    }
}
