namespace OnlineCinema.Backend.Models.DTOs.UserActions;

public class SetStatusDto
{
    public int MovieId { get; set; }
    public string Status { get; set; } = string.Empty;
}