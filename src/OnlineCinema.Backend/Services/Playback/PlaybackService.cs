using OnlineCinema.Backend.Events;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Repositories.Playback;
using OnlineCinema.Backend.Repositories.UnitOfWork;

namespace OnlineCinema.Backend.Services.Playback;

public class PlaybackService : IPlaybackService
{
    private readonly IPlaybackRepository _playback;
    private readonly IUnitOfWork _uow;
    private readonly IEventPublisher _publisher;

    public PlaybackService(IPlaybackRepository playback, IUnitOfWork uow, IEventPublisher publisher)
    {
        _playback = playback;
        _uow = uow;
        _publisher = publisher;
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

        // эвент "досмотрел" публикуем только когда юзер реально дошёл почти до конца
        // (>=95%), иначе каждый тик прогресса свалится в очередь спамом.
        if (duration > 0 && position / duration >= 0.95)
        {
            if (movieId.HasValue)
                await _publisher.PublishAsync("movie.watched", new { userId, movieId, watchSeconds = position, happenedAt = DateTime.UtcNow });
            else if (episodeId.HasValue)
                await _publisher.PublishAsync("episode.watched", new { userId, episodeId, watchSeconds = position, happenedAt = DateTime.UtcNow });
        }
    }

    public Task<PlaybackProgress?> GetProgressAsync(int userId, int? movieId, int? episodeId) =>
        _playback.FindAsync(userId, movieId, episodeId);

    public async Task<IEnumerable<PlaybackProgress>> GetContinueWatchingAsync(int userId) =>
        await _playback.GetContinueWatchingAsync(userId);
}