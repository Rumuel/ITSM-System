using Application.DTOs;

namespace Application.Interfaces
{
    public interface ITicketCategoryService
    {
        Task<TicketCategoryDto?> GetByIdAsync(int id);
        Task<IEnumerable<TicketCategoryDto>> GetAllAsync();
        Task<TicketCategoryDto> CreateAsync(CreateTicketCategoryRequest request);
        Task<TicketCategoryDto?> UpdateAsync(int id, CreateTicketCategoryRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
