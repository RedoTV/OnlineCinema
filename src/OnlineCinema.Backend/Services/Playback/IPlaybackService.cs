using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Services.Playback;

public interface IPlaybackService
{
    Task SaveProgressAsync(int userId, int? movieId, int? episodeId, double position, double duration);
    Task<PlaybackProgress?> GetProgressAsync(int userId, int? movieId, int? episodeId);
    Task<IEnumerable<PlaybackProgress>> GetContinueWatchingAsync(int userId);
}