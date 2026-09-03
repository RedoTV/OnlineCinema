namespace OnlineCinema.Backend.Models.DTOs;

public class SaveProgressDto
{
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }
    public double PositionSeconds { get; set; }
    public double DurationSeconds { get; set; }
}
