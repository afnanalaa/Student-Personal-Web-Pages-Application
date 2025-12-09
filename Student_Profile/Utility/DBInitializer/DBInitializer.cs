using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Student_Profile.Data;
using Student_Profile.Models;

namespace Student_Profile.Utility.DBInitializer
{
    public class DBInitializer :IDBInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DBInitializer(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public void Initilize()
        {
            try
            {
                // 1️⃣ Apply pending migrations
                if (_context.Database.GetPendingMigrations().Any())
                {
                    Console.WriteLine("Applying migrations...");
                    _context.Database.Migrate();
                    Console.WriteLine("Migrations applied.");
                }

                // 2️⃣ Create roles if not exist
                string[] roles = new[] { SD.SuperAdmin, SD.Admin, SD.Student };
                foreach (var roleName in roles)
                {
                    if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                    {
                        var roleResult = _roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
                        if (!roleResult.Succeeded)
                        {
                            Console.WriteLine($"Error creating role {roleName}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                        }
                        else
                        {
                            Console.WriteLine($"Role {roleName} created successfully.");
                        }
                    }
                }

                // 3️⃣ Create SuperAdmin user
                var superAdminEmail = "superadmin@gmail.com";
                var superAdmin = _userManager.FindByEmailAsync(superAdminEmail).GetAwaiter().GetResult();

                if (superAdmin == null)
                {
                    superAdmin = new ApplicationUser
                    {
                        UserName = "SuperAdmin",
                        Email = superAdminEmail,
                        EmailConfirmed = true,
                        FullName = "University Super Admin",
                        NationalId = "0000000001",
                        StudentCardImageORNationalUrl = "N/A" // أو رابط صورة لو عندك
                    };

                    var result = _userManager.CreateAsync(superAdmin, "SuperAdmin123$").GetAwaiter().GetResult();
                    if (!result.Succeeded)
                    {
                        Console.WriteLine("Error creating SuperAdmin:");
                        foreach (var error in result.Errors)
                            Console.WriteLine(error.Description);
                    }
                    else
                    {
                        Console.WriteLine("SuperAdmin user created successfully.");
                    }
                }

                // Add SuperAdmin to role
                if (!_userManager.IsInRoleAsync(superAdmin, SD.SuperAdmin).GetAwaiter().GetResult())
                {
                    _userManager.AddToRoleAsync(superAdmin, SD.SuperAdmin).GetAwaiter().GetResult();
                    Console.WriteLine("SuperAdmin added to role SuperAdmin.");
                }

                // 4️⃣ Create Admin user
                var adminEmail = "admin@gmail.com";
                var adminUser = _userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = "Admin",
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FullName = "University Admin",
                        NationalId = "0000000002",
                        StudentCardImageORNationalUrl = "N/A"
                    };

                    var adminResult = _userManager.CreateAsync(adminUser, "Admin123$").GetAwaiter().GetResult();
                    if (!adminResult.Succeeded)
                    {
                        Console.WriteLine("Error creating Admin:");
                        foreach (var error in adminResult.Errors)
                            Console.WriteLine(error.Description);
                    }
                    else
                    {
                        Console.WriteLine("Admin user created successfully.");
                    }
                }

                // Add Admin to role
                if (!_userManager.IsInRoleAsync(adminUser, SD.Admin).GetAwaiter().GetResult())
                {
                    _userManager.AddToRoleAsync(adminUser, SD.Admin).GetAwaiter().GetResult();
                    Console.WriteLine("Admin added to role Admin.");
                }

                Console.WriteLine("DB Initialization completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DBInitializer Error: {ex.Message}");
            }
        }
    }
}
