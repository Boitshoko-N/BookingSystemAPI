using BookingSystemAPI.Data;
using BookingSystemAPI.DTOs;
using BookingSystemAPI.Exceptions;
using BookingSystemAPI.Helpers;
using BookingSystemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAPI.Services
{
    public interface IStaffService
    {
        Task<List<StaffResponseDto>> GetAllAsync();
        Task<StaffResponseDto> GetByIdAsync(int staffNo);
        Task<StaffResponseDto> CreateAsync(CreateStaffDto dto);
        Task<StaffResponseDto> UpdateAsync(int staffNo, UpdateStaffDto dto);
        Task ChangePasswordAsync(int staffNo, ChangePasswordDto dto);
    }

    public class StaffService : IStaffService
    {
        private readonly ApplicationDbContext _context;

        public StaffService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<StaffResponseDto>> GetAllAsync()
        {
            var staff = await _context.Staff
                .AsNoTracking()
                .Include(s => s.Role)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            return staff.Select(s => s.ToDto()).ToList();
        }

        public async Task<StaffResponseDto> GetByIdAsync(int staffNo)
        {
            var staff = await _context.Staff
                .AsNoTracking()
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.StaffNo == staffNo);

            if (staff == null)
            {
                throw new NotFoundException("Staff member not found.");
            }

            return staff.ToDto();
        }

        public async Task<StaffResponseDto> CreateAsync(CreateStaffDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Staff.AnyAsync(s => s.Email == email))
            {
                throw new ConflictException("A staff member with this email address already exists.");
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.RoleName);
            if (role == null)
            {
                throw new BusinessRuleException("The selected role does not exist.");
            }

            var staff = new Staff
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = email,
                PhoneNumber = dto.PhoneNumber.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                RoleId = role.RoleId,
                Role = role,
                IsActive = true
            };

            _context.Staff.Add(staff);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                throw new ConflictException("A staff member with this email address already exists.");
            }

            return staff.ToDto();
        }

        public async Task<StaffResponseDto> UpdateAsync(int staffNo, UpdateStaffDto dto)
        {
            var staff = await _context.Staff
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.StaffNo == staffNo);

            if (staff == null)
            {
                throw new NotFoundException("Staff member not found.");
            }

            var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.RoleName);
            if (newRole == null)
            {
                throw new BusinessRuleException("The selected role does not exist.");
            }

            // Never allow the system to be left without an active Admin.
            bool isActiveAdminNow = staff.IsActive && staff.Role!.RoleName == RoleNames.Admin;
            bool willBeActiveAdmin = dto.IsActive && newRole.RoleName == RoleNames.Admin;

            if (isActiveAdminNow && !willBeActiveAdmin)
            {
                bool anotherActiveAdminExists = await _context.Staff.AnyAsync(s =>
                    s.StaffNo != staffNo && s.IsActive && s.Role!.RoleName == RoleNames.Admin);

                if (!anotherActiveAdminExists)
                {
                    throw new ConflictException(
                        "This is the last active Admin. Create or activate another Admin before deactivating or demoting this account.");
                }
            }

            staff.FirstName = dto.FirstName.Trim();
            staff.LastName = dto.LastName.Trim();
            staff.PhoneNumber = dto.PhoneNumber.Trim();
            staff.IsActive = dto.IsActive;
            staff.Role = newRole;
            staff.RoleId = newRole.RoleId;

            await _context.SaveChangesAsync();
            return staff.ToDto();
        }

        public async Task ChangePasswordAsync(int staffNo, ChangePasswordDto dto)
        {
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.StaffNo == staffNo);
            if (staff == null)
            {
                throw new NotFoundException("Staff member not found.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, staff.PasswordHash))
            {
                throw new BusinessRuleException("The current password is incorrect.");
            }

            if (dto.CurrentPassword == dto.NewPassword)
            {
                throw new BusinessRuleException("The new password must be different from the current password.");
            }

            staff.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
        }
    }
}