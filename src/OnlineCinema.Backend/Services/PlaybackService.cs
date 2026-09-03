using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Services;

public interface IPlaybackService
{
    Task SaveProgressAsync(int userId, int? movieId, int? episodeId, double position, double duration);
    Task<PlaybackProgress?> GetProgressAsync(int userId, int? movieId, int? episodeId);
    Task<IEnumerable<PlaybackProgress>> GetContinueWatchingAsync(int userId);
}

public class PlaybackService : IPlaybackService
{
    private readonly ApplicationDbContext _context;

    public PlaybackService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveProgressAsync(int userId, int? movieId, int? episodeId, double position, double duration)
    {
        var existing = await _context.PlaybackProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId
                && (movieId.HasValue ? p.MovieId == movieId : p.MovieId == null)
                && (episodeId.HasValue ? p.EpisodeId == episodeId : p.EpisodeId == null));

        if (existing != null)
        {
            existing.PositionSeconds = position;
            existing.DurationSeconds = duration;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _context.PlaybackProgresses.Add(new PlaybackProgress
            {
                UserId = userId,
                MovieId = movieId,
                EpisodeId = episodeId,
                PositionSeconds = position,
                DurationSeconds = duration
            });
        }
        await _context.SaveChangesAsync();
    }

    public async Task<PlaybackProgress?> GetProgressAsync(int userId, int? movieId, int? episodeId)
    {
        return await _context.PlaybackProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId
                && (movieId.HasValue ? p.MovieId == movieId : p.MovieId == null)
                && (episodeId.HasValue ? p.EpisodeId == episodeId : p.EpisodeId == null));
    }

    public async Task<IEnumerable<PlaybackProgress>> GetContinueWatchingAsync(int userId)
    {
        // пока что-то посмотрел, но не досмотрел до конца (осталось больше 5%).
        // включаю вложенные сущности для заголовка в UI.
#pragma warning disable CS8602 // ложное срабатывание nullable-аназиза Include, связи обязательные
        var items = await _context.PlaybackProgresses
            .Include(p => p.Movie)
            .Include(p => p.Episode).ThenInclude(e => e.Season).ThenInclude(s => s.Series)
            .Where(p => p.UserId == userId && p.DurationSeconds > 0 && p.PositionSeconds / p.DurationSeconds < 0.95)
            .OrderByDescending(p => p.UpdatedAt)
            .Take(20)
            .ToListAsync();
#pragma warning restore CS8602

        return items;
    }
}
