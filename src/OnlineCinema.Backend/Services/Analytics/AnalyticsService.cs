using OnlineCinema.Backend.Repositories.Analytics;

namespace OnlineCinema.Backend.Services.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analytics;

    public AnalyticsService(IAnalyticsRepository analytics) => _analytics = analytics;

    public Task<List<TrendingRow>> GetTrendingAsync(TimeSpan window, int count, CancellationToken ct = default)
        => _analytics.GetTrendingAsync(window, count, ct);

    public Task<OverviewRow> GetOverviewAsync(CancellationToken ct = default)
        => _analytics.GetOverviewAsync(ct);

    public Task<List<ActivityRow>> GetActivityAsync(int? userId, int limit, CancellationToken ct = default)
        => _analytics.GetActivityAsync(userId, limit, ct);

    public Task<List<PickRow>> GetPicksAsync(int userId, int count, CancellationToken ct = default)
        => _analytics.GetPicksAsync(userId, count, ct);

    public Task<DashboardFeed> GetDashboardAsync(string? contentType, int? genreId, TimeSpan window, CancellationToken ct = default)
        => _analytics.GetDashboardAsync(contentType, genreId, window, ct);
}
