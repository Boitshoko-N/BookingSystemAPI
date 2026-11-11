using BookingSystemAPI.Data;
using BookingSystemAPI.DTOs;
using BookingSystemAPI.Exceptions;
using BookingSystemAPI.Helpers;
using BookingSystemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAPI.Services
{
    public interface IAuthService
    {
        Task<ParentResponseDto> RegisterParentAsync(RegisterParentDto dto);
        Task<LoginResponseDto> LoginParentAsync(LoginDto dto);
        Task<LoginResponseDto> LoginStaffAsync(LoginDto dto);
    }

    public class AuthService : IAuthService
    {
        private const string InvalidLoginMessage = "Invalid email or password.";

        // Used so that an unknown email takes as long to check as a real one.
        private static readonly string DummyHash = BCrypt.Net.BCrypt.HashPassword("dummy-password-for-timing");

        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<ParentResponseDto> RegisterParentAsync(RegisterParentDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Parents.AnyAsync(p => p.Email == email))
            {
                throw new ConflictException("An account with this email address already exists.");
            }

            var parent = new Parent
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = email,
                PhoneNumber = dto.PhoneNumber.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Parents.Add(parent);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                // Two people registering the same email at the same instant.
                throw new ConflictException("An account with this email address already exists.");
            }

            return parent.ToDto();
        }

        public async Task<LoginResponseDto> LoginParentAsync(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var parent = await _context.Parents.FirstOrDefaultAsync(p => p.Email == email);

            var passwordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, parent?.PasswordHash ?? DummyHash);

            if (parent == null || !passwordCorrect)
            {
                throw new UnauthorizedAccessException(InvalidLoginMessage);
            }

            return _tokenService.CreateToken(
                parent.ParentId, parent.Email, $"{parent.FirstName} {parent.LastName}", RoleNames.Parent);
        }

        public async Task<LoginResponseDto> LoginStaffAsync(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLowerInvariant();
            var staff = await _context.Staff
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.Email == email);

            var passwordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, staff?.PasswordHash ?? DummyHash);

            if (staff == null || !passwordCorrect || !staff.IsActive)
            {
                throw new UnauthorizedAccessException(InvalidLoginMessage);
            }

            return _tokenService.CreateToken(
                staff.StaffNo, staff.Email, $"{staff.FirstName} {staff.LastName}", staff.Role!.RoleName);
        }
    }
}