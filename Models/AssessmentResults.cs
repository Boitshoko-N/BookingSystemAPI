using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public class AssessmentResult
    {
        [Key]
        public int AssessmentResultId { get; set; }

        // The result belongs to the student (one result per student).
        public int StudentNo { get; set; }
        public Student? Student { get; set; }

        // Marks are out of 100. Null means "not captured yet".
        [Range(0, 100)]
        public int? MathematicsMark { get; set; }

        [Range(0, 100)]
        public int? EnglishMark { get; set; }

        [Range(0, 100)]
        public int? HomeLanguageMark { get; set; }

        public AssessmentOutcome Outcome { get; set; } = AssessmentOutcome.Pending;

        // True once staff press "Finalise and send".
        public bool IsFinalised { get; set; }
        public DateTime? FinalisedAt { get; set; }

        // True if the parent has already been told a result before.
        // If marks are corrected later, the next email is marked "Corrected result".
        public bool HasBeenSentToParent { get; set; }

        public int CapturedByStaffNo { get; set; }
        public Staff? CapturedBy { get; set; }

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}