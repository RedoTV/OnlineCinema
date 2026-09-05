using OnlineCinema.Backend.Services.Storage;
using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Movies;
using OnlineCinema.Backend.Repositories.Movies;
using OnlineCinema.Backend.Repositories.UnitOfWork;
using OnlineCinema.Backend.Services;

namespace OnlineCinema.Backend.Services.Movies;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movies;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IStorageService _storage;

    public MovieService(IMovieRepository movies, IUnitOfWork uow, IMapper mapper, IStorageService storage)
    {
        _movies = movies;
        _uow = uow;
        _mapper = mapper;
        _storage = storage;
    }

    public Task<IEnumerable<MovieSummaryDto>> GetAllAsync(string? search, int? genreId) =>
        _movies.GetSummariesAsync(search, genreId);

    public async Task<MovieDto?> GetByIdAsync(int id)
    {
        var movie = await _movies.GetByIdAsync(id);
        return movie == null ? null : _mapper.Map<MovieDto>(movie);
    }

    public async Task<MovieDto> CreateAsync(CreateMovieDto dto)
    {
        var movie = _mapper.Map<Movie>(dto);

        if (dto.GenreIds != null && dto.GenreIds.Any())
            movie.Genres = await _movies.GetGenresByIdsAsync(dto.GenreIds);

        if (dto.ActorIds != null && dto.ActorIds.Any())
            movie.Actors = await _movies.GetActorsByIdsAsync(dto.ActorIds);

        _movies.Add(movie);
        await _uow.SaveChangesAsync();

        return _mapper.Map<MovieDto>(movie);
    }

    public async Task<MovieDto> UpdateAsync(int id, UpdateMovieDto dto)
    {
        // Трекаемая сущность для обновления (GetById возвращает AsNoTracking).
        var movie = await _movies.FindAsync(id)
            ?? throw new KeyNotFoundException($"Movie with ID {id} not found");

        _mapper.Map(dto, movie);

        if (dto.GenreIds != null)
            movie.Genres = await _movies.GetGenresByIdsAsync(dto.GenreIds);

        if (dto.ActorIds != null)
            movie.Actors = await _movies.GetActorsByIdsAsync(dto.ActorIds);

        await _uow.SaveChangesAsync();
        return _mapper.Map<MovieDto>(movie);
    }

    public async Task<string> UploadPosterAsync(int id, IFormFile file)
    {
        ValidateUpload(file, new[] { "image/jpeg", "image/png", "image/webp", "image/svg+xml" }, 15 * 1024 * 1024, "poster");
        var movie = await _movies.FindAsync(id) ?? throw new KeyNotFoundException($"Movie with ID {id} not found");
        if (!string.IsNullOrEmpty(movie.PosterUrl) && _storage.TryParseObjectKey(movie.PosterUrl, out var oldKey))
            await _storage.DeleteAsync(oldKey);

        // Стримим файл прямо в MinIO, не буферизуя в память
        var ext = Path.GetExtension(file.FileName);
        var key = $"posters/{Guid.NewGuid()}{ext}";
        await using var stream = file.OpenReadStream();
        movie.PosterUrl = await _storage.SaveAsync(stream, key, file.ContentType);

        await _uow.SaveChangesAsync();
        return movie.PosterUrl;
    }

    public async Task<string> UploadVideoAsync(int id, IFormFile file)
    {
        ValidateUpload(file, new[] { "video/mp4", "video/webm", "video/ogg", "video/x-matroska", "application/octet-stream" }, 10L * 1024 * 1024 * 1024, "video");
        var movie = await _movies.FindAsync(id) ?? throw new KeyNotFoundException($"Movie with ID {id} not found");
        if (!string.IsNullOrEmpty(movie.VideoUrl) && _storage.TryParseObjectKey(movie.VideoUrl, out var oldKey))
            await _storage.DeleteAsync(oldKey);

        var ext = Path.GetExtension(file.FileName);
        var key = $"films/{Guid.NewGuid()}{ext}";
        await using var stream = file.OpenReadStream();
        movie.VideoUrl = await _storage.SaveAsync(stream, key, file.ContentType ?? "video/mp4");

        await _uow.SaveChangesAsync();
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
        var movie = await _movies.FindAsync(id) ?? throw new KeyNotFoundException($"Movie with ID {id} not found");
        if (_storage.TryParseObjectKey(movie.PosterUrl, out var pKey)) await _storage.DeleteAsync(pKey);
        if (_storage.TryParseObjectKey(movie.VideoUrl, out var vKey)) await _storage.DeleteAsync(vKey);
        _movies.Remove(movie);
        await _uow.SaveChangesAsync();
    }
}