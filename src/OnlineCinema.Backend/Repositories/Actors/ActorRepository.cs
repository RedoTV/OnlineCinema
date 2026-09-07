using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Actors;

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
            .Include(a => a.Series)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Actor?> FindAsync(int id, CancellationToken ct = default) =>
        await _db.Actors.FindAsync(new object[] { id }, ct);

    public async Task<List<ActorCreditDto>> GetCreditsAsync(int id, CancellationToken ct = default)
    {
        var actor = await _db.Actors
            .Include(a => a.Movies).ThenInclude(m => m.Ratings)
            .Include(a => a.Series).ThenInclude(s => s.Ratings)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, ct);

        if (actor == null) return new List<ActorCreditDto>();

        var credits = new List<ActorCreditDto>();

        foreach (var m in actor.Movies)
        {
            credits.Add(new ActorCreditDto
            {
                Id = m.Id,
                Title = m.Title,
                Type = "movie",
                ReleaseYear = m.ReleaseYear,
                PosterUrl = m.PosterUrl,
                AverageRating = m.Ratings.Any() ? m.Ratings.Average(r => r.RatingValue) : 0,
            });
        }

        foreach (var s in actor.Series)
        {
            credits.Add(new ActorCreditDto
            {
                Id = s.Id,
                Title = s.Title,
                Type = "series",
                ReleaseYear = s.ReleaseYear,
                PosterUrl = s.PosterUrl,
                AverageRating = s.Ratings.Any() ? s.Ratings.Average(r => r.RatingValue) : 0,
            });
        }

        return credits
            .OrderByDescending(c => c.ReleaseYear ?? 0)
            .ToList();
    }

    public void Add(Actor actor) => _db.Actors.Add(actor);

    public void Remove(Actor actor) => _db.Actors.Remove(actor);
}
