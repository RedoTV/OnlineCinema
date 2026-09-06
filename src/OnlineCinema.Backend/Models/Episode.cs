namespace OnlineCinema.Backend.Models;

public class Episode
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public int EpisodeNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Duration { get; set; } // в минутах
    public string? VideoUrl { get; set; }
    public string? VideoLocalPath { get; set; }
    public string? ThumbnailUrl { get; set; }

    public Season Season { get; set; } = null!;
    public ICollection<PlaybackProgress> PlaybackProgresses { get; set; } = new List<PlaybackProgress>();
    public ICollection<UserWatch> UserWatches { get; set; } = new List<UserWatch>();
}
