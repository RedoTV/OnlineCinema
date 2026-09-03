namespace OnlineCinema.Backend.Models.DTOs.Series;

public class CreateSeasonDto
{
    public int SeasonNumber { get; set; }
    public string? Title { get; set; }
    public int? ReleaseYear { get; set; }
}
