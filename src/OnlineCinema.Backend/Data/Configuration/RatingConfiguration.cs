using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data.Configuration;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.HasKey(e => e.Id);

        // юзер может оставить только одну оценку на фильм
        builder.HasIndex(e => new { e.UserId, e.MovieId }).IsUnique();
        // и одну на сериал
        builder.HasIndex(e => new { e.UserId, e.SeriesId }).IsUnique();

        builder.Property(e => e.RatingValue).IsRequired();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Movie)
            .WithMany(m => m.Ratings)
            .HasForeignKey(r => r.MovieId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(r => r.Series)
            .WithMany(s => s.Ratings)
            .HasForeignKey(r => r.SeriesId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
