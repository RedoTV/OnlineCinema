using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Repositories.UnitOfWork;
using OnlineCinema.Backend.Repositories.Watches;

namespace OnlineCinema.Backend.Services.Watches;

public class WatchService : IWatchService
{
    private readonly IWatchRepository _watches;
    private readonly IUnitOfWork _uow;

    public WatchService(IWatchRepository watches, IUnitOfWork uow)
    {
        _watches = watches;
        _uow = uow;
    }

    public async Task<bool> ReportAsync(int? userId, string? viewerKey, int? movieId, int? episodeId, double watchedSeconds, CancellationToken ct = default)
    {
        // Должен быть либо фильм, либо серия — не оба, не ни одного.
        if ((movieId.HasValue) == (episodeId.HasValue))
            return false;

        // Нормализуем ключ зрителя: у гостя без него репорт бессмыслен
        // (некого считать), у залогиненного — опционален.
        var key = string.IsNullOrWhiteSpace(viewerKey) ? null : viewerKey.Trim();
        if (key is { Length: > 64 }) key = key[..64];
        if (!userId.HasValue && key == null)
            return false;

        // Не считаем «просмотром» вырожденные/случайные отчёты.
        if (movieId.HasValue && !await _watches.HasMovieAsync(movieId!.Value, ct))
            return false;
        if (episodeId.HasValue && !await _watches.HasEpisodeAsync(episodeId!.Value, ct))
            return false;

        // Дедуп: один и тот же зритель один и тот же контент — один факт.
        // Повторный репорт (двойной ended, ремаунт плеера) не раздувает счётчик,
        // а лишь обновляет объём и время.
        var existing = await _watches.FindAsync(userId, key, movieId, episodeId, ct);
        var seconds = watchedSeconds < 0 ? 0 : watchedSeconds;
        if (existing != null)
        {
            if (seconds > existing.WatchedSeconds) existing.WatchedSeconds = seconds;
            existing.WatchedAt = DateTime.UtcNow;
        }
        else
        {
            _watches.Add(new UserWatch
            {
                UserId = userId,
                ViewerKey = key,
                MovieId = movieId,
                EpisodeId = episodeId,
                WatchedSeconds = seconds,
                WatchedAt = DateTime.UtcNow
            });
        }

        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
