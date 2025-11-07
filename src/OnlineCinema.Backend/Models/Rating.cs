namespace OnlineCinema.Backend.Models;

public class Rating
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MovieId { get; set; }

    public int RatingValue { get; set; } // от 1 до 10

    public string? Review { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public Movie Movie { get; set; } = null!;
}