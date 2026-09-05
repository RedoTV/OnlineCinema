using OnlineCinema.Backend.Repositories.Stats;

namespace OnlineCinema.Backend.Services.Stats;

public class StatsService : IStatsService
{
    private readonly IStatsRepository _stats;

    public StatsService(IStatsRepository stats) => _stats = stats;

    public Task<List<TopRatedRow>> GetTopRatedAsync(int count, CancellationToken ct = default) =>
        _stats.GetTopRatedAsync(count, ct);

    public Task<List<MostWatchedRow>> GetMostWatchedAsync(int count, CancellationToken ct = default) =>
        _stats.GetMostWatchedAsync(count, ct);

    public Task<List<GenreStatRow>> GetGenreStatsAsync(CancellationToken ct = default) =>
        _stats.GetGenreStatsAsync(ct);
}