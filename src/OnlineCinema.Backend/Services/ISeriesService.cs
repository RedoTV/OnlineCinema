using OnlineCinema.Backend.Models.DTOs.Series;

namespace OnlineCinema.Backend.Services;

public interface ISeriesService
{
    Task<IEnumerable<SeriesSummaryDto>> GetAllAsync(string? search, int? genreId);
    Task<SeriesDto?> GetByIdAsync(int id);
    Task<SeriesDto> CreateAsync(CreateSeriesDto dto);
    Task<SeriesDto> UpdateAsync(int id, UpdateSeriesDto dto);
    Task<SeasonDto> AddSeasonAsync(int seriesId, CreateSeasonDto dto);
    Task<EpisodeDto> AddEpisodeAsync(int seasonId, CreateEpisodeDto dto);
    Task<string> UploadPosterAsync(int id, IFormFile file);
    Task<string> UploadEpisodeVideoAsync(int episodeId, IFormFile file);
    Task DeleteAsync(int id);
}
