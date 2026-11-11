using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.DTOs
{
    public class ParentResponseDto
    {
        public int ParentId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    // A student together with all of their bookings.
    public class StudentWithBookingsDto : StudentResponseDto
    {
        public List<BookingResponseDto> Bookings { get; set; } = new List<BookingResponseDto>();
    }

    // The parent's profile, students and each student's bookings in one call.
    public class ParentDetailsDto : ParentResponseDto
    {
        public List<StudentWithBookingsDto> Students { get; set; } = new List<StudentWithBookingsDto>();
    }

    // Email is not here: it is the login, so it cannot be changed.
    public class UpdateParentDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, RegularExpression(ValidationPatterns.Phone, ErrorMessage = ValidationPatterns.PhoneMessage)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}