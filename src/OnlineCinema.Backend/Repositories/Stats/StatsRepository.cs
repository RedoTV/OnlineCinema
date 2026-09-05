using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models.Enums;

namespace OnlineCinema.Backend.Repositories.Stats;

public class StatsRepository : IStatsRepository
{
    private readonly ApplicationDbContext _db;

    public StatsRepository(ApplicationDbContext db) => _db = db;

    public Task<List<TopRatedRow>> GetTopRatedAsync(int count, CancellationToken ct = default) =>
        _db.Movies
            .Include(m => m.Ratings)
            .Where(m => m.Ratings.Any())
            .OrderByDescending(m => m.Ratings.Average(r => r.RatingValue))
            .Take(count)
            .Select(m => new TopRatedRow(
                m.Id, m.Title,
                m.Ratings.Average(r => r.RatingValue),
                m.Ratings.Count))
            .ToListAsync(ct);

    public Task<List<MostWatchedRow>> GetMostWatchedAsync(int count, CancellationToken ct = default) =>
        _db.UserMovieStatuses
            .Where(s => s.Status == MovieStatus.Watched)
            .GroupBy(s => s.MovieId)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => new MostWatchedRow(
                g.Key,
                g.Count(),
                _db.Movies.Where(m => m.Id == g.Key).Select(m => m.Title).FirstOrDefault()))
            .ToListAsync(ct);

    public Task<List<GenreStatRow>> GetGenreStatsAsync(CancellationToken ct = default) =>
        _db.Genres
            .Select(g => new GenreStatRow(g.Name, g.Movies.Count))
            .OrderByDescending(x => x.MovieCount)
            .ToListAsync(ct);
}