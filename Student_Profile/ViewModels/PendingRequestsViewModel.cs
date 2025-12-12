using Student_Profile.Models;

namespace Student_Profile.ViewModels
{
    public class PendingRequestsViewModel
    {
        public List<ApplicationUser> PendingAccounts { get; set; }
        public List<Post> PendingPosts { get; set; }
    }
}
