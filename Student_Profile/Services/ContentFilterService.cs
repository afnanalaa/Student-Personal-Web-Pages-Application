using Microsoft.EntityFrameworkCore;
using Student_Profile.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Student_Profile.Services
{
    public class ContentFilterService
    {
        private readonly ApplicationDbContext _context;

        public ContentFilterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ContainsProhibitedContent(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            var bannedWords = await _context.BannedWords
                .Select(w => w.Word.ToLower())
                .ToListAsync();

            var lowerText = text.ToLower();

            return bannedWords.Any(word => lowerText.Contains(word));
        }
    }
}