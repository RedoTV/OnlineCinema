using OnlineCinema.Backend.Models.DTOs.Movies;

namespace OnlineCinema.Backend.Services.Movies;

public interface IMovieService
{
    Task<IEnumerable<MovieSummaryDto>> GetAllAsync(string? search, int? genreId);
    Task<MovieDto?> GetByIdAsync(int id);
    Task<MovieDto> CreateAsync(CreateMovieDto dto);
    Task<MovieDto> UpdateAsync(int id, UpdateMovieDto dto);
    Task<string> UploadPosterAsync(int id, IFormFile file);
    Task<string> UploadVideoAsync(int id, IFormFile file);
    Task DeleteAsync(int id);
}