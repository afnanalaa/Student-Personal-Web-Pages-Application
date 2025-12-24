using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;
using Student_Profile.Models;
using System.Diagnostics;

namespace Student_Profile.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PendingApproval()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        public async Task<IActionResult> StudentsDirectory(string search)
        {
            var query = _context.StudentProfiles
                .Include(p => p.User)
                .Where(p => p.User.AccountStatus == "Approved") 
                .AsQueryable();

            if (!User.Identity.IsAuthenticated)
            {
                query = query.Where(p => p.PrivacyMode == "Public");
            }
            else
            {
                query = query.Where(p => p.PrivacyMode != "Private");
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.User.FullName.Contains(search) ||
                                         p.Department.Contains(search));
            }

            var students = await query.ToListAsync();
            return View(students);
        }

        [HttpGet]
        public async Task<IActionResult> LiveSearch(string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return Content("");

            var students = await _context.StudentProfiles
                .Include(p => p.User)
                .Where(p => p.User.AccountStatus == "Approved" &&
                           (p.User.FullName.Contains(search) || p.Department.Contains(search)))
                .Take(5) 
                .ToListAsync();

            return PartialView("StudentSearchResults", students);
        }
    }
}