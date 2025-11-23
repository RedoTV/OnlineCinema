using OnlineCinema.Backend.Models.DTOs.Actors;
using OnlineCinema.Backend.Models.DTOs.Genres;

namespace OnlineCinema.Backend.Models.DTOs.Movies;

public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? Duration { get; set; }
    public string? PosterUrl { get; set; }
    public string? VideoUrl { get; set; }
    public double AverageRating { get; set; }
    public List<GenreDto> Genres { get; set; } = new();
    public List<ActorDto> Actors { get; set; } = new();
}