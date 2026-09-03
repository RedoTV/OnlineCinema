using OnlineCinema.Backend.Models.DTOs.Actors;
using OnlineCinema.Backend.Models.DTOs.Genres;

namespace OnlineCinema.Backend.Models.DTOs.Series;

public class SeriesDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public string? PosterUrl { get; set; }
    public double AverageRating { get; set; }
    public int SeasonsCount { get; set; }
    public List<SeasonDto> Seasons { get; set; } = new();
    public List<GenreDto> Genres { get; set; } = new();
    public List<ActorDto> Actors { get; set; } = new();
}
