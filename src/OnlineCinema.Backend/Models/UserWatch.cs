namespace OnlineCinema.Backend.Models;

// Факт «юзер реально досмотрел контент» (один настоящий просмотр).
// Пишется плеером один раз за сессию на один контент (фильм или серия),
// когда воспроизведение пересекло порог досмотра. Отдельно от хартбитов
// PlaybackProgress (continue-watching/resume) и от ручной метки UserMovieStatus.
public class UserWatch
{
    public int Id { get; set; }

    public int UserId { get; set; }

    // Фильм или серия (эпизод) — что-то одно заполнено.
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }

    // Примерный объём просмотра, сек (для отрисовки в активности).
    public double WatchedSeconds { get; set; }

    public DateTime WatchedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Movie? Movie { get; set; }
    public Episode? Episode { get; set; }
}
