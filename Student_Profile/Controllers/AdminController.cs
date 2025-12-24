using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;
using Student_Profile.Models;
using Student_Profile.ViewModels;

namespace Student_Profile.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private IEmailSender _emailSender;
        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender= emailSender;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View("Dashboard");

          
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View("~/Views/Admin/Login.cshtml");
        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,         
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (!isAdmin)
            {
                await _signInManager.SignOutAsync();
                ModelState.AddModelError("", "Access Denied: Not authorized as Admin.");
                return View(model);
            }

            return RedirectToAction("Dashboard", "Admin");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var studentUsers = await _userManager.GetUsersInRoleAsync("Student");
            int totalApprovedStudents = studentUsers.Count(u => u.AccountStatus == "Approved");

            int pendingRegistrations = await _userManager.Users
                                            .CountAsync(u => u.AccountStatus == "Pending");

            int pendingPosts = await _context.Posts
                                    .CountAsync(p => p.Status == "Pending");

            int totalPendingRequests = pendingRegistrations + pendingPosts;


            int reportedPosts = await _context.Posts
                                    .CountAsync(p => p.IsReported == true);


            int unresolvedComplaints = await _context.Complaints
                                    .CountAsync(c => c.Status=="Pending");

            int flaggedContentCount = reportedPosts + unresolvedComplaints;

            var viewModel = new AdminDashboardViewModel
            {
                TotalStudents = totalApprovedStudents,
                PendingRequests = totalPendingRequests,
                FlaggedContentCount = flaggedContentCount
            };

            return View(viewModel);
        }


        public async Task<IActionResult> ReviewRequests()
        {
            var pendingStudents = _context.Users
                .Where(u => u.AccountStatus == "Pending")
                .ToList();

            var studentsWithRole = new List<ApplicationUser>();

            foreach (var user in pendingStudents)
            {
                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    studentsWithRole.Add(user);
                }
            }

            var pendingPosts = _context.Posts
                .Where(p => p.Status == "Pending")
                .Include(p => p.User)
                .ToList();

            var model = new PendingRequestsViewModel
            {
                PendingAccounts = studentsWithRole,  
                PendingPosts = pendingPosts
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ApproveStudent(string userId)
        {
            var student = await _context.Users.FindAsync(userId);
            if (student == null) return NotFound();

            student.AccountStatus = "Approved";
            await _context.SaveChangesAsync();

           

            var actionRecord = new AdminAction
            {
                StudentProfileId = student.Id != null ? student.StudentProfile?.Id : null,
                AdminId = _userManager.GetUserId(User),
                Action = "Approved",
                ActionDate = DateTime.Now
            };

            _context.AdminActions.Add(actionRecord);
            await _context.SaveChangesAsync();

            var loginUrl = Url.Action("Login", "Account", null, Request.Scheme);

            var emailBody = $@"
                <h2>🎉 Account Approved</h2>
                <p>Hello <b>{student.FullName}</b>,</p>
                <p>Your account has been approved successfully.</p>
                 <p>
                 <a href='{loginUrl}' 
                  style='padding:10px 15px;background:#198754;color:white;text-decoration:none; margin-top:20px;'>
                  Login Now
                 </a>
                </p>
                      ";

            await _emailSender.SendEmailAsync(
                student.Email,
                "Your Account Has Been Approved",
                emailBody
            );

            TempData["SuccessMessage"] = $"Student {student.FullName} approved successfully!";
            return RedirectToAction(nameof(ReviewRequests));
        }


        [HttpPost]
        public async Task<IActionResult> RejectStudent(string userId)
        {
            var student = await _context.Users.FindAsync(userId);
            if (student == null) return NotFound();

            student.AccountStatus = "Rejected";
            await _context.SaveChangesAsync();

            var actionRecord = new AdminAction
            {
                StudentProfileId = student.Id != null ? student.StudentProfile?.Id : null,
                AdminId = _userManager.GetUserId(User),
                Action = "Approved",
                ActionDate = DateTime.Now
            };

            _context.AdminActions.Add(actionRecord);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Student {student.FullName} rejected successfully!";
            return RedirectToAction(nameof(ReviewRequests));
        }


        [HttpGet]
        public async Task<IActionResult> ActiveStudents()
        {
            var students = await _userManager.Users
                .Where(u => u.AccountStatus == "Approved" || u.AccountStatus == "Graduated")
                .ToListAsync();

            var studentsWithRole = new List<ApplicationUser>();

            foreach (var user in students)
            {
                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    studentsWithRole.Add(user);
                }
            }

            return View(studentsWithRole);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(string userId)
        {
            var student = await _userManager.FindByIdAsync(userId);
            if (student == null) return NotFound();

            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile != null)
            {
                _context.StudentProfiles.Remove(profile);
            }

            var result = await _userManager.DeleteAsync(student);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Student {student.FullName} has been deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error occurred while deleting the student.";
            }

            return RedirectToAction(nameof(ActiveStudents));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsGraduated(string userId)
        {
            var student = await _userManager.FindByIdAsync(userId);
            if (student == null) return NotFound();

            student.AccountStatus = "Graduated";
            var result = await _userManager.UpdateAsync(student);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Student {student.FullName} is now marked as Graduated.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update student status.";
            }

            return RedirectToAction(nameof(ActiveStudents));
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

        public async Task<IActionResult> Complaints()
        {
            var complaints = await _context.Complaints
                .Include(c => c.User)
                .Where(c => c.Status == "Pending") 
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(complaints);
        }

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

      
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ReportedPosts()
        {
            
            var reportedPosts = await _context.Posts
                .Include(p => p.User) 
                .Where(p => p.IsReported == true)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(reportedPosts);
        }
    }
}
