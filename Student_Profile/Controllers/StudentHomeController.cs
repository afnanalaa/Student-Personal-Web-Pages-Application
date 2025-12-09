using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;
using System.Security.Claims;

namespace Student_Profile.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentHomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentHomeController(ApplicationDbContext context)
        {
            _context = context;
        }

   
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            if (user.AccountStatus == "Pending")
                return RedirectToAction("PendingApproval", "Student");

            if (user.AccountStatus == "Rejected")
                return RedirectToAction("Rejected", "Student");

           
            var posts = await _context.Posts
                .Include(p => p.User)
                .Where(p => p.Status == "Approved")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }
    }
}
