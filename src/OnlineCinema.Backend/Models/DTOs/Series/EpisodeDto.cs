namespace OnlineCinema.Backend.Models.DTOs.Series;

public class EpisodeDto
{
    public int Id { get; set; }
    public int EpisodeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Duration { get; set; }
    public string? VideoUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
}
