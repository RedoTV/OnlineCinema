using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;

namespace OnlineCinema.Backend.Repositories.Admin;

public class AdminRepository : IAdminRepository
{
    private readonly ApplicationDbContext _db;

    public AdminRepository(ApplicationDbContext db) => _db = db;

    public async Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var ratings = await _db.Ratings.AsNoTracking().Select(x => x.RatingValue).ToListAsync(ct);
        return new AdminDashboardDto(
            await _db.Movies.CountAsync(ct),
            await _db.Series.CountAsync(ct),
            await _db.Actors.CountAsync(ct),
            await _db.Genres.CountAsync(ct),
            await _db.Users.CountAsync(ct),
            await _db.Comments.CountAsync(ct),
            await _db.Comments.CountAsync(x => x.IsHidden, ct),
            ratings.Count,
            ratings.Count == 0 ? 0 : Math.Round(ratings.Average(), 2));
    }

    public Task<List<AdminCommentRow>> GetCommentsAsync(CancellationToken ct = default) =>
        _db.Comments.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Movie)
            .Include(x => x.Series)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AdminCommentRow(
                x.Id, x.Text, x.IsHidden, x.CreatedAt, x.ParentId, x.User.Username,
                x.Movie != null ? x.Movie.Title : x.Series != null ? x.Series.Title : "—"))
            .ToListAsync(ct);

    public Task<List<AdminRatingRow>> GetRatingsAsync(CancellationToken ct = default) =>
        _db.Ratings.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Movie)
            .Include(x => x.Series)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new AdminRatingRow(
                x.Id, x.RatingValue, x.CreatedAt, x.User.Username,
                x.Movie != null ? x.Movie.Title : x.Series != null ? x.Series.Title : "—",
                x.MovieId != null ? "Фильм" : "Сериал"))
            .ToListAsync(ct);
}