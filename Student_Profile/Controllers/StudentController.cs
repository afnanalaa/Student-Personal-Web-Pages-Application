using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;
using Student_Profile.Models;
using Student_Profile.ViewModels;
using System.Security.Claims;

namespace Student_Profile.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly string defaultImage = "default-avatar.png";

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Create Profile
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (user.AccountStatus == "Pending") return RedirectToAction("PendingApproval");
            if (user.AccountStatus == "Rejected") return RedirectToAction("Rejected");

            return View();
        }

        // POST: Create Profile
        [HttpPost]
        public async Task<IActionResult> Create(StudentProfileViewModel model)
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
                Bio = model.Bio,
                Interests = model.Interests,
                Skills = model.Skills,
                GitHub = model.GitHub,
                LinkedIn = model.LinkedIn,
                EmailContact = model.EmailContact,
                ProfileImageUrl = fileName ?? defaultImage,
                ProfileSlug = Guid.NewGuid().ToString("N")
            };

            _context.StudentProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { slug = profile.ProfileSlug });
        }

        // GET: MyProfile
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.StudentProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return RedirectToAction("Create");

            return View(profile);
        }

        // GET: EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return RedirectToAction("Create");

            var model = new StudentProfileViewModel
            {
                Bio = profile.Bio,
                Interests = profile.Interests,
                Skills = profile.Skills,
                GitHub = profile.GitHub,
                LinkedIn = profile.LinkedIn,
                EmailContact = profile.EmailContact,
                ExistingProfileImage = profile.ProfileImageUrl
            };

            return View(model);
        }

        // POST: EditProfile
        [HttpPost]
        public async Task<IActionResult> EditProfile(StudentProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var profile = await _context.StudentProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound();

            profile.Bio = model.Bio;
            profile.Interests = model.Interests;
            profile.Skills = model.Skills;
            profile.GitHub = model.GitHub;
            profile.LinkedIn = model.LinkedIn;
            profile.EmailContact = model.EmailContact;

            if (model.ProfileImage != null)
            {
                if (!string.IsNullOrEmpty(profile.ProfileImageUrl) && profile.ProfileImageUrl != defaultImage)
                {
                    DeleteProfileImage(profile.ProfileImageUrl);
                }

                profile.ProfileImageUrl = await SaveProfileImage(model.ProfileImage);
            }
            else if (string.IsNullOrEmpty(profile.ProfileImageUrl))
            {
                profile.ProfileImageUrl = defaultImage;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyProfile");
        }

        // GET: EditImage
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

        // POST: EditImage
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

        // POST: RemoveProfileImage
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

        // 🔹 Helpers
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

        [HttpGet]
        public IActionResult Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            var profile = _context.StudentProfiles
                .Include(p => p.User) // 🔹 هنا
                .FirstOrDefault(p => p.ProfileSlug == slug);

            if (profile == null)
                return NotFound();

            return View(profile);
        }

    }
}
