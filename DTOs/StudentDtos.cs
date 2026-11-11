using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.DTOs
{
    public class CreateStudentDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        // The grade the child is applying for.
        [Required, StringLength(20)]
        public string Grade { get; set; } = string.Empty;

        // Decides which home language test the student writes.
        [Required, StringLength(50)]
        public string HomeLanguage { get; set; } = string.Empty;
    }

    public class UpdateStudentDto
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Grade { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string HomeLanguage { get; set; } = string.Empty;
    }

    public class StudentResponseDto
    {
        public int StudentNo { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Grade { get; set; } = string.Empty;
        public string HomeLanguage { get; set; } = string.Empty;
        public int ParentId { get; set; }
        public string ParentName { get; set; } = string.Empty;
    }
}