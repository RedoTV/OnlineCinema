using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Series;

namespace OnlineCinema.Backend.Repositories.Series;

public interface ISeriesRepository
{
    Task<IEnumerable<SeriesSummaryDto>> GetSummariesAsync(string? search, int? genreId, CancellationToken ct = default);
    Task<Models.Series?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Models.Series?> FindAsync(int id, CancellationToken ct = default);
    void Add(Models.Series series);
    void Remove(Models.Series series);
    Task<Season?> FindSeasonAsync(int seasonId, CancellationToken ct = default);
    void AddSeason(Season season);
    Task<List<Genre>> GetGenresByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<List<Actor>> GetActorsByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    void AddEpisode(Episode episode);
    Task<Episode?> FindEpisodeAsync(int episodeId, CancellationToken ct = default);
    Task<Models.Series?> GetWithEpisodesAsync(int id, CancellationToken ct = default);
}