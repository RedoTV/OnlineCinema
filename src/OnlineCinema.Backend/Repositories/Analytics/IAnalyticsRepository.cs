namespace OnlineCinema.Backend.Repositories.Analytics;

public record TrendingRow(
    string ContentType,
    int ContentId,
    string Title,
    int Views,
    double WatchSeconds,
    int RatingCount,
    double? AvgRating);

public record OverviewRow(int Movies, int Series, int ActiveUsers, int Watches, int Ratings);

public record ActivityRow(
    string? Actor,
    string Kind,
    string? RefType,
    int? RefId,
    string? Title,
    string? Note,
    DateTime HappenedAt);

public record PickRow(int MovieId, string Title, double? Rating, string Reason);

public interface IAnalyticsRepository
{
    Task<List<TrendingRow>> GetTrendingAsync(TimeSpan window, int count, CancellationToken ct = default);
    Task<OverviewRow> GetOverviewAsync(CancellationToken ct = default);
    Task<List<ActivityRow>> GetActivityAsync(int? userId, int limit, CancellationToken ct = default);
    Task<List<PickRow>> GetPicksAsync(int userId, int count, CancellationToken ct = default);
}
