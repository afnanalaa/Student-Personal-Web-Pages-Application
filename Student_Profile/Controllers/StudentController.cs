using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Student_Profile.Data;
using Student_Profile.Models;
using Student_Profile.Services;
using Student_Profile.ViewModels;
using System.Security.Claims;

namespace Student_Profile.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string defaultImage = "default-avatar.png";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ContentFilterService _filterService; 

        public StudentController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> userManager, ContentFilterService filterService)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
            _filterService = filterService;
        }


        [HttpGet]
        public async Task<IActionResult> SetPrivacy(string mode)
        {
            var allowedModes = new[] { "Public", "University", "Private" };
            if (!allowedModes.Contains(mode))
            {
                return BadRequest("Invalid privacy mode.");
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound();

            profile.PrivacyMode = mode;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Your profile is now {mode}.";
            return RedirectToAction("StudentDashboard"); 
}



        [HttpGet]
        public async Task<IActionResult> CreateForm()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (user.AccountStatus == "Pending") return RedirectToAction("PendingApproval");
            if (user.AccountStatus == "Rejected") return RedirectToAction("Rejected");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForm(StudentProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            string? fileName = null;
            if (model.ProfileImage != null)
            {
                fileName = await SaveProfileImage(model.ProfileImage);
            }

            var profile = new StudentProfile
            {
                UserId = userId,
                Department = model.Department,
                Address = model.Address,
                Bio = model.Bio,
                Interests = model.Interests,
                Skills = model.Skills,
                Projects = model.Projects,
                ContactInformation = model.ContactInformation,
                ProfileImageUrl = fileName ?? defaultImage,
                ProfileSlug = Guid.NewGuid().ToString("N")
            };

            _context.StudentProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return Json(new { slug = profile.ProfileSlug });
        }

        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var profile = await _context.StudentProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return RedirectToAction("CreateForm");

            var approvedPosts = await _context.Posts
          .Include(p => p.User) 
          .Include(p => p.User.StudentProfile) 
           .Where(p => p.Status == "Approved")
          .ToListAsync();

            ViewBag.ApprovedPosts = approvedPosts;

            return View(profile);
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var profile = await _context.StudentProfiles
                .Include(p => p.User) 
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return RedirectToAction("CreateForm");

            var complaints = await _context.Complaints
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

            ViewBag.UserComplaints = complaints;

            return View("StudentDashboard", profile);
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportPost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound();

            post.IsReported = true;
            post.ReportsCount += 1;
            post.Status = "Reported";


            await _context.SaveChangesAsync();

            TempData["Success"] = "The post has been reported successfully.";
            return RedirectToAction("MyProfile");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(PostViewModel model)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(model.Content) && model.ImageFile == null)
            {
                TempData["Error"] = "Post must contain either text content or an image.";
                return RedirectToAction("MyProfile");
            }

            if (await _filterService.ContainsProhibitedContent(model.Content))
            {
                TempData["Error"] = "Your post contains restricted language.";
                return RedirectToAction("MyProfile");
            }

            string imageUrl = null;

            if (model.ImageFile != null)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string uploadPath = Path.Combine(wwwRootPath, "images", "posts");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(model.ImageFile.FileName);

                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName + extension), FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                imageUrl = Path.Combine("/images/posts", fileName + extension).Replace('\\', '/');
            }

            var post = new Post
            {
                UserId = userId,
                Content = model.Content,
                ImageFile = imageUrl,
                Status = "Pending", 
                CreatedAt = DateTime.Now
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your post has been submitted and is pending admin approval.";
            return RedirectToAction("MyProfile");
        }
    

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return RedirectToAction("CreateForm");

            var model = new StudentProfileViewModel
            {
                Bio = profile.Bio,
                Address = profile.Address,
                Department = profile.Department,
                Interests = profile.Interests,
                Skills = profile.Skills,
                ContactInformation = profile.ContactInformation,
                Projects = profile.Projects,
                ExistingProfileImage = profile.ProfileImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(StudentProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _filterService.ContainsProhibitedContent(model.Bio) ||
                await _filterService.ContainsProhibitedContent(model.Skills) ||
                await _filterService.ContainsProhibitedContent(model.Projects))
            {
                ModelState.AddModelError("", "Action Denied: Your profile content contains words restricted by the university policy.");

                return View(model);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound();

            profile.Bio = model.Bio;
            profile.Interests = model.Interests;
            profile.Skills = model.Skills;
            profile.Address = model.Address;
            profile.Department = model.Department;
            profile.Projects = model.Projects;
            profile.ContactInformation = model.ContactInformation;

            if (model.ProfileImage != null)
            {
                if (!string.IsNullOrEmpty(profile.ProfileImageUrl) && profile.ProfileImageUrl != defaultImage)
                {
                    DeleteProfileImage(profile.ProfileImageUrl);
                }
                profile.ProfileImageUrl = await SaveProfileImage(model.ProfileImage);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("MyProfile");
        }

        [HttpGet]
        public async Task<IActionResult> EditImage()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound();

            var model = new StudentProfileImageViewModel
            {
                ExistingProfileImage = profile.ProfileImageUrl
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditImage(StudentProfileImageViewModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound();

            if (model.ProfileImage != null)
            {
                if (!string.IsNullOrEmpty(profile.ProfileImageUrl) && profile.ProfileImageUrl != defaultImage)
                {
                    DeleteProfileImage(profile.ProfileImageUrl);
                }

                profile.ProfileImageUrl = await SaveProfileImage(model.ProfileImage);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("EditProfile");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveProfileImage()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound();

            if (!string.IsNullOrEmpty(profile.ProfileImageUrl) && profile.ProfileImageUrl != defaultImage)
            {
                DeleteProfileImage(profile.ProfileImageUrl);
            }

            profile.ProfileImageUrl = defaultImage;
            await _context.SaveChangesAsync();

            return RedirectToAction("EditProfile");
        }

        private async Task<string> SaveProfileImage(IFormFile image)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "student-profiles");

            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string filePath = Path.Combine(uploadsFolder, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            return fileName;
        }

        private void DeleteProfileImage(string fileName)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "student-profiles", fileName);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }


        public IActionResult PendingApproval()
        {
            return View();
        }

        public IActionResult Rejected()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Details(string slug)
        {
            var profile = _context.StudentProfiles
                .Include(p => p.User)
                .FirstOrDefault(p => p.ProfileSlug == slug);

            if (profile == null) return NotFound();

            if (profile.PrivacyMode == "Private" && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (profile.PrivacyMode == "University" && !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewData["Layout"] = "~/Views/Shared/_Layout.cshtml";

            return View("CVDetails", profile);
        }



    }
}
