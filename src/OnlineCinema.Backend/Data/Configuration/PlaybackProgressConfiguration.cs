using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data.Configuration;

public class PlaybackProgressConfiguration : IEntityTypeConfiguration<PlaybackProgress>
{
    public void Configure(EntityTypeBuilder<PlaybackProgress> builder)
    {
        builder.HasKey(e => e.Id);
        // уникальный прогресс на фильм/эпизод для юзера.
        // т.к. оба поля nullable, EF не сделает это чисто одним индексом,
        // но в подавляющем большинстве случаев одно из полей заполнено
        builder.HasIndex(e => new { e.UserId, e.MovieId });
        builder.HasIndex(e => new { e.UserId, e.EpisodeId });

        builder.HasOne(p => p.User)
            .WithMany(u => u.PlaybackProgresses)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Movie)
            .WithMany()
            .HasForeignKey(p => p.MovieId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(p => p.Episode)
            .WithMany(e => e.PlaybackProgresses)
            .HasForeignKey(p => p.EpisodeId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);
    }
}
