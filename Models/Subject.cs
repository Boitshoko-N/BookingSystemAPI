using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public class Subject
    {
        [Key]
        public int SubjectId { get; set; }
        [Required]
        public string SubjectName { get; set; } = string.Empty;
        [Required]
        public string SubjectCode { get; set; } = string.Empty;
        [Required]
        public string SubjectDescription { get; set; } = string.Empty;
        [Required]
        public string Grade { get; set; } = string.Empty;
        [Required]
        public string SubjectType { get; set; } = string.Empty;
        [Required]
        public int StaffNo { get; set; }
        public Staff? Staff { get; set; }
        [Required]
        public int Mark { get; set; }
    }
}
