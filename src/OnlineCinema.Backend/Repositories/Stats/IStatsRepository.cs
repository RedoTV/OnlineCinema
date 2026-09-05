namespace OnlineCinema.Backend.Repositories.Stats;

public record TopRatedRow(int Id, string Title, double AvgRating, int RatingCount);
public record MostWatchedRow(int MovieId, int Views, string? Title);
public record GenreStatRow(string Genre, int MovieCount);

public interface IStatsRepository
{
    Task<List<TopRatedRow>> GetTopRatedAsync(int count, CancellationToken ct = default);
    Task<List<MostWatchedRow>> GetMostWatchedAsync(int count, CancellationToken ct = default);
    Task<List<GenreStatRow>> GetGenreStatsAsync(CancellationToken ct = default);
}