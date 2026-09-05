using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Genres;

public class GenreRepository : IGenreRepository
{
    private readonly ApplicationDbContext _db;

    public GenreRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Genre>> GetAllAsync(CancellationToken ct = default) =>
        _db.Genres.AsNoTracking().ToListAsync(ct);

    public Task<Genre?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Genres.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, ct);

    public async Task<Genre?> FindAsync(int id, CancellationToken ct = default) =>
        await _db.Genres.FindAsync(new object[] { id }, ct);

    public void Add(Genre genre) => _db.Genres.Add(genre);

    public void Remove(Genre genre) => _db.Genres.Remove(genre);
}