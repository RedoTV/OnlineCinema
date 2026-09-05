using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Actors;

public interface IActorRepository
{
    Task<List<Actor>> GetAllAsync(CancellationToken ct = default);
    Task<Actor?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Actor?> FindAsync(int id, CancellationToken ct = default);
    void Add(Actor actor);
    void Remove(Actor actor);
}