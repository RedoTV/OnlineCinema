namespace OnlineCinema.Backend.Models.DTOs.Movies;

public class UpdateMovieDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? Duration { get; set; }
    public List<int>? GenreIds { get; set; }
    public List<int>? ActorIds { get; set; }
}