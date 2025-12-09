using System.ComponentModel.DataAnnotations;

namespace Student_Profile.ViewModels
{
    public class StudentProfileViewModel
    {
        public string? Bio { get; set; }
        public string? Interests { get; set; }
        public string? Skills { get; set; }

        public required string Address { get; set; }

        public required string Department { get; set; }

        [MaxLength(500)]
        public string? Projects { get; set; }


        public string? ContactInformation { get; set; }

        public IFormFile? ProfileImage { get; set; }
        public string? ExistingProfileImage { get; set; }
    }
}
