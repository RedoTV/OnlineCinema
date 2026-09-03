namespace OnlineCinema.Backend.Models.DTOs.UserActions;

public class SetRatingDto
{
    public int? MovieId { get; set; }
    public int? SeriesId { get; set; }
    public int Rating { get; set; }
}
