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
    private readonly IFileService _fileService;

    public MovieService(ApplicationDbContext context, IMapper mapper, IFileService fileService)
    {
        _context = context;
        _mapper = mapper;
        _fileService = fileService;
    }

    public async Task<IEnumerable<MovieDto>> GetAllAsync(string? search, int? genreId)
    {
        var query = _context.Movies
            .Include(m => m.Genres)
            .Include(m => m.Ratings)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Title.ToLower().Contains(search.ToLower()));

        if (genreId.HasValue)
            query = query.Where(m => m.Genres.Any(g => g.Id == genreId));

        var movies = await query.ToListAsync();
        return _mapper.Map<IEnumerable<MovieDto>>(movies);
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
        var movie = await _context.Movies.FindAsync(id) ?? throw new KeyNotFoundException($"Movie with ID {id} not found");
        if (!string.IsNullOrEmpty(movie.PosterUrl)) _fileService.DeleteFile(movie.PosterUrl);
        movie.PosterUrl = await _fileService.SaveFileAsync(file, "posters");
        await _context.SaveChangesAsync();
        return movie.PosterUrl;
    }

    public async Task<string> UploadVideoAsync(int id, IFormFile file)
    {
        var movie = await _context.Movies.FindAsync(id) ?? throw new KeyNotFoundException($"Movie with ID {id} not found");
        if (!string.IsNullOrEmpty(movie.VideoUrl)) _fileService.DeleteFile(movie.VideoUrl);
        movie.VideoUrl = await _fileService.SaveFileAsync(file, "films");
        await _context.SaveChangesAsync();
        return movie.VideoUrl;
    }

    public async Task DeleteAsync(int id)
    {
        var movie = await _context.Movies.FindAsync(id) ?? throw new KeyNotFoundException($"Movie with ID {id} not found");
        _fileService.DeleteFile(movie.PosterUrl);
        _fileService.DeleteFile(movie.VideoUrl);
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync();
    }
}