using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Repositories.Playback;
using OnlineCinema.Backend.Repositories.UnitOfWork;

namespace OnlineCinema.Backend.Services.Playback;

public class PlaybackService : IPlaybackService
{
    private readonly IPlaybackRepository _playback;
    private readonly IUnitOfWork _uow;

    public PlaybackService(IPlaybackRepository playback, IUnitOfWork uow)
    {
        _playback = playback;
        _uow = uow;
    }

    public async Task SaveProgressAsync(int userId, int? movieId, int? episodeId, double position, double duration)
    {
        var existing = await _playback.FindAsync(userId, movieId, episodeId);

        if (existing != null)
        {
            existing.PositionSeconds = position;
            existing.DurationSeconds = duration;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _playback.Add(new PlaybackProgress
            {
                UserId = userId,
                MovieId = movieId,
                EpisodeId = episodeId,
                PositionSeconds = position,
                DurationSeconds = duration
            });
        }
        await _uow.SaveChangesAsync();
    }

    public Task<PlaybackProgress?> GetProgressAsync(int userId, int? movieId, int? episodeId) =>
        _playback.FindAsync(userId, movieId, episodeId);

    public async Task<IEnumerable<PlaybackProgress>> GetContinueWatchingAsync(int userId) =>
        await _playback.GetContinueWatchingAsync(userId);
}