using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;
using Student_Profile.Models;
using Student_Profile.ViewModels;

namespace Student_Profile.Controllers
{
    public class AdminModerationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminModerationController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var vm = new ModerationViewModel
            {
                PendingPosts = _context.Posts
                    .Include(p => p.User)
                    .Where(p => p.Status == "Pending")
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList(),

                Complaints = _context.Complaints
                    .Include(c => c.User)
                    .Where(c => c.Status == "Pending")
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList()
            };

            return View("~/Views/Admin/Moderation.cshtml", vm);
        }

        // ========================
        // Approve / Reject Post Actions
        // ========================
        [HttpPost]
        public IActionResult ApprovePost(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();

            post.Status = "Approved";
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RejectPost(int id)
        {
            var post = _context.Posts.Find(id);
            if (post == null) return NotFound();

            post.Status = "Rejected";
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // ========================
        // Solve Complaint Action
        // ========================
        [HttpPost]
        public IActionResult SolveComplaint(int id)
        {
            var complaint = _context.Complaints.Find(id);
            if (complaint == null) return NotFound();

            complaint.Status = "Solved";
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
