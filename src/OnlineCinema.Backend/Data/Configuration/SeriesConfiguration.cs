using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data.Configuration;

public class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
    public void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Title);
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.PosterUrl).HasMaxLength(500);
        builder.Property(e => e.PosterLocalPath).HasMaxLength(500);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasMany(s => s.Seasons)
            .WithOne(se => se.Series)
            .HasForeignKey(se => se.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Genres).WithMany(g => g.Series).UsingEntity(j => j.ToTable("SeriesGenres"));
        builder.HasMany(s => s.Actors).WithMany(a => a.Series).UsingEntity(j => j.ToTable("SeriesActors"));
    }
}
