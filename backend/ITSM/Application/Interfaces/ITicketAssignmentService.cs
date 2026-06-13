using Domain.Entities;

namespace Application.Interfaces
{
    public interface ITicketAssignmentService
    {
        Task<Technician?> AssignBestTechnicianAsync(Ticket ticket);
    }
}
