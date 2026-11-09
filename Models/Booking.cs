
using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }
        [Required]
        public string BookingTime { get; set; } = string.Empty;
        [Required]
        public int StudentNo { get; set; }
        public Student? Student { get; set; }
        [Required]
        public string StudentName { get; set; } = string.Empty;
        [Required]
        public string ParentName { get; set; } = string.Empty;
        [Required]
        public string ParentEmail { get; set; } = string.Empty;
        [Required]
        public string ParentPhoneNumber { get; set; } = string.Empty;
        [Required]
        public string Grade { get; set; } = string.Empty;
        [Required]
        public DateTime DateMade { get; set; }
        [Required]
        public string HomeLanguage { get; set; } = string.Empty;
        [Required]
        public string BookingStatus { get; set; } = string.Empty;
        [Required]
        public string BookingType { get; set; } = string.Empty;
        [Required]
        public int SlotId { get; set; }
        [Required]
        public int SlotDateId { get; set; }
        public Slot? Slot { get; set; }

        public Parent? Parent { get; set; }
        public int ParentId { get; set; }
    }
}
