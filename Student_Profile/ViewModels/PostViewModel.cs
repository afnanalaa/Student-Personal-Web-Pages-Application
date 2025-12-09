using System.ComponentModel.DataAnnotations;

namespace Student_Profile.ViewModels
{
    public class PostViewModel
    {
        [Required]
        [StringLength(500, ErrorMessage = "Post cannot exceed 500 characters.")]
        [Display(Name = "Post Content")]
        public string Content { get; set; }
    }
}
