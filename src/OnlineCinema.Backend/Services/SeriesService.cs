using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Series;

namespace OnlineCinema.Backend.Services;

public class SeriesService : ISeriesService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStorageService _storage;

    public SeriesService(ApplicationDbContext context, IMapper mapper, IStorageService storage)
    {
        _context = context;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<IEnumerable<SeriesDto>> GetAllAsync(string? search, int? genreId)
    {
        var query = _context.Series
            .Include(s => s.Seasons)
            .Include(s => s.Genres)
            .Include(s => s.Ratings)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(s => s.Title.ToLower().Contains(search.ToLower()));

        if (genreId.HasValue)
            query = query.Where(s => s.Genres.Any(g => g.Id == genreId));

        var series = await query.ToListAsync();
        return _mapper.Map<IEnumerable<SeriesDto>>(series);
    }

    public async Task<SeriesDto?> GetByIdAsync(int id)
    {
        var series = await _context.Series
            .Include(s => s.Seasons).ThenInclude(se => se.Episodes)
            .Include(s => s.Genres)
            .Include(s => s.Actors)
            .Include(s => s.Ratings)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return series == null ? null : _mapper.Map<SeriesDto>(series);
    }

    public async Task<SeriesDto> CreateAsync(CreateSeriesDto dto)
    {
        var series = _mapper.Map<Series>(dto);

        if (dto.GenreIds != null && dto.GenreIds.Any())
            series.Genres = await _context.Genres.Where(g => dto.GenreIds.Contains(g.Id)).ToListAsync();

        if (dto.ActorIds != null && dto.ActorIds.Any())
            series.Actors = await _context.Actors.Where(a => dto.ActorIds.Contains(a.Id)).ToListAsync();

        _context.Series.Add(series);
        await _context.SaveChangesAsync();
        return _mapper.Map<SeriesDto>(series);
    }

    public async Task<SeriesDto> UpdateAsync(int id, UpdateSeriesDto dto)
    {
        var series = await _context.Series
            .Include(s => s.Genres)
            .Include(s => s.Actors)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (series == null) throw new KeyNotFoundException($"Series with ID {id} not found");

        _mapper.Map(dto, series);

        if (dto.GenreIds != null)
            series.Genres = await _context.Genres.Where(g => dto.GenreIds.Contains(g.Id)).ToListAsync();

        if (dto.ActorIds != null)
            series.Actors = await _context.Actors.Where(a => dto.ActorIds.Contains(a.Id)).ToListAsync();

        await _context.SaveChangesAsync();
        return _mapper.Map<SeriesDto>(series);
    }

    public async Task<SeasonDto> AddSeasonAsync(int seriesId, CreateSeasonDto dto)
    {
        var series = await _context.Series.FindAsync(seriesId) ?? throw new KeyNotFoundException("Series not found");

        var season = _mapper.Map<Season>(dto);
        season.SeriesId = seriesId;
        _context.Seasons.Add(season);
        await _context.SaveChangesAsync();
        return _mapper.Map<SeasonDto>(season);
    }

    public async Task<EpisodeDto> AddEpisodeAsync(int seasonId, CreateEpisodeDto dto)
    {
        var season = await _context.Seasons.FindAsync(seasonId) ?? throw new KeyNotFoundException("Season not found");

        var episode = _mapper.Map<Episode>(dto);
        episode.SeasonId = seasonId;
        _context.Episodes.Add(episode);
        await _context.SaveChangesAsync();
        return _mapper.Map<EpisodeDto>(episode);
    }

    public async Task<string> UploadPosterAsync(int id, IFormFile file)
    {
        var series = await _context.Series.FindAsync(id) ?? throw new KeyNotFoundException($"Series with ID {id} not found");

        if (!string.IsNullOrEmpty(series.PosterUrl) && _storage.TryParseObjectKey(series.PosterUrl, out var oldKey))
            await _storage.DeleteAsync(oldKey);

        var key = $"series-posters/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var stream = file.OpenReadStream();
        series.PosterUrl = await _storage.SaveAsync(stream, key, file.ContentType);

        await _context.SaveChangesAsync();
        return series.PosterUrl;
    }

    public async Task<string> UploadEpisodeVideoAsync(int episodeId, IFormFile file)
    {
        var episode = await _context.Episodes.FindAsync(episodeId) ?? throw new KeyNotFoundException($"Episode with ID {episodeId} not found");

        if (!string.IsNullOrEmpty(episode.VideoUrl) && _storage.TryParseObjectKey(episode.VideoUrl, out var oldKey))
            await _storage.DeleteAsync(oldKey);

        var key = $"episodes/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var stream = file.OpenReadStream();
        episode.VideoUrl = await _storage.SaveAsync(stream, key, file.ContentType ?? "video/mp4");

        await _context.SaveChangesAsync();
        return episode.VideoUrl;
    }

    public async Task DeleteAsync(int id)
    {
        var series = await _context.Series
            .Include(s => s.Seasons).ThenInclude(se => se.Episodes)
            .FirstOrDefaultAsync(s => s.Id == id) ?? throw new KeyNotFoundException($"Series with ID {id} not found");

        if (_storage.TryParseObjectKey(series.PosterUrl, out var pKey)) await _storage.DeleteAsync(pKey);

        foreach (var ep in series.Seasons.SelectMany(se => se.Episodes))
            if (_storage.TryParseObjectKey(ep.VideoUrl, out var vKey)) await _storage.DeleteAsync(vKey);

        _context.Series.Remove(series);
        await _context.SaveChangesAsync();
    }
}
