using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.UserActions;
using OnlineCinema.Backend.Models.Enums;

namespace OnlineCinema.Backend.Repositories.UserActions;

public class UserActionRepository : IUserActionRepository
{
    private readonly ApplicationDbContext _db;

    public UserActionRepository(ApplicationDbContext db) => _db = db;

    public Task<UserMovieStatus?> FindStatusAsync(int userId, int movieId, CancellationToken ct = default) =>
        _db.UserMovieStatuses.FirstOrDefaultAsync(s => s.UserId == userId && s.MovieId == movieId, ct);

    public void AddStatus(UserMovieStatus status) => _db.UserMovieStatuses.Add(status);

    public Task<Rating?> FindRatingAsync(int userId, int? movieId, int? seriesId, CancellationToken ct = default) =>
        _db.Ratings.FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId && r.SeriesId == seriesId, ct);

    public void AddRating(Rating rating) => _db.Ratings.Add(rating);

    public void RemoveRating(Rating rating) => _db.Ratings.Remove(rating);

    public async Task<List<UserMovieStatus>> GetUserMoviesAsync(int userId, string? status, CancellationToken ct = default)
    {
        var query = _db.UserMovieStatuses
            .Include(s => s.Movie)
            .Where(s => s.UserId == userId)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MovieStatus>(status, true, out var statusEnum))
            query = query.Where(s => s.Status == statusEnum);

        return await query.ToListAsync(ct);
    }
}