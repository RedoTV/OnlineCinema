namespace OnlineCinema.Backend.Models.DTOs.UserActions;

public class UserMovieDto
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public string? PosterUrl { get; set; }
}