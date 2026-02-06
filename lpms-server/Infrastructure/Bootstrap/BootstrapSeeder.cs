using LegalCaseManagement.Data;
using LegalCaseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LegalCaseManagement.Infrastructure.Bootstrap
{
    public static class BootstrapSeeder
    {
        public static async Task SeedAsync(LegalCaseDbContext db)
        {
            if (!await db.Roles.AnyAsync())
            {
                db.Roles.AddRange(
                    new Role { Name = "Admin", Description = "System administrator" },
                    new Role { Name = "LegalOfficer", Description = "Legal officer" },
                    new Role { Name = "HeadOfLegal", Description = "Head of legal" },
                    new Role { Name = "ExecutiveManagement", Description = "Executive management" },
                    new Role { Name = "Board", Description = "Board" }
                );
                await db.SaveChangesAsync();
            }

            if (!await db.Users.AnyAsync())
            {
                var user = new User
                {
                    FirstName = "Dev",
                    LastName = "Admin",
                    Email = "admin@local.dev",
                    Department = "IT",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                var adminRoleId = await db.Roles.Where(r => r.Name == "Admin").Select(r => r.RoleId).FirstAsync();
                db.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = adminRoleId, IsActive = true, AssignedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
            }
        }
    }
}
