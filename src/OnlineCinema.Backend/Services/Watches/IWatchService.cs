namespace OnlineCinema.Backend.Services.Watches;

public interface IWatchService
{
    // userId = null — анонимный просмотр гостя (различаем по viewerKey).
    Task<bool> ReportAsync(int? userId, string? viewerKey, int? movieId, int? episodeId, double watchedSeconds, CancellationToken ct = default);
}
