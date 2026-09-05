using OnlineCinema.Backend.Services.Storage;
using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Series;
using OnlineCinema.Backend.Repositories.Series;
using OnlineCinema.Backend.Repositories.UnitOfWork;

namespace OnlineCinema.Backend.Services.Series;

public class SeriesService : ISeriesService
{
    private readonly ISeriesRepository _series;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IStorageService _storage;

    public SeriesService(ISeriesRepository series, IUnitOfWork uow, IMapper mapper, IStorageService storage)
    {
        _series = series;
        _uow = uow;
        _mapper = mapper;
        _storage = storage;
    }

    public Task<IEnumerable<SeriesSummaryDto>> GetAllAsync(string? search, int? genreId) =>
        _series.GetSummariesAsync(search, genreId);

    public async Task<SeriesDto?> GetByIdAsync(int id)
    {
        var series = await _series.GetByIdAsync(id);
        return series == null ? null : _mapper.Map<SeriesDto>(series);
    }

    public async Task<SeriesDto> CreateAsync(CreateSeriesDto dto)
    {
        var series = _mapper.Map<Models.Series>(dto);

        if (dto.GenreIds != null && dto.GenreIds.Any())
            series.Genres = await _series.GetGenresByIdsAsync(dto.GenreIds);

        if (dto.ActorIds != null && dto.ActorIds.Any())
            series.Actors = await _series.GetActorsByIdsAsync(dto.ActorIds);

        _series.Add(series);
        await _uow.SaveChangesAsync();
        return _mapper.Map<SeriesDto>(series);
    }

    public async Task<SeriesDto> UpdateAsync(int id, UpdateSeriesDto dto)
    {
        var series = await _series.FindAsync(id)
            ?? throw new KeyNotFoundException($"Series with ID {id} not found");

        _mapper.Map(dto, series);

        if (dto.GenreIds != null)
            series.Genres = await _series.GetGenresByIdsAsync(dto.GenreIds);

        if (dto.ActorIds != null)
            series.Actors = await _series.GetActorsByIdsAsync(dto.ActorIds);

        await _uow.SaveChangesAsync();
        return _mapper.Map<SeriesDto>(series);
    }

    public async Task<SeasonDto> AddSeasonAsync(int seriesId, CreateSeasonDto dto)
    {
        _ = await _series.FindAsync(seriesId) ?? throw new KeyNotFoundException("Series not found");

        var season = _mapper.Map<Season>(dto);
        season.SeriesId = seriesId;
        _series.AddSeason(season);
        await _uow.SaveChangesAsync();
        return _mapper.Map<SeasonDto>(season);
    }

    public async Task<EpisodeDto> AddEpisodeAsync(int seasonId, CreateEpisodeDto dto)
    {
        _ = await _series.FindSeasonAsync(seasonId) ?? throw new KeyNotFoundException("Season not found");

        var episode = _mapper.Map<Episode>(dto);
        episode.SeasonId = seasonId;
        _series.AddEpisode(episode);
        await _uow.SaveChangesAsync();
        return _mapper.Map<EpisodeDto>(episode);
    }

    public async Task<string> UploadPosterAsync(int id, IFormFile file)
    {
        ValidateUpload(file, new[] { "image/jpeg", "image/png", "image/webp", "image/svg+xml" }, 15 * 1024 * 1024, "poster");
        var series = await _series.FindAsync(id) ?? throw new KeyNotFoundException($"Series with ID {id} not found");

        if (!string.IsNullOrEmpty(series.PosterUrl) && _storage.TryParseObjectKey(series.PosterUrl, out var oldKey))
            await _storage.DeleteAsync(oldKey);

        var key = $"series-posters/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var stream = file.OpenReadStream();
        series.PosterUrl = await _storage.SaveAsync(stream, key, file.ContentType);

        await _uow.SaveChangesAsync();
        return series.PosterUrl;
    }

    public async Task<string> UploadEpisodeVideoAsync(int episodeId, IFormFile file)
    {
        ValidateUpload(file, new[] { "video/mp4", "video/webm", "video/ogg", "video/x-matroska", "application/octet-stream" }, 10L * 1024 * 1024 * 1024, "video");
        var episode = await _series.FindEpisodeAsync(episodeId) ?? throw new KeyNotFoundException($"Episode with ID {episodeId} not found");

        if (!string.IsNullOrEmpty(episode.VideoUrl) && _storage.TryParseObjectKey(episode.VideoUrl, out var oldKey))
            await _storage.DeleteAsync(oldKey);

        var key = $"episodes/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var stream = file.OpenReadStream();
        episode.VideoUrl = await _storage.SaveAsync(stream, key, file.ContentType ?? "video/mp4");

        await _uow.SaveChangesAsync();
        return episode.VideoUrl;
    }

    private static void ValidateUpload(IFormFile file, IReadOnlyCollection<string> allowedTypes, long maxBytes, string label)
    {
        if (file == null || file.Length == 0) throw new ArgumentException($"The {label} file is empty.");
        if (file.Length > maxBytes) throw new ArgumentException($"The {label} file is too large.");
        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Unsupported {label} content type: {file.ContentType}.");
    }

    public async Task DeleteAsync(int id)
    {
        var series = await _series.GetWithEpisodesAsync(id)
            ?? throw new KeyNotFoundException($"Series with ID {id} not found");

        if (_storage.TryParseObjectKey(series.PosterUrl, out var pKey)) await _storage.DeleteAsync(pKey);

        foreach (var ep in series.Seasons.SelectMany(se => se.Episodes))
            if (_storage.TryParseObjectKey(ep.VideoUrl, out var vKey)) await _storage.DeleteAsync(vKey);

        _series.Remove(series);
        await _uow.SaveChangesAsync();
    }
}