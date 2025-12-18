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
        public async Task<IActionResult> Index() 
        {
            //var reportedPosts = await _context.Posts
            //    .Include(p => p.User)
            //    .Where(p => p.IsReported == true )
            //    .ToListAsync(); 

            var vm = new ModerationViewModel
            {
               
                Complaints = await _context.Complaints
                    .Include(c => c.User)
                    .Where(c => c.Status == "Pending")
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync(),
                ReportedPosts = await _context.Posts
                    .Include(p => p.User)
                    .Where(p => p.IsReported == true)
                    .OrderByDescending(p => p.ReportsCount)
                    .ToListAsync()

            };

            return View("~/Views/Admin/Moderation.cshtml", vm);
        }
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


        [HttpPost]
        public async Task<IActionResult> DeleteInappropriatePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // Solve Complaint Action
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
