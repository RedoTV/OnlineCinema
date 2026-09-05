using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Genres;
using OnlineCinema.Backend.Repositories.Genres;
using OnlineCinema.Backend.Repositories.UnitOfWork;

namespace OnlineCinema.Backend.Services.Genres;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _genres;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GenreService(IGenreRepository genres, IUnitOfWork uow, IMapper mapper)
    {
        _genres = genres;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GenreDto>> GetAllAsync()
    {
        var genres = await _genres.GetAllAsync();
        return _mapper.Map<IEnumerable<GenreDto>>(genres);
    }

    public async Task<GenreDto?> GetByIdAsync(int id)
    {
        var genre = await _genres.GetByIdAsync(id);
        return genre == null ? null : _mapper.Map<GenreDto>(genre);
    }

    public async Task<GenreDto> CreateAsync(CreateGenreDto dto)
    {
        var genre = _mapper.Map<Genre>(dto);
        _genres.Add(genre);
        await _uow.SaveChangesAsync();
        return _mapper.Map<GenreDto>(genre);
    }

    public async Task<GenreDto> UpdateAsync(int id, UpdateGenreDto dto)
    {
        var genre = await _genres.FindAsync(id)
            ?? throw new KeyNotFoundException($"Genre with ID {id} not found");

        _mapper.Map(dto, genre);
        await _uow.SaveChangesAsync();
        return _mapper.Map<GenreDto>(genre);
    }

    public async Task DeleteAsync(int id)
    {
        var genre = await _genres.FindAsync(id)
            ?? throw new KeyNotFoundException($"Genre with ID {id} not found");

        _genres.Remove(genre);
        await _uow.SaveChangesAsync();
    }
}