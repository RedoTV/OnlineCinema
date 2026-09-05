using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Users;

public interface IUserRepository
{
    Task<User?> FindByEmailOrUsernameAsync(string email, string username, CancellationToken ct = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    void Add(User user);
}