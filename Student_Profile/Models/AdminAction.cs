using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Student_Profile.Models
{
    public class AdminAction
    {
        [Key]
        public int Id { get; set; }

        public int? StudentProfileId { get; set; }
        [ForeignKey("StudentProfileId")]
        public StudentProfile StudentProfile { get; set; }

        public int? PostId { get; set; }
        [ForeignKey("PostId")]
        public Post Post { get; set; }

        public int? ComplaintId { get; set; }
        [ForeignKey("ComplaintId")]
        public Complaint Complaint { get; set; }


        [Required]
        public string AdminId { get; set; }
        [ForeignKey("AdminId")]
        public ApplicationUser Admin { get; set; }

        [Required]
        public string Action { get; set; } 

        public DateTime ActionDate { get; set; } = DateTime.Now;

      
    }
}
