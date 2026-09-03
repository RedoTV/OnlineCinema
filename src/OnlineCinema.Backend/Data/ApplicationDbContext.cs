using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Actor> Actors { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<UserMovieStatus> UserMovieStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        DataSeeder.SeedAdminUsers(modelBuilder);
    }
}