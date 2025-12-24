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
                ModelState.AddModelError("", "You must provide text or upload a file (Image/PDF) to publish.");
            }


            if (!ModelState.IsValid)
                return View("~/Views/Student/Post/CreatePost.cshtml", model);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string fileUrl = null;

            if (model.ImageFile != null)
            {

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf" };
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();


                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Sorry, only images (JPG, PNG) or PDF files are allowed for security reasons.");
                    return View("~/Views/Student/Post/CreatePost.cshtml", model);
                }


                if (model.ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "The file size is too large. The maximum allowed size is 5MB.");
                    return View("~/Views/Student/Post/CreatePost.cshtml", model);
                }


                string wwwRootPath = _hostEnvironment.WebRootPath;
                string uploadPath = Path.Combine(wwwRootPath, "images", "posts");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                string fileName = Guid.NewGuid().ToString();

                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName + extension), FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }


                fileUrl = Path.Combine("/images/posts", fileName + extension).Replace('\\', '/');
            }


            var post = new Post
            {
                UserId = userId,
                Content = model.Content,
                ImageFile = fileUrl,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your post has been created successfully and is pending approval.";
            return RedirectToAction("MyPosts");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, PostViewModel model)
        {


            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null) return NotFound();


            if (string.IsNullOrWhiteSpace(model.Content) && model.ImageFile == null && string.IsNullOrEmpty(post.ImageFile))
            {
                ModelState.AddModelError("", "The post cannot be completely empty.");
                return View("~/Views/Student/Post/EditPost.cshtml", model);
            }


            if (model.ImageFile != null)
            {

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".pdf" };
                var extension = Path.GetExtension(model.ImageFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageFile", "Invalid file type. Only images and PDF files are allowed.");
                    model.Content = post.Content;
                    return View("~/Views/Student/Post/EditPost.cshtml", model);
                }


                string wwwRootPath = _hostEnvironment.WebRootPath;


                if (!string.IsNullOrEmpty(post.ImageFile))
                {
                    string oldFilePath = Path.Combine(wwwRootPath, post.ImageFile.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }


                string uploadPath = Path.Combine(wwwRootPath, "images", "posts");
                string fileName = Guid.NewGuid().ToString();

                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName + extension), FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }

                post.ImageFile = Path.Combine("/images/posts", fileName + extension).Replace('\\', '/');
            }


            post.Content = model.Content;
            post.Status = "Pending";

            await _context.SaveChangesAsync();

            TempData["Success"] = "Your post has been updated successfully and is pending approval..";
            return RedirectToAction("MyPosts");
        }

        [HttpPost]
        public async Task<IActionResult> ReportPost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();

            post.IsReported = true;
            post.ReportsCount += 1;

            await _context.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString());
        }



        [HttpGet]
        public async Task<IActionResult> EditPost(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null)
                return NotFound();

            var model = new PostViewModel { Content = post.Content, };
            return View("~/Views/Student/Post/EditPost.cshtml", model);
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