using System.ComponentModel.DataAnnotations;

namespace BookingSystemAPI.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required, StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [Required, StringLength(250)]
        public string RoleDescription { get; set; } = string.Empty;

        public List<Staff> StaffMembers { get; set; } = new List<Staff>();
    }
}