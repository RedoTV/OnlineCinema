using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data.Configuration;

public class ActorConfiguration : IEntityTypeConfiguration<Actor>
{
    public void Configure(EntityTypeBuilder<Actor> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.LastName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.PhotoLocalPath)
            .HasMaxLength(500);

        builder.Property(e => e.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(e => e.Biography)
            .HasColumnType("text");
    }
}