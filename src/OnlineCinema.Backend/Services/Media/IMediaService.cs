namespace OnlineCinema.Backend.Services.Media;

public interface IMediaService
{
    Task<string?> GetMoviePosterKeyAsync(int movieId, CancellationToken ct = default);
    Task<string?> GetSeriesPosterKeyAsync(int seriesId, CancellationToken ct = default);
    Task<string?> GetActorPhotoKeyAsync(int actorId, CancellationToken ct = default);
    Task<(string Key, string? VideoUrl)?> GetMovieVideoAsync(int id, CancellationToken ct = default);
    Task<(string Key, string? VideoUrl)?> GetEpisodeVideoAsync(int episodeId, CancellationToken ct = default);
}