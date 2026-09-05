using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Movies;

namespace OnlineCinema.Backend.Repositories.Movies;

public class MovieRepository : IMovieRepository
{
    private readonly ApplicationDbContext _db;

    public MovieRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<MovieSummaryDto>> GetSummariesAsync(string? search, int? genreId, CancellationToken ct = default)
    {
        var query = _db.Movies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Title.ToLower().Contains(search.ToLower()));

        if (genreId.HasValue)
            query = query.Where(m => m.Genres.Any(g => g.Id == genreId));

        return await query
            .OrderBy(m => m.Id)
            .Select(m => new MovieSummaryDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                Duration = m.Duration,
                PosterUrl = m.PosterUrl,
                AverageRating = m.Ratings.Any() ? m.Ratings.Average(r => r.RatingValue) : 0,
                HasVideo = !string.IsNullOrEmpty(m.VideoUrl)
            })
            .ToListAsync(ct);
    }

    public Task<Movie?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Movies
            .Include(m => m.Genres)
            .Include(m => m.Actors)
            .Include(m => m.Ratings)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<Movie?> FindAsync(int id, CancellationToken ct = default) =>
        await _db.Movies.FindAsync(new object[] { id }, ct);

    public void Add(Movie movie) => _db.Movies.Add(movie);

    public void Remove(Movie movie) => _db.Movies.Remove(movie);

    public Task<List<Genre>> GetGenresByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default) =>
        _db.Genres.Where(g => ids.Contains(g.Id)).ToListAsync(ct);

    public Task<List<Actor>> GetActorsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default) =>
        _db.Actors.Where(a => ids.Contains(a.Id)).ToListAsync(ct);
}