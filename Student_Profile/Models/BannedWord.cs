using System.ComponentModel.DataAnnotations;

namespace Student_Profile.Models
{
    public class BannedWord
    {
        public int Id { get; set; }
        [Required]
        public string Word { get; set; }
    }
}
