using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Playback;

public class PlaybackRepository : IPlaybackRepository
{
    private readonly ApplicationDbContext _db;

    public PlaybackRepository(ApplicationDbContext db) => _db = db;

    public Task<PlaybackProgress?> FindAsync(int userId, int? movieId, int? episodeId, CancellationToken ct = default) =>
        _db.PlaybackProgresses.FirstOrDefaultAsync(p => p.UserId == userId
            && (movieId.HasValue ? p.MovieId == movieId : p.MovieId == null)
            && (episodeId.HasValue ? p.EpisodeId == episodeId : p.EpisodeId == null), ct);

    public void Add(PlaybackProgress progress) => _db.PlaybackProgresses.Add(progress);

    public Task<List<PlaybackProgress>> GetContinueWatchingAsync(int userId, CancellationToken ct = default) =>
#pragma warning disable CS8602 // ложное срабатывание nullable-аназиза Include, связи обязательные
        _db.PlaybackProgresses
            .Include(p => p.Movie)
            .Include(p => p.Episode).ThenInclude(e => e.Season).ThenInclude(s => s.Series)
            .Where(p => p.UserId == userId && p.DurationSeconds > 0 && p.PositionSeconds / p.DurationSeconds < 0.95)
            .OrderByDescending(p => p.UpdatedAt)
            .Take(20)
            .ToListAsync(ct);
#pragma warning restore CS8602
}