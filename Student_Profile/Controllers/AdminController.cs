using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;
using Student_Profile.Models;

namespace Student_Profile.Controllers
{
    // [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Pending student accounts
        public async Task<IActionResult> ReviewRequests()
        {
            var pendingStudents = await _context.Users
                .Where(u => u.AccountStatus == "Pending")
                .ToListAsync();

            var studentsWithRole = new List<ApplicationUser>();

            foreach (var user in pendingStudents)
            {
                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    studentsWithRole.Add(user);
                }
            }

            return View(studentsWithRole);
        }

        // POST: Approve a student account
        [HttpPost]
        public async Task<IActionResult> ApproveStudent(string userId)
        {
            var student = await _context.Users.FindAsync(userId);
            if (student == null) return NotFound();

            student.AccountStatus = "Approved";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Student {student.UserName} approved successfully!";
            return RedirectToAction(nameof(ReviewRequests));
        }

        // POST: Reject a student account
        [HttpPost]
        public async Task<IActionResult> RejectStudent(string userId)
        {
            var student = await _context.Users.FindAsync(userId);
            if (student == null) return NotFound();

            student.AccountStatus = "Rejected";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Student {student.UserName} rejected and deleted!";
            return RedirectToAction(nameof(ReviewRequests));
        }

        public async Task<IActionResult> ActiveRequests()
        {
            var pendingStudents = await _context.Users
                .Where(u => u.AccountStatus == "Approved")
                .ToListAsync();

            var studentsWithRole = new List<ApplicationUser>();

            foreach (var user in pendingStudents)
            {
                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    studentsWithRole.Add(user);
                }
            }

            return View(studentsWithRole);
        }

        public async Task<IActionResult> PendingPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Where(p => p.Status == "Pending")
                .ToListAsync();

            return View(posts);
        }
        [HttpPost]
        public async Task<IActionResult> ApprovePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            post.Status = "Approved";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post approved successfully!";
            return RedirectToAction(nameof(PendingPosts));
        }
        [HttpPost]
        public async Task<IActionResult> RejectPost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            post.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post rejected!";
            return RedirectToAction(nameof(PendingPosts));
        }

        // All complains pending,resolved,tech....
        //public async Task<IActionResult> Complaints()
        //{
        //    var complaints = await _context.Complaints
        //        .Include(c => c.User)
        //        .OrderByDescending(c => c.CreatedAt)
        //        .ToListAsync();

        //    return View(complaints);
        //}

        public async Task<IActionResult> Complaints()
        {
            var complaints = await _context.Complaints
                .Include(c => c.User)
                .Where(c => c.Status == "Pending") // بس الشكاوي Pending
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }


        // POST: Assign to technician
        [HttpPost]
        public async Task<IActionResult> AssignToTechnical(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            complaint.Status = "Assigned to Technical";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Complaint assigned to technical successfully!";
            return RedirectToAction(nameof(Complaints));
        }

        // POST: Mark as resolved
        [HttpPost]
        public async Task<IActionResult> MarkAsResolved(int id)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            complaint.Status = "Resolved";
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Complaint marked as resolved!";
            return RedirectToAction(nameof(Complaints));
        }

        // POST: Contact sender (optional: maybe just set a comment)
        [HttpPost]
        public async Task<IActionResult> ContactSender(int id, string adminComment)
        {
            var complaint = await _context.Complaints.FindAsync(id);
            if (complaint == null) return NotFound();

            complaint.AdminComment = adminComment;
            complaint.Status = "Contact";
           
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Sender has been contacted with your comment.";
            return RedirectToAction(nameof(Complaints));
        }



    }
}
