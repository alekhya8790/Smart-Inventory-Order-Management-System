using Microsoft.EntityFrameworkCore;
using SmartIOMS.Models;

namespace SmartIOMS.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(SmartIOMSDbContext context)
        {
            // Create Admin user if it does not already exist
            bool adminExists = await context.Users
                .AnyAsync(x => x.Email == "admin@smartioms.com");

            if (!adminExists)
            {
                var admin = new User
                {
                    FullName = "SmartIOMS Admin",
                    Email = "admin@smartioms.com",
                    PhoneNumber = "9876543210",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(admin);

                await context.SaveChangesAsync();
            }
        }
    }
}