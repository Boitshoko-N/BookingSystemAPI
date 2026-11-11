using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.DTOs
{
    public class CreateStaffDto
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

        [Required, RegularExpression("^(Admin|Clerk)$", ErrorMessage = "Role must be either Admin or Clerk.")]
        public string RoleName { get; set; } = string.Empty;
    }

    public class UpdateStaffDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, RegularExpression(ValidationPatterns.Phone, ErrorMessage = ValidationPatterns.PhoneMessage)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, RegularExpression("^(Admin|Clerk)$", ErrorMessage = "Role must be either Admin or Clerk.")]
        public string RoleName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }

    public class StaffResponseDto
    {
        public int StaffNo { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required, StringLength(100)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, RegularExpression(ValidationPatterns.Password, ErrorMessage = ValidationPatterns.PasswordMessage)]
        public string NewPassword { get; set; } = string.Empty;
    }
}