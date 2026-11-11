using BookingSystemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookingSystemAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedFirstAdminAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == RoleNames.Admin);
            if (adminRole == null)
            {
                logger.LogError("The Admin role was not found. Make sure the database migrations have been applied.");
                return;
            }

            bool adminExists = await context.Staff.AnyAsync(s => s.RoleId == adminRole.RoleId);
            if (adminExists)
            {
                return;
            }

            var section = configuration.GetSection("SeedAdmin");
            var email = section["Email"];
            var password = section["Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                logger.LogWarning("No Admin exists and SeedAdmin settings are missing or the password is shorter than 8 characters. No Admin was created.");
                return;
            }

            var admin = new Staff
            {
                FirstName = section["FirstName"] ?? "System",
                LastName = section["LastName"] ?? "Administrator",
                Email = email.Trim().ToLowerInvariant(),
                PhoneNumber = section["PhoneNumber"] ?? "0000000000",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RoleId = adminRole.RoleId,
                IsActive = true
            };

            context.Staff.Add(admin);
            await context.SaveChangesAsync();

            logger.LogInformation("The first Admin account was created for {Email}.", admin.Email);
        }
    }
}