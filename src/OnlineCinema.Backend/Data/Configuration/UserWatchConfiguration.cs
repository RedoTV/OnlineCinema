using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data.Configuration;

public class UserWatchConfiguration : IEntityTypeConfiguration<UserWatch>
{
    public void Configure(EntityTypeBuilder<UserWatch> builder)
    {
        builder.HasKey(e => e.Id);

        // Быстрые выборки «просмотры юзера» и «просмотры контента в окне».
        builder.HasIndex(e => e.WatchedAt);
        builder.HasIndex(e => new { e.MovieId, e.WatchedAt });
        builder.HasIndex(e => e.ViewerKey);

        builder.Property(e => e.UserId).IsRequired(false);
        builder.Property(e => e.ViewerKey).HasMaxLength(64).IsRequired(false);

        builder.HasOne(w => w.User)
            .WithMany(u => u.UserWatches)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(w => w.Movie)
            .WithMany()
            .HasForeignKey(w => w.MovieId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(w => w.Episode)
            .WithMany(e => e.UserWatches)
            .HasForeignKey(w => w.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
