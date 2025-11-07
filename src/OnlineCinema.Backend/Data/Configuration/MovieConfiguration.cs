using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data.Configuration;

public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Title);
        builder.HasIndex(e => e.ReleaseYear);

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnType("text");

        builder.Property(e => e.PosterLocalPath)
            .HasMaxLength(500);

        builder.Property(e => e.PosterUrl)
            .HasMaxLength(500);

        builder.Property(e => e.VideoLocalPath)
            .HasMaxLength(500);

        builder.Property(e => e.VideoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasMany(m => m.Genres)
            .WithMany(g => g.Movies)
            .UsingEntity(j => j.ToTable("MovieGenres"));

        builder.HasMany(m => m.Actors)
            .WithMany(a => a.Movies)
            .UsingEntity(j => j.ToTable("MovieActors"));
    }
}
