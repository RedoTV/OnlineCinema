namespace OnlineCinema.Backend.Models.DTOs.Genres;

public class UpdateGenreDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}