namespace OnlineCinema.Backend.Models.DTOs.Series;

/// <summary>
/// Лёгкая проекция для списка сериалов: без сезонов, эпизодов, жанров
/// и актёров. Полная структура — только через GET by id.
/// </summary>
public class SeriesSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public string? PosterUrl { get; set; }
    public double AverageRating { get; set; }
    public int SeasonsCount { get; set; }
    public int EpisodesCount { get; set; }
    public bool HasVideo { get; set; }
}