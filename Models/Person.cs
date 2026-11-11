using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public abstract class Person
    {
        [Required, StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string LastName { get; set; } = string.Empty;
    }

    public class Parent : Person
    {
        [Key]
        public int ParentId { get; set; }

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        // BCrypt hash. The real password is never stored.
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Student> Students { get; set; } = new List<Student>();
        public List<Booking> Bookings { get; set; } = new List<Booking>();
    }

    public class Student : Person
    {
        [Key]
        public int StudentNo { get; set; }

        // The grade the child is applying for.
        [Required, StringLength(20)]
        public string Grade { get; set; } = string.Empty;

        // Decides which home language test the student writes.
        [Required, StringLength(50)]
        public string HomeLanguage { get; set; } = string.Empty;

        public int ParentId { get; set; }
        public Parent? Parent { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();
        public AssessmentResult? AssessmentResult { get; set; }
    }

    public class Staff : Person
    {
        [Key]
        public int StaffNo { get; set; }

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}