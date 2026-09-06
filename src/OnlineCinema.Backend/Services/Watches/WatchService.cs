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

    public async Task<bool> ReportAsync(int userId, int? movieId, int? episodeId, double watchedSeconds, CancellationToken ct = default)
    {
        // Должен быть либо фильм, либо серия — не оба, не ни одного.
        if ((movieId.HasValue) == (episodeId.HasValue))
            return false;

        // Не считаем «просмотром» вырожденные/случайные отчёты.
        if (movieId.HasValue && !await _watches.HasMovieAsync(movieId!.Value, ct))
            return false;
        if (episodeId.HasValue && !await _watches.HasEpisodeAsync(episodeId!.Value, ct))
            return false;

        _watches.Add(new UserWatch
        {
            UserId = userId,
            MovieId = movieId,
            EpisodeId = episodeId,
            WatchedSeconds = watchedSeconds < 0 ? 0 : watchedSeconds,
            WatchedAt = DateTime.UtcNow
        });

        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
