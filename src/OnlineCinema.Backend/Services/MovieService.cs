using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Movies;

namespace OnlineCinema.Backend.Services;

public class MovieService : IMovieService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStorageService _storage;

    public MovieService(ApplicationDbContext context, IMapper mapper, IStorageService storage)
    {
        _context = context;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<IEnumerable<MovieSummaryDto>> GetAllAsync(string? search, int? genreId)
    {
        // Списочная проекция одним SQL-запросом: без Include жанров/актёров/оценок
        // (раньше это раздувало полезную нагрузку декартовым произведением).
        var query = _context.Movies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Title.ToLower().Contains(search.ToLower()));

        if (genreId.HasValue)
            query = query.Where(m => m.Genres.Any(g => g.Id == genreId));

        return await query
            .OrderBy(m => m.Id)
            .Select(m => new MovieSummaryDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                Duration = m.Duration,
                PosterUrl = m.PosterUrl,
                AverageRating = m.Ratings.Any() ? m.Ratings.Average(r => r.RatingValue) : 0,
                HasVideo = !string.IsNullOrEmpty(m.VideoUrl)
            })
            .ToListAsync();
    }

    public async Task<MovieDto?> GetByIdAsync(int id)
    {
        var movie = await _context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Actors)
            .Include(m => m.Ratings)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return movie == null ? null : _mapper.Map<MovieDto>(movie);
    }

    public async Task<MovieDto> CreateAsync(CreateMovieDto dto)
    {
        var movie = _mapper.Map<Movie>(dto);

        if (dto.GenreIds != null && dto.GenreIds.Any())
            movie.Genres = await _context.Genres.Where(g => dto.GenreIds.Contains(g.Id)).ToListAsync();

        if (dto.ActorIds != null && dto.ActorIds.Any())
            movie.Actors = await _context.Actors.Where(a => dto.ActorIds.Contains(a.Id)).ToListAsync();

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();

        return _mapper.Map<MovieDto>(movie);
    }

    public async Task<MovieDto> UpdateAsync(int id, UpdateMovieDto dto)
    {
        var movie = await _context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Actors)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null) throw new KeyNotFoundException($"Movie with ID {id} not found");

        _mapper.Map(dto, movie);

        if (dto.GenreIds != null)
        {
            var genres = await _context.Genres.Where(g => dto.GenreIds.Contains(g.Id)).ToListAsync();
            movie.Genres = genres;
        }

        if (dto.ActorIds != null)
        {
            var actors = await _context.Actors.Where(a => dto.ActorIds.Contains(a.Id)).ToListAsync();
            movie.Actors = actors;
        }

        await _context.SaveChangesAsync();
        return _mapper.Map<MovieDto>(movie);
    }

    public async Task<string> UploadPosterAsync(int id, IFormFile file)
    {
        ValidateUpload(file, new[] { "image/jpeg", "image/png", "image/webp", "image/svg+xml" }, 15 * 1024 * 1024, "poster");
        var movie = await _context.Movies.FindAsync(id) ?? throw new KeyNotFoundException($"Movie with ID {id} not found");
        if (!string.IsNullOrEmpty(movie.PosterUrl) && _storage.TryParseObjectKey(movie.PosterUrl, out var oldKey))
            await _storage.DeleteAsync(oldKey);

        // Стримим файл прямо в MinIO, не буферизуя в память
        var ext = Path.GetExtension(file.FileName);
        var key = $"posters/{Guid.NewGuid()}{ext}";
        await using var stream = file.OpenReadStream();
        movie.PosterUrl = await _storage.SaveAsync(stream, key, file.ContentType);

        await _context.SaveChangesAsync();
        return movie.PosterUrl;
    }

    public async Task<string> UploadVideoAsync(int id, IFormFile file)
    {
        ValidateUpload(file, new[] { "video/mp4", "video/webm", "video/ogg", "video/x-matroska", "application/octet-stream" }, 10L * 1024 * 1024 * 1024, "video");
        var movie = await _context.Movies.FindAsync(id) ?? throw new KeyNotFoundException($"Movie with ID {id} not found");
        if (!string.IsNullOrEmpty(movie.VideoUrl) && _storage.TryParseObjectKey(movie.VideoUrl, out var oldKey))
            await _storage.DeleteAsync(oldKey);

        var ext = Path.GetExtension(file.FileName);
        var key = $"films/{Guid.NewGuid()}{ext}";
        await using var stream = file.OpenReadStream();
        movie.VideoUrl = await _storage.SaveAsync(stream, key, file.ContentType ?? "video/mp4");

        await _context.SaveChangesAsync();
        return movie.VideoUrl;
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
        var movie = await _context.Movies.FindAsync(id) ?? throw new KeyNotFoundException($"Movie with ID {id} not found");
        if (_storage.TryParseObjectKey(movie.PosterUrl, out var pKey)) await _storage.DeleteAsync(pKey);
        if (_storage.TryParseObjectKey(movie.VideoUrl, out var vKey)) await _storage.DeleteAsync(vKey);
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
    }
}