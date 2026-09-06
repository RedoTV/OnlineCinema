using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Watches;

public class WatchRepository : IWatchRepository
{
    private readonly ApplicationDbContext _db;

    public WatchRepository(ApplicationDbContext db) => _db = db;

    public Task<bool> HasMovieAsync(int movieId, CancellationToken ct = default) =>
        _db.Movies.AnyAsync(m => m.Id == movieId, ct);

    public Task<bool> HasEpisodeAsync(int episodeId, CancellationToken ct = default) =>
        _db.Episodes.AnyAsync(e => e.Id == episodeId, ct);

    public void Add(UserWatch watch) => _db.UserWatches.Add(watch);
}
