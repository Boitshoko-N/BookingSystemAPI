using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public class EmailLog
    {
        [Key]
        public int EmailLogId { get; set; }

        [Required, StringLength(256)]
        public string ToAddress { get; set; } = string.Empty;

        [Required, StringLength(250)]
        public string Subject { get; set; } = string.Empty;

        // Full HTML body, kept so a failed email can be resent exactly as written.
        [Required]
        public string Body { get; set; } = string.Empty;

        public EmailType Type { get; set; }
        public EmailStatus Status { get; set; }

        [StringLength(1000)]
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SentAt { get; set; }

        public int? BookingId { get; set; }
    }
}