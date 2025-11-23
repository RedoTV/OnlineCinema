namespace OnlineCinema.Backend.Models.DTOs.UserActions;

public class SetRatingDto
{
    public int MovieId { get; set; }
    public int Rating { get; set; }
    public string? Review { get; set; }
}