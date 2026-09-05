using OnlineCinema.Backend.Data;

namespace OnlineCinema.Backend.Repositories.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;

    public UnitOfWork(ApplicationDbContext db) => _db = db;

    public ApplicationDbContext Db => _db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _db.SaveChangesAsync(ct);
}