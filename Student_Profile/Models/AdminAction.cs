using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Student_Profile.Models
{
    public class AdminAction
    {
        [Key]
        public int Id { get; set; }

        // القرار اتخذ على حساب الطالب (اختياري)
        public int? StudentProfileId { get; set; }
        [ForeignKey("StudentProfileId")]
        public StudentProfile StudentProfile { get; set; }

        // القرار اتخذ على بوست (اختياري)
        public int? PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }

        // الـ Admin اللي اتخذ القرار
        [Required]
        public string AdminId { get; set; }
        [ForeignKey("AdminId")]
        public ApplicationUser Admin { get; set; }

        // نوع الإجراء
        [Required]
        public string Action { get; set; } // Approved / Rejected

        // تاريخ اتخاذ القرار
        public DateTime ActionDate { get; set; } = DateTime.Now;
    }
}
