using OnlineCinema.Backend.Models.Enums;

namespace OnlineCinema.Backend.Models;

public class UserMovieStatus
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MovieId { get; set; }

    public MovieStatus Status { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public Movie Movie { get; set; } = null!;
}