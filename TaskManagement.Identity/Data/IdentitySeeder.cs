using System;
using TaskManagement.Identity.Models;
using TaskManagement.Identity.Models.Enums;

namespace TaskManagement.Identity.Data;

public static class IdentitySeeder
{
    // seed default admin bruger i development - kører kun hvis brugeren ikke eksisterer
    public static void SeedAdmin(IdentityDbContext context)
    {
        if (context.Users.Any(u => u.Email == "admin@admin.com"))
            return;
        
        context.Users.Add(new User
        {
            id = Guid.NewGuid(),
            Name = "Admin",
            Email = "admin@admin.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        });

        context.SaveChanges();
    }
}
