using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Student_Profile.Models
{
    public class StudentProfile
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        // 🔹 بيانات اختيارية

        public string? Bio { get; set; }
        public string? Interests { get; set; }
        public string? Skills { get; set; }

        public string Address { get; set; }
        public string Department { get; set; } 
        
        [MaxLength(500)]
        public string? Projects { get; set; }

        // 🔹 روابط
        public string? ContactInformation { get; set; }

        // 🔹 صورة البروفايل (اختيارية)
        public string? ProfileImageUrl { get; set; }

        // 🔹 رابط URL فريد
        [Required]
        public string ProfileSlug { get; set; }
    }
}
