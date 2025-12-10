using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; 

namespace Student_Profile.ViewModels
{
    public class PostViewModel
    {
        [StringLength(500, ErrorMessage = "Post cannot exceed 500 characters.")]
        [Display(Name = "Post Content")]
        public string Content { get; set; }

        [Display(Name = "Image/File")]
        public IFormFile ImageFile { get; set; }
    }

}