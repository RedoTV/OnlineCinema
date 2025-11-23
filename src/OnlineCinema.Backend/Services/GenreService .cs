using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Genres;

namespace OnlineCinema.Backend.Services;

public class GenreService : IGenreService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GenreService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GenreDto>> GetAllAsync()
    {
        var genres = await _context.Genres.AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<GenreDto>>(genres);
    }

    public async Task<GenreDto?> GetByIdAsync(int id)
    {
        var genre = await _context.Genres.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id);
        return genre == null ? null : _mapper.Map<GenreDto>(genre);
    }

    public async Task<GenreDto> CreateAsync(CreateGenreDto dto)
    {
        var genre = _mapper.Map<Genre>(dto);
        _context.Genres.Add(genre);
        await _context.SaveChangesAsync();
        return _mapper.Map<GenreDto>(genre);
    }

    public async Task<GenreDto> UpdateAsync(int id, UpdateGenreDto dto)
    {
        var genre = await _context.Genres.FindAsync(id);
        if (genre == null) throw new KeyNotFoundException($"Genre with ID {id} not found");

        _mapper.Map(dto, genre);
        await _context.SaveChangesAsync();
        return _mapper.Map<GenreDto>(genre);
    }

    public async Task DeleteAsync(int id)
    {
        var genre = await _context.Genres.FindAsync(id);
        if (genre == null) throw new KeyNotFoundException($"Genre with ID {id} not found");

        _context.Genres.Remove(genre);
        await _context.SaveChangesAsync();
    }
}
