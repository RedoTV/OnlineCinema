namespace OnlineCinema.Backend.Models.DTOs.Actors;

public class UpdateActorDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? Biography { get; set; }
}