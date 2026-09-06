namespace OnlineCinema.Backend.Models.DTOs;

public class ReportWatchDto
{
    // Фильм или серия (эпизод) — заполнено что-то одно.
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }

    // Примерный объём просмотра, сек (для отрисовки в активности).
    public double WatchedSeconds { get; set; }
}
