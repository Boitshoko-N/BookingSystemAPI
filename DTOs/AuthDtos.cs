using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.DTOs
{
    public class RegisterParentDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, RegularExpression(ValidationPatterns.Phone, ErrorMessage = ValidationPatterns.PhoneMessage)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, RegularExpression(ValidationPatterns.Password, ErrorMessage = ValidationPatterns.PasswordMessage)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }

        // "Parent", "Admin" or "Clerk". Angular uses this to decide which screens to show.
        public string Role { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}