using System.ComponentModel.DataAnnotations;

namespace Student_Profile.ViewModels
{
    public class AdminDashboardViewModel
    {
        [Display(Name = "Active Students")]
        public int TotalStudents { get; set; }

       
        [Display(Name = "Pending Requests")]
        public int PendingRequests { get; set; }

        [Display(Name = "Flagged Content & Complaints")]
        public int FlaggedContentCount { get; set; }

    }
}