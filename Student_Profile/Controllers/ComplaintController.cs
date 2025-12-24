using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;
using Student_Profile.Models;
using System.Security.Claims;

namespace Student_Profile.Controllers
{
    [Authorize(Roles = "Student")]
    public class ComplaintController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ComplaintController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string subject, string content)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(content))
            {
                ModelState.AddModelError("", "Subject and Content are required.");
                return View();
            }

            var complaint = new Complaint
            {
                UserId = userId,
                Subject = subject,
                Content = content
            };

            _context.Complaints.Add(complaint);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Complaint submitted successfully!";
            return RedirectToAction("Index","Home");
        }

        public async Task<IActionResult> MyComplaints()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var complaints = await _context.Complaints
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }
    }
}
