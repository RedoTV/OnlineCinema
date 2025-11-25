using System.Security.Cryptography;
using System.Text;

namespace OnlineCinema.Backend.Services.Helpers;

public static class PasswordHasher
{
    public static string GenerateSalt()
    {
        var buffer = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }
        return Convert.ToBase64String(buffer);
    }

    public static string HashPassword(string password, string salt)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(salt), 10000, HashAlgorithmName.SHA256))
        {
            var hash = pbkdf2.GetBytes(20);
            return Convert.ToBase64String(hash);
        }
    }
}