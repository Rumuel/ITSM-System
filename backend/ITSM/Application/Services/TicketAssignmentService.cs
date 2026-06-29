using Application.Events;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public sealed class TicketAssignmentService : ITicketAssignmentService
    {
        private const int AvailabilityScore = 3;
        private readonly IApplicationDbContext _context;
        private readonly IOperationalDataCache _cache;
        private readonly IOperationalDataLoader _loader;
        private readonly IApplicationEventDispatcher _eventDispatcher;

        public TicketAssignmentService(
            IApplicationDbContext context,
            IOperationalDataCache cache,
            IOperationalDataLoader loader,
            IApplicationEventDispatcher eventDispatcher)
        {
            _context = context;
            _cache = cache;
            _loader = loader;
            _eventDispatcher = eventDispatcher;
        }

        public async Task<Technician?> AssignBestTechnicianAsync(Ticket ticket)
        {
            var ticketData = await _context.Tickets
                .AsNoTracking()
                .FirstAsync(item => item.Id == ticket.Id);

            await _loader.RefreshAsync();
            var snapshot = _cache.Current;

            var requiredSkill = ticketData.CategoryId.HasValue
                ? snapshot.Categories.GetValueOrDefault(ticketData.CategoryId.Value)
                : null;
            var priorityWeight = snapshot.PriorityWeights.GetValueOrDefault(ticketData.PriorityId);
            var now = DateTime.Now;

            var candidates =
                new PriorityQueue<TechnicianOperationalData, (int NegativeScore, int Workload, int TechnicianId)>();

            foreach (var technician in snapshot.Technicians.Values.Where(item =>
                         item.IsAvailable && item.ActiveTicketCount < item.MaxActiveTickets))
            {
                var score = CalculateScore(technician, requiredSkill, priorityWeight, now);
                candidates.Enqueue(
                    technician,
                    (-score, technician.ActiveTicketCount, technician.TechnicianId));
            }

            if (!candidates.TryDequeue(out var bestTechnician, out var queuePriority))
            {
                return null;
            }

            ticket.AssignedTechnicianId = bestTechnician.TechnicianId;
            ticket.UpdatedAt = DateTime.UtcNow;

            _context.TicketHistories.Add(new TicketHistory
            {
                TicketId = ticket.Id,
                OldStatus = snapshot.Statuses.GetValueOrDefault(ticket.StatusId, "Novo"),
                NewStatus = snapshot.Statuses.GetValueOrDefault(ticket.StatusId, "Novo"),
                Comment = $"Ticket atribuido automaticamente ao tecnico {bestTechnician.DisplayName} " +
                          $"com pontuacao {-queuePriority.NegativeScore}.",
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await _eventDispatcher.PublishAsync(
                new TicketAssignmentChangedEvent(ticket.Id, bestTechnician.TechnicianId));

            return await _context.Technicians.FindAsync(bestTechnician.TechnicianId);
        }

        private static int CalculateScore(
            TechnicianOperationalData technician,
            string? requiredSkill,
            int priorityWeight,
            DateTime now)
        {
            var skillScore = string.IsNullOrWhiteSpace(requiredSkill)
                ? (technician.Skills.Count > 0 ? 1 : 0)
                : technician.Skills.GetValueOrDefault(requiredSkill);
            var availabilityScore = IsAvailableNow(technician, now) ? AvailabilityScore : 0;

            return skillScore - technician.ActiveTicketCount + availabilityScore + priorityWeight;
        }

        private static bool IsAvailableNow(TechnicianOperationalData technician, DateTime now)
        {
            if (technician.Availabilities.Count == 0)
            {
                return technician.IsAvailable;
            }

            return technician.Availabilities.Any(availability =>
                availability.WeekDay == now.DayOfWeek
                && availability.StartTime <= now.TimeOfDay
                && availability.EndTime >= now.TimeOfDay);
        }
    }
}
