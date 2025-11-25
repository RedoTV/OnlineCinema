using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Services.Helpers;

namespace OnlineCinema.Backend.Data;

public static class DataSeeder
{
    public static void SeedAdminUsers(ModelBuilder modelBuilder)
    {
        var admins = new[]
        {
            new { Id = 1, Username = "admin1", Email = "admin1@onlinecinema.com", Password = "AdminPassword123!", FirstName = "Admin", LastName = "One" },
            new { Id = 2, Username = "admin2", Email = "admin2@onlinecinema.com", Password = "AdminPassword123!", FirstName = "Admin", LastName = "Two" },
            new { Id = 3, Username = "admin3", Email = "admin3@onlinecinema.com", Password = "AdminPassword123!", FirstName = "Admin", LastName = "Three" }
        };

        var users = new List<User>();

        foreach (var admin in admins)
        {
            var salt = PasswordHasher.GenerateSalt();
            var hashedPassword = PasswordHasher.HashPassword(admin.Password, salt);

            users.Add(new User
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                Role = "Admin",
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                CreatedAt = new DateTime(2025, 11, 22, 20, 22, 53, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 11, 22, 20, 22, 53, DateTimeKind.Utc)
            });
        }

        modelBuilder.Entity<User>().HasData(users);
    }
}
