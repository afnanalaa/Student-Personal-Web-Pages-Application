using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;
using Student_Profile.Models;

namespace Student_Profile.Controllers
{
    [Authorize(Roles = "Admin")] 
    public class BannedWordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BannedWordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var words = await _context.BannedWords.ToListAsync();
            return View(words);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                TempData["Error"] = "Word cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            var exists = await _context.BannedWords.AnyAsync(w => w.Word.ToLower() == word.ToLower());
            if (exists)
            {
                TempData["Error"] = "This word is already in the list.";
                return RedirectToAction(nameof(Index));
            }

            _context.BannedWords.Add(new BannedWord { Word = word.Trim() });
            await _context.SaveChangesAsync();

            TempData["Success"] = "Word added successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var word = await _context.BannedWords.FindAsync(id);
            if (word != null)
            {
                _context.BannedWords.Remove(word);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Word removed.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}