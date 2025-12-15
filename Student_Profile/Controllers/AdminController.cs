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

        // GET: عرض صفحة تسجيل الدخول
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View("~/Views/Admin/Login.cshtml");
        }

        // POST: معالجة بيانات تسجيل الدخول
    
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
            // 1️⃣ الطلاب المعتمدين
            var studentUsers = await _userManager.GetUsersInRoleAsync("Student");
            int totalApprovedStudents = studentUsers.Count(u => u.AccountStatus == "Approved");

            // 2️⃣ طلبات التسجيل المعلقة
            int pendingRegistrations = await _userManager.Users
                                            .CountAsync(u => u.AccountStatus == "Pending");

            // 3️⃣ البوستات المعلقة
            int pendingPosts = await _context.Posts
                                    .CountAsync(p => p.Status == "Pending");

            int totalPendingRequests = pendingRegistrations + pendingPosts;

            // ⭐ 4️⃣ المحتوى المبلّغ عنه + الشكاوى

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


        // GET: Pending student accounts

        //public async Task<IActionResult> ReviewRequests()
        //{
        //    var pendingStudents = await _context.Users
        //        .Where(u => u.AccountStatus == "Pending")
        //        .ToListAsync();

        //    var studentsWithRole = new List<ApplicationUser>();

        //    foreach (var user in pendingStudents)
        //    {
        //        if (await _userManager.IsInRoleAsync(user, "Student"))
        //        {
        //            studentsWithRole.Add(user);
        //        }
        //    }

        //    return View(studentsWithRole);
        //}
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



        public IActionResult Moderation()
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

            return View(vm);
        }
        // without mail message to student to know if he approved or not :)
        //[HttpPost]
        //public async Task<IActionResult> ApproveStudent(string userId)
        //{
        //    var student = await _context.Users.FindAsync(userId);
        //    if (student == null) return NotFound();

        //    student.AccountStatus = "Approved";
        //    await _context.SaveChangesAsync();

        //    var adminAction = new AdminAction
        //    {
        //        StudentProfileId = null,                
        //        AdminId = _userManager.GetUserId(User),  
        //        Action = "Approved",
        //        ActionDate = DateTime.Now
        //    };

        //    _context.AdminActions.Add(adminAction);
        //    await _context.SaveChangesAsync();

        //    TempData["SuccessMessage"] = $"Student {student.FullName} approved successfully!";
        //    return RedirectToAction(nameof(ReviewRequests));
        //}


        [HttpPost]
        public async Task<IActionResult> ApproveStudent(string userId)
        {
            var student = await _context.Users.FindAsync(userId);
            if (student == null) return NotFound();

            student.AccountStatus = "Approved";
            await _context.SaveChangesAsync();

            //var adminAction = new AdminAction
            //{
            //    StudentProfileId = null,
            //    AdminId = _userManager.GetUserId(User),
            //    Action = "Approved",
            //    ActionDate = DateTime.Now
            //};

            //_context.AdminActions.Add(adminAction);
            //await _context.SaveChangesAsync();

            var actionRecord = new AdminAction
            {
                StudentProfileId = student.Id != null ? student.StudentProfile?.Id : null,
                AdminId = _userManager.GetUserId(User),
                Action = "Approved",
                ActionDate = DateTime.Now
            };

            _context.AdminActions.Add(actionRecord);
            await _context.SaveChangesAsync();

            // 🔥 EMAIL PART
            var loginUrl = Url.Action("Login", "Account", null, Request.Scheme);

            var emailBody = $@"
        <h2>🎉 Account Approved</h2>
        <p>Hello <b>{student.FullName}</b>,</p>
        <p>Your account has been approved successfully.</p>
        <p>
            <a href='{loginUrl}' 
               style='padding:10px 15px;background:#198754;color:white;text-decoration:none;'>
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

            //var adminAction = new AdminAction
            //{
            //    StudentProfileId = null,
            //    AdminId = _userManager.GetUserId(User),
            //    Action = "Rejected",
            //    ActionDate = DateTime.Now
            //};

            //_context.AdminActions.Add(adminAction);
            //await _context.SaveChangesAsync();

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


        public async Task<IActionResult> ActiveStudents()
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

            //var actionRecord = new AdminAction
            //{
            //    PostId = post.Id,
            //    AdminId = _userManager.GetUserId(User),
            //    Action = "Approved",
            //    ActionDate = DateTime.Now
            //};
            //_context.AdminActions.Add(actionRecord);
            //await _context.SaveChangesAsync();

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
