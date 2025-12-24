using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Student_Profile.Models
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        public string Type { get; set; } = "General"; 
        public string Status { get; set; } = "Pending"; 
        public string? AdminComment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
