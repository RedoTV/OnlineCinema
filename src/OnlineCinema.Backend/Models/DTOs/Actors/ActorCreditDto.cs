namespace OnlineCinema.Backend.Models.DTOs.Actors;

// Работа актёра (фильм или сериал) для блока «Фильмография».
public class ActorCreditDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = "movie"; // movie | series
    public int? ReleaseYear { get; set; }
    public string? PosterUrl { get; set; }
    public double AverageRating { get; set; }
}
