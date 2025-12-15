using Student_Profile.Models;

namespace Student_Profile.ViewModels
{
    public class ModerationViewModel
    {
        public List<Post> PendingPosts { get; set; } = new List<Post>();

        public List<Complaint> Complaints { get; set; } = new List<Complaint>();

        public List<Post> ReportedPosts { get; set; } = new List<Post>();
    }
}
