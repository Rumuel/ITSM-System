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

        public TicketService(ItsmDbContext context)
        {
            _context = context;
        }

        public async Task<TicketDto?> GetByIdAsync(int id)
        {
            var ticket = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedTechnician)
                    .ThenInclude(t => t!.User)
                .Include(t => t.Category)
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
                .Where(t => t.AssignedTechnicianId == assigneeId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return tickets.Select(MapToDto);
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
                CreatedByUserId = requesterId
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var createdTicket = await GetByIdAsync(ticket.Id);
            return createdTicket!;
        }

        public async Task<TicketDto?> UpdateAsync(int id, UpdateTicketRequest request, int userId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
                return null;

            // Only requester or assignee can update
            var isAssignedTechnician = await _context.Technicians
                .AnyAsync(t => t.Id == ticket.AssignedTechnicianId && t.UserId == userId);

            if (ticket.CreatedByUserId != userId && !isAssignedTechnician)
                return null;

            if (!string.IsNullOrEmpty(request.Title))
                ticket.Title = request.Title;

            if (request.Description != null)
                ticket.Description = request.Description;

            if (request.Priority.HasValue)
                ticket.PriorityId = request.Priority.Value;

            if (request.Status.HasValue)
                ticket.StatusId = request.Status.Value;

            if (request.AssigneeId.HasValue || request.AssigneeId == null)
                ticket.AssignedTechnicianId = request.AssigneeId;

            if (request.CategoryId.HasValue || request.CategoryId == null)
                ticket.CategoryId = request.CategoryId;

            ticket.UpdatedAt = DateTime.UtcNow;

            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id, int userId)
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null)
                return false;

            // Only requester can delete
            if (ticket.CreatedByUserId != userId)
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
                Status = ticket.StatusId,
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
