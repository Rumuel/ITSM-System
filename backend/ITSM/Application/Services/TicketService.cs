using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class TicketService : ITicketService
    {
        private readonly ItsmDbContext _context;
        private readonly ITicketAssignmentService _assignmentService;

        public TicketService(ItsmDbContext context, ITicketAssignmentService assignmentService)
        {
            _context = context;
            _assignmentService = assignmentService;
        }

        public async Task<TicketDto?> GetByIdAsync(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedTechnician)
                    .ThenInclude(t => t!.User)
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.Id == id);

            return ticket == null ? null : MapToDto(ticket);
        }

        public async Task<IEnumerable<TicketDto>> GetAllAsync()
        {
            var tickets = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedTechnician)
                    .ThenInclude(t => t!.User)
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tickets.Select(MapToDto);
        }

        public async Task<IEnumerable<TicketDto>> GetByRequesterAsync(int requesterId)
        {
            var tickets = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedTechnician)
                    .ThenInclude(t => t!.User)
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Where(t => t.CreatedByUserId == requesterId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tickets.Select(MapToDto);
        }

        public async Task<IEnumerable<TicketDto>> GetAssignedToAsync(int assigneeId)
        {
            var tickets = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedTechnician)
                    .ThenInclude(t => t!.User)
                .Include(t => t.Category)
                .Include(t => t.Priority)
                .Include(t => t.Status)
                .Where(t => t.AssignedTechnicianId == assigneeId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tickets.Select(MapToDto);
        }

        public async Task<IEnumerable<TicketLookupDto>> GetPrioritiesAsync()
        {
            return await _context.Priorities
                .OrderBy(p => p.Weight)
                .Select(p => new TicketLookupDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Weight = p.Weight
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketLookupDto>> GetStatusesAsync()
        {
            return await _context.TicketStatuses
                .OrderBy(s => s.Id)
                .Select(s => new TicketLookupDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();
        }

        public async Task<TicketDto> CreateAsync(CreateTicketRequest request, int requesterId)
        {
            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                PriorityId = request.Priority,
                StatusId = 1,
                CategoryId = request.CategoryId,
                CreatedByUserId = requesterId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            await _assignmentService.AssignBestTechnicianAsync(ticket);

            var createdTicket = await GetByIdAsync(ticket.Id);
            return createdTicket!;
        }

        public async Task<TicketDto?> UpdateAsync(int id, UpdateTicketRequest request, int userId, bool canManageAnyTicket)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
                return null;

            var isAssignedTechnician = await _context.Technicians
                .AnyAsync(t => t.Id == ticket.AssignedTechnicianId && t.UserId == userId);

            if (!canManageAnyTicket && ticket.CreatedByUserId != userId && !isAssignedTechnician)
                return null;

            if (!string.IsNullOrEmpty(request.Title))
                ticket.Title = request.Title;

            if (request.Description != null)
                ticket.Description = request.Description;

            if (request.Priority.HasValue)
                ticket.PriorityId = request.Priority.Value;

            if (request.Status.HasValue)
                ticket.StatusId = request.Status.Value;

            if (request.AssigneeId.HasValue)
                ticket.AssignedTechnicianId = request.AssigneeId;

            if (request.CategoryId.HasValue)
                ticket.CategoryId = request.CategoryId;

            ticket.UpdatedAt = DateTime.UtcNow;

            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id, int userId, bool canManageAnyTicket)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
                return false;

            if (!canManageAnyTicket && ticket.CreatedByUserId != userId)
                return false;

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return true;
        }

        private static TicketDto MapToDto(Ticket ticket)
        {
            return new TicketDto
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Priority = ticket.PriorityId,
                PriorityName = ticket.Priority?.Name,
                Status = ticket.StatusId,
                StatusName = ticket.Status?.Name,
                CreatedAt = ticket.CreatedAt,
                UpdatedAt = ticket.UpdatedAt,
                RequesterId = ticket.CreatedByUserId,
                RequesterName = string.IsNullOrWhiteSpace(ticket.CreatedByUser?.Name) ? ticket.CreatedByUser?.UserName : ticket.CreatedByUser.Name,
                AssigneeId = ticket.AssignedTechnicianId,
                AssigneeName = string.IsNullOrWhiteSpace(ticket.AssignedTechnician?.User?.Name)
                    ? ticket.AssignedTechnician?.User?.UserName
                    : ticket.AssignedTechnician.User.Name,
                CategoryId = ticket.CategoryId,
                CategoryName = ticket.Category?.Name
            };
        }
    }
}
