using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data;

public static class DataSeeder
{
    public static void SeedAdminUsers(ModelBuilder modelBuilder)
    {
        var admins = new[]
        {
            new { Id = -1, Username = "admin1", Email = "admin1@onlinecinema.com", Password = "AdminPassword123!", FirstName = "Admin", LastName = "One" },
            new { Id = -2, Username = "admin2", Email = "admin2@onlinecinema.com", Password = "AdminPassword123!", FirstName = "Admin", LastName = "Two" },
            new { Id = -3, Username = "admin3", Email = "admin3@onlinecinema.com", Password = "AdminPassword123!", FirstName = "Admin", LastName = "Three" }
        };

        var users = new List<User>();

        foreach (var admin in admins)
        {
            var salt = GenerateSalt();
            var hashedPassword = HashPassword(admin.Password, salt);

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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        modelBuilder.Entity<User>().HasData(users);
    }

    private static string GenerateSalt()
    {
        var buffer = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }
        return Convert.ToBase64String(buffer);
    }

    private static string HashPassword(string password, string salt)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256))
        {
            var hash = pbkdf2.GetBytes(20);
            return Convert.ToBase64String(hash);
        }
    }
}