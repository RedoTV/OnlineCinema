using OnlineCinema.Backend.Services.Helpers;
using Xunit;

namespace OnlineCinema.Backend.Tests;

// Хэши паролей — единственная часть сервиса, тестируемая без БД и MinIO.
// Проверяем что соль реальная и детерминированный хэш совпадает.
public class PasswordHasherTests
{
    [Fact]
    public void Hash_Produces_Valid_Base64_And_Not_Empty()
    {
        var salt = PasswordHasher.GenerateSalt();
        var hash = PasswordHasher.HashPassword("SuperSecret123!", salt);

        Assert.False(string.IsNullOrEmpty(hash));
        // base64 round-trip
        var bytes = Convert.FromBase64String(hash);
        Assert.Equal(32, bytes.Length); // KeySize
    }

    [Fact]
    public void Same_Salt_Same_Password_Gives_Same_Hash()
    {
        var salt = PasswordHasher.GenerateSalt();
        var h1 = PasswordHasher.HashPassword("secret", salt);
        var h2 = PasswordHasher.HashPassword("secret", salt);

        Assert.Equal(h1, h2); // это и позволяет сверять при логине
    }

    [Fact]
    public void Different_Salts_Produce_Different_Hashes()
    {
        var h1 = PasswordHasher.HashPassword("secret", PasswordHasher.GenerateSalt());
        var h2 = PasswordHasher.HashPassword("secret", PasswordHasher.GenerateSalt());

        Assert.NotEqual(h1, h2);
    }
}
