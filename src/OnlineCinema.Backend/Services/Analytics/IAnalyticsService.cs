using OnlineCinema.Backend.Repositories.Analytics;

namespace OnlineCinema.Backend.Services.Analytics;

public interface IAnalyticsService
{
    Task<List<TrendingRow>> GetTrendingAsync(TimeSpan window, int count, CancellationToken ct = default);
    Task<OverviewRow> GetOverviewAsync(CancellationToken ct = default);
    Task<List<ActivityRow>> GetActivityAsync(int? userId, int limit, CancellationToken ct = default);
    Task<List<PickRow>> GetPicksAsync(int userId, int count, CancellationToken ct = default);
}
