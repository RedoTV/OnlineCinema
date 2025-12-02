using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Data.Configuration;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.Property(e => e.Name).HasMaxLength(50).IsRequired();
    }
}