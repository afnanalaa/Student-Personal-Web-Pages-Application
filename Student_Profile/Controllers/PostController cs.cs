using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;
using Student_Profile.Models;
using Student_Profile.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Student_Profile.Controllers
{
    [Authorize(Roles = "Student")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public PostController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }


        public async Task<IActionResult> MyPosts()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View("~/Views/Student/Post/MyPosts.cshtml", posts);
        }

        [HttpGet]
        public IActionResult CreatePost()
        {
            return View("~/Views/Student/Post/CreatePost.cshtml");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(PostViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Content) && model.ImageFile == null)
            {
                ModelState.AddModelError("", "Post must contain either text content or an image.");
            }

            if (!ModelState.IsValid)
                return View("~/Views/Student/Post/CreatePost.cshtml", model);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string imageUrl = null;

            if (model.ImageFile != null)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string uploadPath = Path.Combine(wwwRootPath, "images", "posts");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

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
                Status = "Pending", // 🔹 كل منشور جديد يكون في حالة الانتظار
                CreatedAt = DateTime.Now
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyPosts");
        }


        [HttpGet]
        public async Task<IActionResult> EditPost(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null)
                return NotFound();

            // 🔹 لا نقوم بتحميل الصورة إلى الـ ViewModel في الـ GET
            var model = new PostViewModel { Content = post.Content, };
            return View("~/Views/Student/Post/EditPost.cshtml", model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, PostViewModel model)
        {
            // 🔹 التحقق المخصص
            if (model.ImageFile == null && string.IsNullOrWhiteSpace(model.Content))
            {
                ModelState.AddModelError("", "Post must contain either text content or a new image.");
            }

            if (!ModelState.IsValid)
                return View("~/Views/Student/Post/EditPost.cshtml", model);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null)
                return NotFound();

            string wwwRootPath = _hostEnvironment.WebRootPath;

            // 1. معالجة الصورة الجديدة
            if (model.ImageFile != null)
            {
                // 1.1. حذف الصورة القديمة إذا كانت موجودة
                if (!string.IsNullOrEmpty(post.ImageFile))
                {
                    string oldFilePath = Path.Combine(wwwRootPath, post.ImageFile.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // 1.2. حفظ الصورة الجديدة
                string uploadPath = Path.Combine(wwwRootPath, "images", "posts");
                string fileName = Guid.NewGuid().ToString();
                string extension = Path.GetExtension(model.ImageFile.FileName);

                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName + extension), FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                // تحديث المسار
                post.ImageFile = Path.Combine("/images/posts", fileName + extension).Replace('\\', '/');
            }

            post.Content = model.Content;
            post.Status = "Pending";

            await _context.SaveChangesAsync();

            return RedirectToAction("MyPosts");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null)
                return NotFound();

            if (!string.IsNullOrEmpty(post.ImageFile))
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string filePathToDelete = Path.Combine(wwwRootPath, post.ImageFile.TrimStart('/'));

                if (System.IO.File.Exists(filePathToDelete))
                {
                    System.IO.File.Delete(filePathToDelete);
                }
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyPosts");
        }
    }
}