using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        // Short, human-friendly and unique, e.g. BK-7K4M2X
        [Required, StringLength(12)]
        public string BookingNumber { get; set; } = string.Empty;

        public int ParentId { get; set; }
        public Parent? Parent { get; set; }

        public int StudentNo { get; set; }
        public Student? Student { get; set; }

        public int SlotId { get; set; }
        public Slot? Slot { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;

        // Timestamps are stored in UTC.
        public DateTime DateMade { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        [StringLength(250)]
        public string? CancellationReason { get; set; }

    }
}