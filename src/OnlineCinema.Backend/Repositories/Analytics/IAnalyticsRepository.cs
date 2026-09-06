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

// ---- Новый дашборд аналитики -------------------------------

public record DashboardOverviewRow(
    int Movies,
    int Series,
    int ActiveViewers,          // уникальные зрители за всё время
    int WatchEvents,            // всего эвентов просмотра за всё время
    double WatchSeconds,        // всего секунд за всё время
    int RangeViewers,           // уникальные зрители за выбранный период
    int RangeWatchEvents,       // эвентов за выбранный период
    double RangeWatchSeconds,   // секунд за выбранный период
    int RangeRatings,           // оценок за выбранный период
    int TotalRatings);          // оценок за всё время

public record ContentRow(
    int ContentId,
    string ContentType,
    string Title,
    int? ReleaseYear,
    string[] Genres,
    int RatingCount,
    double? AvgRating,
    int Watches,                // эвенты просмотра за всё время
    double WatchSeconds,        // посмотрено за всё время, сек
    int WatchesInRange,         // эвенты за выбранный период
    double WatchSecondsInRange);// посмотрено за выбранный период, сек

public record GenreRowItem(
    int GenreId,
    string Name,
    int Movies,
    int Series,
    int WatchEvents);           // просмотров контента жанра (всё время)

public record TimelineItem(string Label, int Watches);

// Полный набор данных для страницы аналитики.
public record DashboardFeed(
    DashboardOverviewRow Overview,
    ContentRow[] Content,
    GenreRowItem[] Genres,
    TimelineItem[] Activity);

public interface IAnalyticsRepository
{
    Task<List<TrendingRow>> GetTrendingAsync(TimeSpan window, int count, CancellationToken ct = default);
    Task<OverviewRow> GetOverviewAsync(CancellationToken ct = default);
    Task<List<ActivityRow>> GetActivityAsync(int? userId, int limit, CancellationToken ct = default);
    Task<List<PickRow>> GetPicksAsync(int userId, int count, CancellationToken ct = default);

    Task<DashboardFeed> GetDashboardAsync(
        string? contentType,
        int? genreId,
        TimeSpan window,
        CancellationToken ct = default);
}
