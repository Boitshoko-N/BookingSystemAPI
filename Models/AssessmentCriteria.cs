using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public class AssessmentCriteria
    {
        [Key]
        public int AssessmentCriteriaId { get; set; }

        [Range(0, 100)]
        public int MinimumMathematics { get; set; }

        [Range(0, 100)]
        public int MinimumEnglish { get; set; }

        [Range(0, 100)]
        public int MinimumHomeLanguage { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public int UpdatedByStaffNo { get; set; }
    }
}