using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class TicketCategoryService : ITicketCategoryService
    {
        private readonly ItsmDbContext _context;

        public TicketCategoryService(ItsmDbContext context)
        {
            _context = context;
        }

        public async Task<TicketCategoryDto?> GetByIdAsync(int id)
        {
            var category = await _context.TicketCategories.FirstOrDefaultAsync(c => c.Id == id);
            return category == null ? null : MapToDto(category);
        }

        public async Task<IEnumerable<TicketCategoryDto>> GetAllAsync()
        {
            var categories = await _context.TicketCategories
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories.Select(MapToDto);
        }

        public async Task<TicketCategoryDto> CreateAsync(CreateTicketCategoryRequest request)
        {
            var category = new TicketCategory
            {
                Name = request.Name,
                Description = request.Description
            };

            _context.TicketCategories.Add(category);
            await _context.SaveChangesAsync();

            return MapToDto(category);
        }

        public async Task<TicketCategoryDto?> UpdateAsync(int id, CreateTicketCategoryRequest request)
        {
            var category = await _context.TicketCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return null;

            category.Name = request.Name;
            category.Description = request.Description;

            _context.TicketCategories.Update(category);
            await _context.SaveChangesAsync();

            return MapToDto(category);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.TicketCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return false;

            _context.TicketCategories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        private static TicketCategoryDto MapToDto(TicketCategory category)
        {
            return new TicketCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }
    }
}
