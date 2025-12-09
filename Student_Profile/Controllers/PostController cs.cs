using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Profile.Data;
using Student_Profile.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Student_Profile.ViewModels;

namespace Student_Profile.Controllers
{
    [Authorize(Roles = "Student")]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض كل بوستات الطالب
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
        public async Task<IActionResult> CreatePost(PostViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Student/Post/CreatePost.cshtml", model);


            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var post = new Post
            {
                UserId = userId,
                Content = model.Content,
                Status = "Pending", // 🔹 كل بوست جديد Pending
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

            var model = new PostViewModel { Content = post.Content };
            return View("~/Views/Student/Post/EditPost.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(int id, PostViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Student/Post/EditPost.cshtml", model);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null)
                return NotFound();

            post.Content = model.Content;
            post.Status = "Pending"; 
            await _context.SaveChangesAsync();

            return RedirectToAction("MyPosts");
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null)
                return NotFound();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyPosts");
        }
    }
}
