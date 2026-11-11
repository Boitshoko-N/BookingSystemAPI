using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public class Slot
    {
        [Key]
        public int SlotId { get; set; }

        // Date and times are in the school's local time (South Africa).
        public DateOnly AssessmentDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public int MaxBookings { get; set; }

        // Open, Closed or Cancelled. "Full" is calculated from bookings, never stored.
        public SlotStatus Status { get; set; } = SlotStatus.Open;

        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}