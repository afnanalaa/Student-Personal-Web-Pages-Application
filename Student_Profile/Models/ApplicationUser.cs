using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Student_Profile.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string NationalId { get; set; }

        [Required]
        public string? StudentCardImageORNationalUrl { get; set; }

        public string AccountStatus { get; set; } = "Pending";


        public StudentProfile StudentProfile { get; set; }
    }
}
