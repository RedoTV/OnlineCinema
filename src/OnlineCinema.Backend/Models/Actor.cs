namespace OnlineCinema.Backend.Models;

public class Actor
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateOnly? BirthDate { get; set; }

    public string? PhotoLocalPath { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Biography { get; set; }

    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    public ICollection<Series> Series { get; set; } = new List<Series>();
}