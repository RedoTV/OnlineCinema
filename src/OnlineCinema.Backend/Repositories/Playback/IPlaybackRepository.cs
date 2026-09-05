using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Playback;

public interface IPlaybackRepository
{
    Task<PlaybackProgress?> FindAsync(int userId, int? movieId, int? episodeId, CancellationToken ct = default);
    void Add(PlaybackProgress progress);
    Task<List<PlaybackProgress>> GetContinueWatchingAsync(int userId, CancellationToken ct = default);
}