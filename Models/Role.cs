using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        [Required]
        public string RoleName { get; set; } = string.Empty;
        [Required]
        public string RoleDescription { get; set; } = string.Empty;
        [Required]
        public string RoleType { get; set; } = string.Empty;
        public List<Staff> StaffMembers { get; set; } = new List<Staff>();


    }
}
