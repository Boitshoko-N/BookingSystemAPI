using BookingSystemAPI.Data;
using BookingSystemAPI.DTOs;
using BookingSystemAPI.Exceptions;
using BookingSystemAPI.Helpers;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAPI.Services
{
    public interface IParentService
    {
        Task<ParentDetailsDto> GetDetailsAsync(int parentId);
        Task<ParentResponseDto> UpdateProfileAsync(int parentId, UpdateParentDto dto);
        Task<List<ParentResponseDto>> GetAllAsync(string? search);
    }

    public class ParentService : IParentService
    {
        private readonly ApplicationDbContext _context;

        public ParentService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Profile + students + each student's bookings (including cancelled ones, as history).
        public async Task<ParentDetailsDto> GetDetailsAsync(int parentId)
        {
            var parent = await _context.Parents
                .Include(p => p.Students)
                    .ThenInclude(s => s.Bookings)
                        .ThenInclude(b => b.Slot)
                .FirstOrDefaultAsync(p => p.ParentId == parentId);

            if (parent == null)
            {
                throw new NotFoundException("Parent not found.");
            }

            return parent.ToDetailsDto();
        }

        public async Task<ParentResponseDto> UpdateProfileAsync(int parentId, UpdateParentDto dto)
        {
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.ParentId == parentId);
            if (parent == null)
            {
                throw new NotFoundException("Parent not found.");
            }

            parent.FirstName = dto.FirstName.Trim();
            parent.LastName = dto.LastName.Trim();
            parent.PhoneNumber = dto.PhoneNumber.Trim();

            await _context.SaveChangesAsync();
            return parent.ToDto();
        }

        public async Task<List<ParentResponseDto>> GetAllAsync(string? search)
        {
            var query = _context.Parents.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p => p.FirstName.Contains(term) ||
                                         p.LastName.Contains(term) ||
                                         p.Email.Contains(term));
            }

            var parents = await query
                .OrderBy(p => p.LastName)
                .ThenBy(p => p.FirstName)
                .ToListAsync();

            return parents.Select(p => p.ToDto()).ToList();
        }
    }
}