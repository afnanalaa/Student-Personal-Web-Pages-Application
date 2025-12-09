namespace Student_Profile.ViewModels
{
    public class StudentProfileViewModel
    {
        public string? Bio { get; set; }
        public string? Interests { get; set; }
        public string? Skills { get; set; }

        public string? GitHub { get; set; }
        public string? LinkedIn { get; set; }
        public string? EmailContact { get; set; }

        public IFormFile? ProfileImage { get; set; }
        public string? ExistingProfileImage { get; set; }
    }
}
