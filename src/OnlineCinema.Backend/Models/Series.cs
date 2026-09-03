namespace OnlineCinema.Backend.Models;

public class Series
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public string? PosterUrl { get; set; }
    public string? PosterLocalPath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Season> Seasons { get; set; } = new List<Season>();
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public ICollection<Actor> Actors { get; set; } = new List<Actor>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
