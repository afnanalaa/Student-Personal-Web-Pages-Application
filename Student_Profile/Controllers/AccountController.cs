using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Student_Profile.Models;
using Student_Profile.Utility;
using Student_Profile.ViewModels;

namespace Student_Profile.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 RoleManager<IdentityRole> roleManager,
                                 SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult RegisterStudent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStudent(StudentRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                string fileName = null;

                // 1. حفظ ملف الإثبات على wwwroot/images
                if (model.StudentCardOrNationalImage != null)
                {
                    // يفضل استخدام Path.Combine مع WebRootPath بدلاً من Directory.GetCurrentDirectory() لبيئة ASP.NET Core
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    fileName = Guid.NewGuid() + Path.GetExtension(model.StudentCardOrNationalImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.StudentCardOrNationalImage.CopyToAsync(fileStream);
                    }
                }

                // 2. إنشاء المستخدم وتعيين بياناته
                // (يجب أن يحتوي نموذج ApplicationUser على خاصية لحالة الموافقة مثل IsApproved = false)
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    NationalId = model.NationalId,
                    PhoneNumber = model.PhoneNumber,
                    StudentCardImageORNationalUrl = "/images/" + fileName,

                    // ************ التعديل الأهم لتطبيق الموافقة ************
                    // يجب تعيين هذا الحقل false/Pending في النموذج
                    EmailConfirmed = true // (يجب أن يتم تعديل هذا الحقل في قاعدة البيانات لاحقاً ليعكس Pending)
                                          // إذا كان نموذج ApplicationUser يحتوي على خاصية IsApproved/Status، يجب تعيينها هنا إلى false/Pending
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // 3. إضافة الطالب إلى رول Student
                    await _userManager.AddToRoleAsync(user, SD.Student);

                    // 4. إلغاء تسجيل الدخول المباشر
                    // *** تم حذف السطر: await _signInManager.SignInAsync(user, isPersistent: false); ***

                    // 5. ************ الخطوة الحاسمة: التوجيه لصفحة انتظار الموافقة ************
                    return RedirectToAction("PendingApproval", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(model);
        }

        // Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
