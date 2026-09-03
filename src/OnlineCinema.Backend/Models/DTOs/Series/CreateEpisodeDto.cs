namespace OnlineCinema.Backend.Models.DTOs.Series;

public class CreateEpisodeDto
{
    public int EpisodeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Duration { get; set; }
}
