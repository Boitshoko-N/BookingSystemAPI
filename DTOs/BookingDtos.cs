using System.ComponentModel.DataAnnotations;
using BookingSystemAPI.Models;

namespace BookingSystemAPI.DTOs
{
    public class CreateBookingDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select a student.")]
        public int StudentNo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select an assessment slot.")]
        public int SlotId { get; set; }
    }

    // Updating a booking means moving it to a different slot.
    public class UpdateBookingDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Please select an assessment slot.")]
        public int SlotId { get; set; }
    }

    public class CancelBookingDto
    {
        [StringLength(250)]
        public string? Reason { get; set; }
    }

    // Used in lists.
    public class BookingResponseDto
    {
        public int BookingId { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public BookingStatus Status { get; set; }
        public int StudentNo { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int SlotId { get; set; }
        public DateOnly AssessmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public DateTime DateMade { get; set; }
    }

    // Used when viewing a single booking.
    public class BookingDetailsDto : BookingResponseDto
    {
        public int ParentId { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public string ParentEmail { get; set; } = string.Empty;
        public string ParentPhoneNumber { get; set; } = string.Empty;
        public string StudentGrade { get; set; } = string.Empty;
        public string StudentHomeLanguage { get; set; } = string.Empty;
        public DateTime? LastModifiedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }

    }
}