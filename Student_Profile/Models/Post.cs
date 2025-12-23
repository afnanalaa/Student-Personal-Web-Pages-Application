using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Student_Profile.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public string? Content { get; set; }

        public string? ImageFile { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Admin Approval
        // Pending, Approved, Rejected

        public string Status { get; set; } = "Pending";

        public bool IsReported { get; set; } = false;
        public int ReportsCount { get; set; } = 0;


    }
}
