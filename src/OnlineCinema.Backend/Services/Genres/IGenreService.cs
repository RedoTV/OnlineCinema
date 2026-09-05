using OnlineCinema.Backend.Models.DTOs.Genres;

namespace OnlineCinema.Backend.Services.Genres;

public interface IGenreService
{
    Task<IEnumerable<GenreDto>> GetAllAsync();
    Task<GenreDto?> GetByIdAsync(int id);
    Task<GenreDto> CreateAsync(CreateGenreDto dto);
    Task<GenreDto> UpdateAsync(int id, UpdateGenreDto dto);
    Task DeleteAsync(int id);
}