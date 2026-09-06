namespace OnlineCinema.Backend.Services.Watches;

public interface IWatchService
{
    Task<bool> ReportAsync(int userId, int? movieId, int? episodeId, double watchedSeconds, CancellationToken ct = default);
}
