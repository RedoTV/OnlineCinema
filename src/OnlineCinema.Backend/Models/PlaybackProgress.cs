namespace OnlineCinema.Backend.Models;

// Хранит где юзер остановился при просмотре. Юзается для "продолжить просмотр".
public class PlaybackProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // null = прогресс по фильму, иначе по эпизоду
    public int? MovieId { get; set; }
    public int? EpisodeId { get; set; }

    public double PositionSeconds { get; set; }
    public double DurationSeconds { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Movie? Movie { get; set; }
    public Episode? Episode { get; set; }
}
