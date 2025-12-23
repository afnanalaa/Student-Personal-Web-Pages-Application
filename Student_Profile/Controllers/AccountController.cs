using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Models;
using Student_Profile.Utility;
using Student_Profile.ViewModels;
using Student_Profile.Data;

namespace Student_Profile.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
        }

        // ----------------- ACCESS DENIED -----------------
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ----------------- REGISTER STUDENT (GET) -----------------
        [HttpGet]
        public IActionResult RegisterStudent()
        {
            return View();
        }

        // ----------------- REGISTER STUDENT (POST) -----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStudent(StudentRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            string fileName = null;

            // حفظ صورة البطاقة / الكارنيه
            if (model.StudentCardOrNationalImage != null)
            {
                // ============================================================
                // 🔒 SECURITY CHECK START: التحقق من نوع الملف وحجمه
                // ============================================================

                // 1. تحديد الامتدادات المسموحة
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf" };
                var extension = Path.GetExtension(model.StudentCardOrNationalImage.FileName).ToLower();

                // 2. فحص الامتداد
                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("StudentCardOrNationalImage", "Invalid file type. Only images (JPG, PNG) and PDF files are allowed.");
                    return View(model);
                }

                // 3. فحص الحجم (5 ميجا كحد أقصى)
                if (model.StudentCardOrNationalImage.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("StudentCardOrNationalImage", "File size is too large. The maximum allowed size is 5MB.");
                    return View(model);
                }
                // ============================================================
                // 🔒 SECURITY CHECK END
                // ============================================================

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // استخدام الامتداد الذي تم فحصه لضمان الأمان
                fileName = Guid.NewGuid() + extension;
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.StudentCardOrNationalImage.CopyToAsync(fileStream);
                }
            }

            else
            {
                ModelState.AddModelError("StudentCardOrNationalImage", "Identity document is required.");
                return View(model);
            }

            // إنشاء الطالب
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                NationalId = model.NationalId,
                PhoneNumber = model.PhoneNumber,
                StudentCardImageORNationalUrl = fileName != null ? "/images/" + fileName : null, // تأكدنا من حفظ المسار الصحيح

                // الحساب يحتاج موافقة الأدمن، لذا نتركه مفعلاً كإيميل ولكن الحالة Pending
                EmailConfirmed = true,
                AccountStatus = "Pending" // تأكد أن لديك هذا الحقل في قاعدة البيانات، أو اعتمد على المنطق الافتراضي
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, SD.Student);

                // توجيه لصفحة الانتظار
                return RedirectToAction("PendingApproval", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> RegisterStudent(StudentRegisterViewModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return View(model);

        //    string fileName = null;

        //    // حفظ صورة البطاقة / الكارنيه
        //    if (model.StudentCardOrNationalImage != null)
        //    {
        //        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

        //        if (!Directory.Exists(uploadsFolder))
        //            Directory.CreateDirectory(uploadsFolder);

        //        fileName = Guid.NewGuid() + Path.GetExtension(model.StudentCardOrNationalImage.FileName);
        //        string filePath = Path.Combine(uploadsFolder, fileName);

        //        using (var fileStream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await model.StudentCardOrNationalImage.CopyToAsync(fileStream);
        //        }
        //    }

        //    // إنشاء الطالب
        //    var user = new ApplicationUser
        //    {
        //        UserName = model.Email,
        //        Email = model.Email,
        //        FullName = model.FullName,
        //        NationalId = model.NationalId,
        //        PhoneNumber = model.PhoneNumber,
        //        StudentCardImageORNationalUrl = "/images/" + fileName,

        //        // لو عندك نظام Approval
        //        // EmailConfirmed = false,
        //        // IsApproved = false
        //        EmailConfirmed = true
        //    };

        //    var result = await _userManager.CreateAsync(user, model.Password);

        //    if (result.Succeeded)
        //    {
        //        await _userManager.AddToRoleAsync(user, SD.Student);

        //        // لا تسجّل الدخول مباشرة
        //        return RedirectToAction("PendingApproval", "Home");
        //    }

        //    foreach (var error in result.Errors)
        //        ModelState.AddModelError("", error.Description);

        //    return View(model);
        //}

        // ----------------- LOGIN (GET) -----------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ----------------- LOGIN (POST) -----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            // لو عندك System Approval
            // if (!user.IsApproved)
            // {
            //     return RedirectToAction("PendingApproval", "Home");
            // }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, model.RememberMe, lockoutOnFailure: false
            );

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            // جلب بروفايل الطالب
            var studentProfile = await _context.StudentProfiles
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (studentProfile == null)
                return RedirectToAction("CreateForm", "Student");

            return RedirectToAction("MyProfile", "Student", new { slug = studentProfile.ProfileSlug });
        }

        // ----------------- LOGOUT -----------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
