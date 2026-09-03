namespace OnlineCinema.Backend.Models;

public class Season
{
    public int Id { get; set; }
    public int SeriesId { get; set; }
    public int SeasonNumber { get; set; }
    public string? Title { get; set; }
    public int? ReleaseYear { get; set; }

    public Series Series { get; set; } = null!;
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
}
