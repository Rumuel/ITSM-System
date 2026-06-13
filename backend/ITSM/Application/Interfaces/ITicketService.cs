using Application.DTOs;

namespace Application.Interfaces
{
    public interface ITicketService
    {
        Task<TicketDto?> GetByIdAsync(int id);
        Task<IEnumerable<TicketDto>> GetAllAsync();
        Task<IEnumerable<TicketDto>> GetByRequesterAsync(int requesterId);
        Task<IEnumerable<TicketDto>> GetAssignedToAsync(int assigneeId);
        Task<TicketDto> CreateAsync(CreateTicketRequest request, int requesterId);
        Task<TicketDto?> UpdateAsync(int id, UpdateTicketRequest request, int userId);
        Task<bool> DeleteAsync(int id, int userId);
    }
}
