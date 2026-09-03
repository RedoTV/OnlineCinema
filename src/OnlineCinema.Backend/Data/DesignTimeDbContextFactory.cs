using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OnlineCinema.Backend.Data;

// Нужен чтобы dotnet ef мог генерить миграции без запущенной базы.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=onlinecinema_db;Username=cinema_user;Password=x")
            .Options;
        return new ApplicationDbContext(options);
    }
}
