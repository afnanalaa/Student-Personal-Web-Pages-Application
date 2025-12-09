using Student_Profile.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Student_Profile.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
: base(options)
        {
        }
        public DbSet<StudentProfile> StudentProfiles { get; set; }
        public DbSet<Post> Posts { get; set; }
    }
}
