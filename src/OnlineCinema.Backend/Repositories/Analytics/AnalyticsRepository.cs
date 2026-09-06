using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models.Enums;

namespace OnlineCinema.Backend.Repositories.Analytics;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ApplicationDbContext _db;

    public AnalyticsRepository(ApplicationDbContext db) => _db = db;

    public async Task<OverviewRow> GetOverviewAsync(CancellationToken ct = default)
    {
        var movies = await _db.Movies.CountAsync(ct);
        var series = await _db.Series.CountAsync(ct);
        var activeUsers = await _db.Ratings.Select(r => r.UserId).Distinct().CountAsync(ct);
        var watches = await _db.UserMovieStatuses
            .Where(s => s.Status == MovieStatus.Watched)
            .Select(s => s.UserId)
            .Distinct()
            .CountAsync(ct);
        var ratings = await _db.Ratings.CountAsync(ct);
        return new OverviewRow(movies, series, activeUsers, watches, ratings);
    }

    public async Task<List<TrendingRow>> GetTrendingAsync(TimeSpan window, int count, CancellationToken ct = default)
    {
        var since = DateTime.UtcNow - window;

        var ratingRows = await _db.Ratings
            .Where(r => r.CreatedAt >= since)
            .Select(r => new { r.MovieId, r.SeriesId, r.RatingValue })
            .ToListAsync(ct);

        var movieWatches = await _db.UserMovieStatuses
            .Where(s => s.Status == MovieStatus.Watched && s.AddedAt >= since)
            .GroupBy(s => s.MovieId)
            .Select(g => new { MovieId = g.Key, Views = g.Count() })
            .ToListAsync(ct);

        var episodeWatches = await _db.PlaybackProgresses
            .Where(p => p.EpisodeId != null && p.UpdatedAt >= since)
            .GroupBy(p => p.Episode!.Season.SeriesId)
            .Select(g => new { SeriesId = g.Key, Views = g.Count() })
            .ToListAsync(ct);

        var rows = new List<TrendingRow>();

        foreach (var watched in movieWatches)
        {
            var rs = ratingRows.Where(r => r.MovieId == watched.MovieId).ToList();
            var title = await _db.Movies.Where(m => m.Id == watched.MovieId)
                .Select(m => m.Title).FirstOrDefaultAsync(ct) ?? $"Материал {watched.MovieId}";
            rows.Add(new TrendingRow("movie", watched.MovieId, title, watched.Views, 0,
                rs.Count, rs.Count > 0 ? rs.Average(r => (double)r.RatingValue) : null));
        }

        foreach (var eps in episodeWatches)
        {
            var seriesRatings = ratingRows.Where(r => r.SeriesId == eps.SeriesId).ToList();
            var title = await _db.Series.Where(s => s.Id == eps.SeriesId)
                .Select(s => s.Title).FirstOrDefaultAsync(ct) ?? $"Сериал {eps.SeriesId}";
            rows.Add(new TrendingRow("series", eps.SeriesId, title, eps.Views, 0,
                seriesRatings.Count,
                seriesRatings.Count > 0 ? seriesRatings.Average(r => (double)r.RatingValue) : null));
        }

        foreach (var movieId in ratingRows.Where(r => r.MovieId != null).Select(r => r.MovieId!.Value)
                     .Except(movieWatches.Select(w => w.MovieId)).Distinct())
        {
            var mr = ratingRows.Where(r => r.MovieId == movieId).ToList();
            var title = await _db.Movies.Where(m => m.Id == movieId)
                .Select(m => m.Title).FirstOrDefaultAsync(ct) ?? $"Материал {movieId}";
            rows.Add(new TrendingRow("movie", movieId, title, 0, 0, mr.Count,
                mr.Average(r => (double)r.RatingValue)));
        }

        foreach (var seriesId in ratingRows.Where(r => r.SeriesId != null).Select(r => r.SeriesId!.Value)
                     .Except(episodeWatches.Select(w => w.SeriesId)).Distinct())
        {
            var sr = ratingRows.Where(r => r.SeriesId == seriesId).ToList();
            var title = await _db.Series.Where(s => s.Id == seriesId)
                .Select(s => s.Title).FirstOrDefaultAsync(ct) ?? $"Сериал {seriesId}";
            rows.Add(new TrendingRow("series", seriesId, title, 0, 0, sr.Count,
                sr.Average(r => (double)r.RatingValue)));
        }

        return rows
            .OrderByDescending(r => r.Views + r.RatingCount)
            .ThenByDescending(r => r.AvgRating ?? 0)
            .Take(count)
            .ToList();
    }

    public async Task<List<ActivityRow>> GetActivityAsync(int? userId, int limit, CancellationToken ct = default)
    {
        var rated = userId.HasValue
            ? await _db.Ratings.Where(r => r.UserId == userId.Value)
                .Select(r => new { r.User!.Username, r.CreatedAt, r.MovieId, r.SeriesId, r.RatingValue })
                .ToListAsync(ct)
            : await _db.Ratings
                .Select(r => new { r.User!.Username, r.CreatedAt, r.MovieId, r.SeriesId, r.RatingValue })
                .ToListAsync(ct);

        var watched = userId.HasValue
            ? await _db.UserMovieStatuses
                .Where(s => s.UserId == userId.Value && s.Status == MovieStatus.Watched)
                .Select(s => new { s.User!.Username, s.AddedAt, s.MovieId })
                .ToListAsync(ct)
            : await _db.UserMovieStatuses
                .Where(s => s.Status == MovieStatus.Watched)
                .Select(s => new { s.User!.Username, s.AddedAt, s.MovieId })
                .ToListAsync(ct);

        var comments = userId.HasValue
            ? await _db.Comments
                .Where(c => c.UserId == userId.Value && !c.IsHidden)
                .Select(c => new { c.User!.Username, c.CreatedAt, c.MovieId, c.SeriesId })
                .ToListAsync(ct)
            : await _db.Comments
                .Where(c => !c.IsHidden)
                .Select(c => new { c.User!.Username, c.CreatedAt, c.MovieId, c.SeriesId })
                .ToListAsync(ct);

        var activities = new List<ActivityRow>();

        foreach (var r in rated)
        {
            var type = r.MovieId.HasValue ? "movie" : "series";
            var id = r.MovieId ?? r.SeriesId ?? 0;
            var title = await ResolveTitleAsync(id, type, ct);
            activities.Add(new ActivityRow(r.Username, "rated", type, id, title,
                $"оценил в {r.RatingValue}/10", r.CreatedAt));
        }

        foreach (var s in watched)
        {
            var title = await _db.Movies.Where(m => m.Id == s.MovieId)
                .Select(m => m.Title).FirstOrDefaultAsync(ct);
            activities.Add(new ActivityRow(s.Username, "watched", "movie", s.MovieId, title, null, s.AddedAt));
        }

        foreach (var c in comments)
        {
            var type = c.MovieId.HasValue ? "movie" : "series";
            var id = c.MovieId ?? c.SeriesId ?? 0;
            var title = await ResolveTitleAsync(id, type, ct);
            activities.Add(new ActivityRow(c.Username, "comment", type, id, title, null, c.CreatedAt));
        }

        return activities
            .Where(a => a.Actor != null)
            .OrderByDescending(a => a.HappenedAt)
            .Take(limit)
            .ToList();
    }

    private async Task<string?> ResolveTitleAsync(int id, string type, CancellationToken ct) =>
        type == "movie"
            ? await _db.Movies.Where(m => m.Id == id).Select(m => m.Title).FirstOrDefaultAsync(ct)
            : await _db.Series.Where(s => s.Id == id).Select(s => s.Title).FirstOrDefaultAsync(ct);

    public async Task<List<PickRow>> GetPicksAsync(int userId, int count, CancellationToken ct = default)
    {
        var excluded = (await _db.UserMovieStatuses.Where(s => s.UserId == userId)
                .Select(s => s.MovieId).Distinct().ToListAsync(ct))
            .Concat(await _db.Ratings.Where(r => r.UserId == userId && r.MovieId != null)
                .Select(r => r.MovieId!.Value).Distinct().ToListAsync(ct))
            .ToHashSet();

        var lovedGenreIds = await _db.Ratings
            .Where(r => r.UserId == userId && r.MovieId != null && r.RatingValue >= 7)
            .SelectMany(r => r.Movie!.Genres)
            .Select(g => g.Id)
            .Distinct()
            .ToListAsync(ct);

        var candidates = await _db.Movies
            .Where(m => !excluded.Contains(m.Id) && m.Genres.Any(g => lovedGenreIds.Contains(g.Id)))
            .Select(m => new { m.Id, m.Title, Avg = (double?)m.Ratings.Average(r => r.RatingValue), m.ReleaseYear })
            .OrderByDescending(m => m.Avg)
            .ThenByDescending(m => m.ReleaseYear)
            .ThenByDescending(m => m.Id)
            .Take(count)
            .ToListAsync(ct);

        var result = candidates
            .Select(m => new PickRow(m.Id, m.Title, m.Avg, "по вашим любимым жанрам"))
            .ToList();

        if (result.Count < count)
        {
            var used = result.Select(p => p.MovieId).ToHashSet();
            used.UnionWith(excluded);
            var fillers = await _db.Movies
                .Where(m => !used.Contains(m.Id))
                .Select(m => new { m.Id, m.Title, Avg = (double?)m.Ratings.Average(r => r.RatingValue), m.ReleaseYear })
                .OrderByDescending(m => m.ReleaseYear)
                .ThenByDescending(m => m.Id)
                .Take(count - result.Count)
                .ToListAsync(ct);
            result.AddRange(fillers.Select(m => new PickRow(m.Id, m.Title, m.Avg, "новинка каталога")));
        }

        return result;
    }
}
