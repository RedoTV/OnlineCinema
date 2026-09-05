namespace OnlineCinema.Backend.Models.DTOs.Movies;

/// <summary>
/// Лёгкая проекция для списков (каталог, админка): без жанров, актёров
/// и оценок. Полные данные — только через GET by id.
/// </summary>
public class MovieSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? Duration { get; set; }
    public string? PosterUrl { get; set; }
    public double AverageRating { get; set; }
    public bool HasVideo { get; set; }
}