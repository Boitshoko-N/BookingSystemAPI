using System.ComponentModel.DataAnnotations;
using BookingSystemAPI.Models;

namespace BookingSystemAPI.DTOs
{
    public class CreateSlotDto
    {
        public DateOnly AssessmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        [Range(1, 500, ErrorMessage = "Maximum bookings must be between 1 and 500.")]
        public int MaxBookings { get; set; }
    }

    public class UpdateSlotDto
    {
        public DateOnly AssessmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        [Range(1, 500, ErrorMessage = "Maximum bookings must be between 1 and 500.")]
        public int MaxBookings { get; set; }

        // Open, Closed or Cancelled. ("Full" is calculated, so it can't be set.)
        [EnumDataType(typeof(SlotStatus), ErrorMessage = "Status must be Open, Closed or Cancelled.")]
        public SlotStatus Status { get; set; }
    }

    public class SlotResponseDto
    {
        public int SlotId { get; set; }
        public DateOnly AssessmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int MaxBookings { get; set; }

        // Bookings that are not Cancelled.
        public int ActiveBookings { get; set; }
        public int AvailablePlaces { get; set; }

        // The stored status: Open, Closed or Cancelled.
        public SlotStatus Status { get; set; }

        public bool IsFull { get; set; }

        // True only if the slot is Open, has places left and has not started.
        // Angular can use this directly instead of repeating the rules.
        public bool IsBookable { get; set; }

        // For display: "Open", "Closed", "Cancelled" or "Full".
        public string DisplayStatus { get; set; } = string.Empty;
    }
}