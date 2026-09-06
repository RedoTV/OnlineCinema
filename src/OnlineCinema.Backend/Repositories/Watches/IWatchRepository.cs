using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Watches;

// Тонкая запись факта «реально досмотрел контент». Чтение (тренды, активность,
// overview, most-watched) живёт в аналитик-репозиториях поверх _db.UserWatches —
// как и остальные аналитические выборки.
public interface IWatchRepository
{
    Task<bool> HasMovieAsync(int movieId, CancellationToken ct = default);
    Task<bool> HasEpisodeAsync(int episodeId, CancellationToken ct = default);
    Task<UserWatch?> FindAsync(int? userId, string? viewerKey, int? movieId, int? episodeId, CancellationToken ct = default);
    void Add(UserWatch watch);
}
