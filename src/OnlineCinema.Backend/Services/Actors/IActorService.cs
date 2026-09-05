using OnlineCinema.Backend.Models.DTOs.Actors;

namespace OnlineCinema.Backend.Services.Actors;

public interface IActorService
{
    Task<IEnumerable<ActorDto>> GetAllAsync();
    Task<ActorDto?> GetByIdAsync(int id);
    Task<ActorDto> CreateAsync(CreateActorDto dto);
    Task<ActorDto> UpdateAsync(int id, UpdateActorDto dto);
    Task<string> UploadPhotoAsync(int id, IFormFile file);
    Task DeleteAsync(int id);
}