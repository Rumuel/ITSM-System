using Application.Interfaces;
using Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public sealed class OperationalDataLoader : IOperationalDataLoader
    {
        private readonly ItsmDbContext _context;
        private readonly IOperationalDataCache _cache;

        public OperationalDataLoader(ItsmDbContext context, IOperationalDataCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<OperationalDataSnapshot> LoadAsync(CancellationToken cancellationToken = default)
        {
            var workloads = await _context.Tickets
                .AsNoTracking()
                .Where(ticket => ticket.AssignedTechnicianId != null
                    && ticket.StatusId != 3
                    && ticket.StatusId != 4)
                .GroupBy(ticket => ticket.AssignedTechnicianId!.Value)
                .ToDictionaryAsync(group => group.Key, group => group.Count(), cancellationToken);

            var technicianEntities = await _context.Technicians
                .AsNoTracking()
                .Include(technician => technician.User)
                .Include(technician => technician.TechnicianSkills)
                    .ThenInclude(technicianSkill => technicianSkill.Skill)
                .Include(technician => technician.Availabilities)
                .ToListAsync(cancellationToken);

            var technicians = technicianEntities.ToDictionary(
                technician => technician.Id,
                technician =>
                {
                    workloads.TryGetValue(technician.Id, out var workload);
                    return new TechnicianOperationalData(
                        technician.Id,
                        technician.User?.Name ?? technician.User?.UserName ?? $"Tecnico {technician.Id}",
                        technician.MaxActiveTickets,
                        technician.IsAvailable,
                        workload,
                        technician.TechnicianSkills
                            .Where(item => item.Skill != null)
                            .GroupBy(item => item.Skill!.Name, StringComparer.OrdinalIgnoreCase)
                            .ToDictionary(
                                group => group.Key,
                                group => group.Max(item => item.Level),
                                StringComparer.OrdinalIgnoreCase),
                        technician.Availabilities
                            .Select(item => new TechnicianAvailabilityData(
                                item.WeekDay,
                                item.StartTime,
                                item.EndTime))
                            .ToArray());
                });

            var categories = await _context.TicketCategories
                .AsNoTracking()
                .ToDictionaryAsync(
                    category => category.Id,
                    category => category.RequiredSkill ?? string.Empty,
                    cancellationToken);

            var statuses = await _context.TicketStatuses
                .AsNoTracking()
                .ToDictionaryAsync(status => status.Id, status => status.Name, cancellationToken);

            var priorities = await _context.Priorities
                .AsNoTracking()
                .ToDictionaryAsync(priority => priority.Id, priority => priority.Weight, cancellationToken);

            return new OperationalDataSnapshot(
                technicians,
                categories,
                statuses,
                priorities,
                DateTime.UtcNow);
        }

        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            _cache.Replace(await LoadAsync(cancellationToken));
        }
    }
}
