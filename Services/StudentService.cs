using BookingSystemAPI.Data;
using BookingSystemAPI.DTOs;
using BookingSystemAPI.Exceptions;
using BookingSystemAPI.Helpers;
using BookingSystemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAPI.Services
{
    public interface IStudentService
    {
        Task<StudentResponseDto> CreateAsync(int parentId, CreateStudentDto dto);
        Task<List<StudentResponseDto>> GetMineAsync(int parentId);
        Task<StudentResponseDto> GetByIdAsync(int studentNo, int userId, string role);
        Task<StudentResponseDto> UpdateAsync(int studentNo, int parentId, UpdateStudentDto dto);
        Task DeleteAsync(int studentNo, int parentId);
        Task<List<StudentResponseDto>> GetAllAsync(string? search);
    }

    public class StudentService : IStudentService
    {
        private readonly ApplicationDbContext _context;

        public StudentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<StudentResponseDto> CreateAsync(int parentId, CreateStudentDto dto)
        {
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.ParentId == parentId);
            if (parent == null)
            {
                throw new NotFoundException("Parent not found.");
            }

            var firstName = dto.FirstName.Trim();
            var lastName = dto.LastName.Trim();

            bool duplicate = await _context.Students.AnyAsync(s =>
                s.ParentId == parentId && s.FirstName == firstName && s.LastName == lastName);
            if (duplicate)
            {
                throw new ConflictException("You have already added a student with this name.");
            }

            var student = new Student
            {
                FirstName = firstName,
                LastName = lastName,
                Grade = dto.Grade.Trim(),
                HomeLanguage = dto.HomeLanguage.Trim(),
                ParentId = parentId,
                Parent = parent
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return student.ToDto();
        }

        public async Task<List<StudentResponseDto>> GetMineAsync(int parentId)
        {
            var students = await _context.Students
                .AsNoTracking()
                .Include(s => s.Parent)
                .Where(s => s.ParentId == parentId)
                .OrderBy(s => s.FirstName)
                .ToListAsync();

            return students.Select(s => s.ToDto()).ToList();
        }

        public async Task<StudentResponseDto> GetByIdAsync(int studentNo, int userId, string role)
        {
            var student = await _context.Students
                .AsNoTracking()
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.StudentNo == studentNo);

            // Parents can only see their own students. Anything else looks like "not found".
            if (student == null || (role == RoleNames.Parent && student.ParentId != userId))
            {
                throw new NotFoundException("Student not found.");
            }

            return student.ToDto();
        }

        public async Task<StudentResponseDto> UpdateAsync(int studentNo, int parentId, UpdateStudentDto dto)
        {
            var student = await _context.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.StudentNo == studentNo && s.ParentId == parentId);

            if (student == null)
            {
                throw new NotFoundException("Student not found.");
            }

            var firstName = dto.FirstName.Trim();
            var lastName = dto.LastName.Trim();

            bool duplicate = await _context.Students.AnyAsync(s =>
                s.ParentId == parentId && s.StudentNo != studentNo &&
                s.FirstName == firstName && s.LastName == lastName);
            if (duplicate)
            {
                throw new ConflictException("You have already added a student with this name.");
            }

            student.FirstName = firstName;
            student.LastName = lastName;
            student.Grade = dto.Grade.Trim();
            student.HomeLanguage = dto.HomeLanguage.Trim();

            await _context.SaveChangesAsync();
            return student.ToDto();
        }

        // A student with any booking history (even cancelled) can never be deleted.
        public async Task DeleteAsync(int studentNo, int parentId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNo == studentNo && s.ParentId == parentId);

            if (student == null)
            {
                throw new NotFoundException("Student not found.");
            }

            bool hasBookings = await _context.Bookings.AnyAsync(b => b.StudentNo == studentNo);
            if (hasBookings)
            {
                throw new ConflictException(
                    "This student has booking history and cannot be removed. Their records must be kept.");
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
        }

        public async Task<List<StudentResponseDto>> GetAllAsync(string? search)
        {
            var query = _context.Students
                .AsNoTracking()
                .Include(s => s.Parent)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(s => s.FirstName.Contains(term) || s.LastName.Contains(term));
            }

            var students = await query
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            return students.Select(s => s.ToDto()).ToList();
        }
    }
}