using OnlineCinema.Backend.Data;

namespace OnlineCinema.Backend.Repositories.UnitOfWork;

/// <summary>
/// Единая точка сохранения: репозитории меняют трекаемые сущности,
/// транзакцию фиксирует только этот метод.
/// </summary>
public interface IUnitOfWork
{
    ApplicationDbContext Db { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}