namespace OnlineCinema.Backend.Models.DTOs.Series;

public class SeasonDto
{
    public int Id { get; set; }
    public int SeasonNumber { get; set; }
    public string? Title { get; set; }
    public int? ReleaseYear { get; set; }
    public List<EpisodeDto> Episodes { get; set; } = new();
}
