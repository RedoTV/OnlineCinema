using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Genres;

public interface IGenreRepository
{
    Task<List<Genre>> GetAllAsync(CancellationToken ct = default);
    Task<Genre?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Genre?> FindAsync(int id, CancellationToken ct = default);
    void Add(Genre genre);
    void Remove(Genre genre);
}