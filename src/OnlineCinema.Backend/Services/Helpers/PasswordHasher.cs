using System.Security.Cryptography;
using System.Text;

namespace OnlineCinema.Backend.Services.Helpers;

public static class PasswordHasher
{
    // 600k итераций по OWASP для PBKDF2-HMAC-SHA256. Старые хэши (10k) останутся валидными,
    // но при следующем логине можно докрутить до нового формата.
    private const int Iterations = 600_000;
    private const int KeySize = 32;

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
        var bytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(salt),
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        return Convert.ToBase64String(bytes);
    }
}
