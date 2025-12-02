namespace OnlineCinema.Backend.Models.DTOs.Actors;

public class ActorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? Biography { get; set; }
    public string? PhotoUrl { get; set; }
}