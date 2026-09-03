using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data.Configuration;

public class EpisodeConfiguration : IEntityTypeConfiguration<Episode>
{
    public void Configure(EntityTypeBuilder<Episode> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.SeasonId, e.EpisodeNumber }).IsUnique();
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasColumnType("text");
        builder.Property(e => e.VideoUrl).HasMaxLength(500);
        builder.Property(e => e.VideoLocalPath).HasMaxLength(500);
        builder.Property(e => e.ThumbnailUrl).HasMaxLength(500);
    }
}
