
using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public abstract class Person
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;

    }

    public class Parent : Person
    {
        [Key]
        public int ParentId { get; set; }
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Booking> Bookings { get; set; } = new List<Booking>();

    }

    public class Student : Person
    {
        [Key]
        public int StudentNo { get; set; }
        [Required]
        public string Grade { get; set; } = string.Empty;
        [Required]
        public string email { get; set; } = string.Empty;
        [Required] 
        public string phoneNumber { get; set; } = string.Empty;
        [Required]
        public int ParentId { get; set; }
        public Parent? Parent { get; set; }
        public bool IsRegistered { get; set; } = false;
    }
    public class Staff:Person
    {
        [Key]
        public int StaffNo { get; set; }
        [Required]
        public string email { get; set; } = string.Empty;
        [Required]
        public string phoneNumber { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        [Required]
        public string Password { get; set; } = string.Empty;
    }

}
