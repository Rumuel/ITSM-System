using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class TicketAssignmentService : ITicketAssignmentService
    {
        private const int AvailabilityScore = 3;
        private readonly ItsmDbContext _context;

        public TicketAssignmentService(ItsmDbContext context)
        {
            _context = context;
        }

        public async Task<Technician?> AssignBestTechnicianAsync(Ticket ticket)
        {
            var ticketData = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .FirstAsync(t => t.Id == ticket.Id);

            var technicians = await _context.Technicians
                .Include(t => t.TechnicianSkills)
                    .ThenInclude(ts => ts.Skill)
                .Include(t => t.Availabilities)
                .Where(t => t.IsAvailable)
                .ToListAsync();

            if (technicians.Count == 0)
            {
                return null;
            }

            var activeWorkloads = await _context.Tickets
                .Where(t => t.AssignedTechnicianId != null && t.StatusId != 3 && t.StatusId != 4)
                .GroupBy(t => t.AssignedTechnicianId!.Value)
                .Select(group => new
                {
                    TechnicianId = group.Key,
                    Count = group.Count()
                })
                .ToDictionaryAsync(item => item.TechnicianId, item => item.Count);

            var now = DateTime.Now;
            var priorityWeight = ticketData.Priority?.Weight ?? 0;
            var requiredSkill = ticketData.Category?.RequiredSkill;

            var bestTechnician = technicians
                .Select(technician =>
                {
                    activeWorkloads.TryGetValue(technician.Id, out var workload);
                    return new
                    {
                        Technician = technician,
                        Workload = workload,
                        Score = CalculateScore(technician, requiredSkill, workload, priorityWeight, now)
                    };
                })
                .Where(item => item.Workload < item.Technician.MaxActiveTickets)
                .OrderByDescending(item => item.Score)
                .ThenBy(item => item.Workload)
                .ThenBy(item => item.Technician.Id)
                .FirstOrDefault();

            if (bestTechnician == null)
            {
                return null;
            }

            ticket.AssignedTechnicianId = bestTechnician.Technician.Id;
            ticket.UpdatedAt = DateTime.UtcNow;

            _context.TicketHistories.Add(new TicketHistory
            {
                TicketId = ticket.Id,
                OldStatus = ticket.Status?.Name ?? "Novo",
                NewStatus = ticket.Status?.Name ?? "Novo",
                Comment = $"Ticket atribuido automaticamente ao tecnico {bestTechnician.Technician.User?.Name ?? bestTechnician.Technician.Id.ToString()} com pontuacao {bestTechnician.Score}.",
                ChangedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return bestTechnician.Technician;
        }

        private static int CalculateScore(
            Technician technician,
            string? requiredSkill,
            int workload,
            int priorityWeight,
            DateTime now)
        {
            var skillScore = GetSkillScore(technician, requiredSkill);
            var availabilityScore = IsAvailableNow(technician, now) ? AvailabilityScore : 0;

            return skillScore - workload + availabilityScore + priorityWeight;
        }

        private static int GetSkillScore(Technician technician, string? requiredSkill)
        {
            if (string.IsNullOrWhiteSpace(requiredSkill))
            {
                return technician.TechnicianSkills.Any() ? 1 : 0;
            }

            var matchingSkill = technician.TechnicianSkills
                .Where(ts => ts.Skill != null && ts.Skill.Name.Equals(requiredSkill, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(ts => ts.Level)
                .FirstOrDefault();

            return matchingSkill?.Level ?? 0;
        }

        private static bool IsAvailableNow(Technician technician, DateTime now)
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
