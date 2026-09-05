using OnlineCinema.Backend.Repositories.Stats;

namespace OnlineCinema.Backend.Services.Stats;

public interface IStatsService
{
    Task<List<TopRatedRow>> GetTopRatedAsync(int count, CancellationToken ct = default);
    Task<List<MostWatchedRow>> GetMostWatchedAsync(int count, CancellationToken ct = default);
    Task<List<GenreStatRow>> GetGenreStatsAsync(CancellationToken ct = default);
}