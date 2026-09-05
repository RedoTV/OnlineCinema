using OnlineCinema.Backend.Services.Storage;
using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Actors;
using OnlineCinema.Backend.Repositories.Actors;
using OnlineCinema.Backend.Repositories.UnitOfWork;

namespace OnlineCinema.Backend.Services.Actors;

public class ActorService : IActorService
{
    private readonly IActorRepository _actors;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IStorageService _storage;

    public ActorService(IActorRepository actors, IUnitOfWork uow, IMapper mapper, IStorageService storage)
    {
        _actors = actors;
        _uow = uow;
        _mapper = mapper;
        _storage = storage;
    }

    public async Task<IEnumerable<ActorDto>> GetAllAsync()
    {
        var actors = await _actors.GetAllAsync();
        return _mapper.Map<IEnumerable<ActorDto>>(actors);
    }

    public async Task<ActorDto?> GetByIdAsync(int id)
    {
        var actor = await _actors.GetByIdAsync(id);
        return actor == null ? null : _mapper.Map<ActorDto>(actor);
    }

    public async Task<ActorDto> CreateAsync(CreateActorDto dto)
    {
        var actor = _mapper.Map<Actor>(dto);
        _actors.Add(actor);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ActorDto>(actor);
    }

    public async Task<ActorDto> UpdateAsync(int id, UpdateActorDto dto)
    {
        var actor = await _actors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Actor with ID {id} not found");

        _mapper.Map(dto, actor);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ActorDto>(actor);
    }

    public async Task<string> UploadPhotoAsync(int id, IFormFile file)
    {
        var actor = await _actors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Actor with ID {id} not found");

        if (!string.IsNullOrEmpty(actor.PhotoUrl) && _storage.TryParseObjectKey(actor.PhotoUrl, out var oldKey))
            await _storage.DeleteAsync(oldKey);

        var key = $"actors/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        await using var stream = file.OpenReadStream();
        actor.PhotoUrl = await _storage.SaveAsync(stream, key, file.ContentType);
        await _uow.SaveChangesAsync();

        return actor.PhotoUrl;
    }

    public async Task DeleteAsync(int id)
    {
        var actor = await _actors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Actor with ID {id} not found");

        if (_storage.TryParseObjectKey(actor.PhotoUrl, out var key)) await _storage.DeleteAsync(key);

        _actors.Remove(actor);
        await _uow.SaveChangesAsync();
    }
}