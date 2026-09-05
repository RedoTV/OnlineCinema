using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Series;

namespace OnlineCinema.Backend.Repositories.Series;

public class SeriesRepository : ISeriesRepository
{
    private readonly ApplicationDbContext _db;

    public SeriesRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<SeriesSummaryDto>> GetSummariesAsync(string? search, int? genreId, CancellationToken ct = default)
    {
        var query = _db.Series.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Title.ToLower().Contains(search.ToLower()));

        if (genreId.HasValue)
            query = query.Where(s => s.Genres.Any(g => g.Id == genreId));

        return await query
            .OrderBy(s => s.Id)
            .Select(s => new SeriesSummaryDto
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                ReleaseYear = s.ReleaseYear,
                PosterUrl = s.PosterUrl,
                AverageRating = s.Ratings.Any() ? s.Ratings.Average(r => r.RatingValue) : 0,
                SeasonsCount = s.Seasons.Count,
                EpisodesCount = s.Seasons.SelectMany(se => se.Episodes).Count(),
                HasVideo = s.Seasons.SelectMany(se => se.Episodes).Any(ep => ep.VideoUrl != null && ep.VideoUrl != "")
            })
            .ToListAsync(ct);
    }

    public Task<Models.Series?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Series
            .Include(s => s.Seasons).ThenInclude(se => se.Episodes)
            .Include(s => s.Genres)
            .Include(s => s.Actors)
            .Include(s => s.Ratings)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Models.Series?> FindAsync(int id, CancellationToken ct = default) =>
        await _db.Series.FindAsync(new object[] { id }, ct);

    public void Add(Models.Series series) => _db.Series.Add(series);

    public void Remove(Models.Series series) => _db.Series.Remove(series);

    public async Task<Season?> FindSeasonAsync(int seasonId, CancellationToken ct = default) =>
        await _db.Seasons.FindAsync(new object[] { seasonId }, ct);

    public void AddSeason(Season season) => _db.Seasons.Add(season);

    public Task<List<Genre>> GetGenresByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default) =>
        _db.Genres.Where(g => ids.Contains(g.Id)).ToListAsync(ct);

    public Task<List<Actor>> GetActorsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default) =>
        _db.Actors.Where(a => ids.Contains(a.Id)).ToListAsync(ct);

    public void AddEpisode(Episode episode) => _db.Episodes.Add(episode);

    public async Task<Episode?> FindEpisodeAsync(int episodeId, CancellationToken ct = default) =>
        await _db.Episodes.FindAsync(new object[] { episodeId }, ct);

    public Task<Models.Series?> GetWithEpisodesAsync(int id, CancellationToken ct = default) =>
        _db.Series
            .Include(s => s.Seasons).ThenInclude(se => se.Episodes)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
}