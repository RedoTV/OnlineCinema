using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Actors;

public class ActorRepository : IActorRepository
{
    private readonly ApplicationDbContext _db;

    public ActorRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Actor>> GetAllAsync(CancellationToken ct = default) =>
        _db.Actors.AsNoTracking().ToListAsync(ct);

    public Task<Actor?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Actors
            .Include(a => a.Movies)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Actor?> FindAsync(int id, CancellationToken ct = default) =>
        await _db.Actors.FindAsync(new object[] { id }, ct);

    public void Add(Actor actor) => _db.Actors.Add(actor);

    public void Remove(Actor actor) => _db.Actors.Remove(actor);
}