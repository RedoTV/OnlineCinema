using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data.Configuration;

public class UserMovieStatusConfiguration : IEntityTypeConfiguration<UserMovieStatus>
{
    public void Configure(EntityTypeBuilder<UserMovieStatus> builder)
    {
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.UserId, e.MovieId, e.Status }).IsUnique();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.AddedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(ums => ums.User)
            .WithMany(u => u.UserMovieStatuses)
            .HasForeignKey(ums => ums.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ums => ums.Movie)
            .WithMany(m => m.UserMovieStatuses)
            .HasForeignKey(ums => ums.MovieId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}