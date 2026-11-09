using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public class Slot
    {
        [Key]
        public int SlotId { get; set; }
        [Required]
        public string SlotTime { get; set; } = string.Empty;
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }
        [Required]
        public int SlotDate { get; set; }
        public bool IsAvailable { get; set; } = true;
        [Required]
        public int MaxBookings { get; set; }
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
