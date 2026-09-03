using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Actors;

namespace OnlineCinema.Backend.Services;

public class ActorService : IActorService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IStorageService _storage;

    public ActorService(ApplicationDbContext context, IMapper mapper, IStorageService storage)
    {
        _context = context;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<IEnumerable<ActorDto>> GetAllAsync()
    {
        var actors = await _context.Actors.AsNoTracking().ToListAsync();
        return _mapper.Map<IEnumerable<ActorDto>>(actors);
    }

    public async Task<ActorDto?> GetByIdAsync(int id)
    {
        var actor = await _context.Actors
            .Include(a => a.Movies)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return actor == null ? null : _mapper.Map<ActorDto>(actor);
    }

    public async Task<ActorDto> CreateAsync(CreateActorDto dto)
    {
        var actor = _mapper.Map<Actor>(dto);
        _context.Actors.Add(actor);
        await _context.SaveChangesAsync();
        return _mapper.Map<ActorDto>(actor);
    }

    public async Task<ActorDto> UpdateAsync(int id, UpdateActorDto dto)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null) throw new KeyNotFoundException($"Actor with ID {id} not found");

        _mapper.Map(dto, actor);
        await _context.SaveChangesAsync();
        return _mapper.Map<ActorDto>(actor);
    }

    public async Task<string> UploadPhotoAsync(int id, IFormFile file)
    {
        var actor = await _context.Actors.FindAsync(id) ?? throw new KeyNotFoundException($"Actor with ID {id} not found");

        if (!string.IsNullOrEmpty(actor.PhotoUrl) && _storage.TryParseObjectKey(actor.PhotoUrl, out var oldKey))
            await _storage.DeleteAsync(oldKey);

        var key = $"actors/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var stream = file.OpenReadStream();
        actor.PhotoUrl = await _storage.SaveAsync(stream, key, file.ContentType);
        await _context.SaveChangesAsync();

        return actor.PhotoUrl;
    }

    public async Task DeleteAsync(int id)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null) throw new KeyNotFoundException($"Actor with ID {id} not found");

        if (_storage.TryParseObjectKey(actor.PhotoUrl, out var key)) await _storage.DeleteAsync(key);

        _context.Actors.Remove(actor);
        await _context.SaveChangesAsync();
    }
}
