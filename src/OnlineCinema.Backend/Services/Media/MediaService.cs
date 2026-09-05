using OnlineCinema.Backend.Services.Storage;
using OnlineCinema.Backend.Repositories.Actors;
using OnlineCinema.Backend.Repositories.Movies;
using OnlineCinema.Backend.Repositories.Series;

namespace OnlineCinema.Backend.Services.Media;

public class MediaService : IMediaService
{
    private readonly IMovieRepository _movies;
    private readonly ISeriesRepository _series;
    private readonly IActorRepository _actors;
    private readonly IStorageService _storage;

    public MediaService(IMovieRepository movies, ISeriesRepository series, IActorRepository actors, IStorageService storage)
    {
        _movies = movies;
        _series = series;
        _actors = actors;
        _storage = storage;
    }

    public async Task<string?> GetMoviePosterKeyAsync(int movieId, CancellationToken ct = default)
    {
        var movie = await _movies.FindAsync(movieId, ct);
        if (movie == null || string.IsNullOrEmpty(movie.PosterUrl)) return null;
        return _storage.TryParseObjectKey(movie.PosterUrl, out var key) ? key : null;
    }

    public async Task<string?> GetSeriesPosterKeyAsync(int seriesId, CancellationToken ct = default)
    {
        var series = await _series.FindAsync(seriesId, ct);
        if (series == null || string.IsNullOrEmpty(series.PosterUrl)) return null;
        return _storage.TryParseObjectKey(series.PosterUrl, out var key) ? key : null;
    }

    public async Task<string?> GetActorPhotoKeyAsync(int actorId, CancellationToken ct = default)
    {
        var actor = await _actors.FindAsync(actorId, ct);
        if (actor == null || string.IsNullOrEmpty(actor.PhotoUrl)) return null;
        return _storage.TryParseObjectKey(actor.PhotoUrl, out var key) ? key : null;
    }

    public async Task<(string Key, string? VideoUrl)?> GetMovieVideoAsync(int id, CancellationToken ct = default)
    {
        var movie = await _movies.FindAsync(id, ct);
        if (movie == null || string.IsNullOrEmpty(movie.VideoUrl)) return null;
        return _storage.TryParseObjectKey(movie.VideoUrl, out var key) ? (key, movie.VideoUrl) : null;
    }

    public async Task<(string Key, string? VideoUrl)?> GetEpisodeVideoAsync(int episodeId, CancellationToken ct = default)
    {
        var episode = await _series.FindEpisodeAsync(episodeId, ct);
        if (episode == null || string.IsNullOrEmpty(episode.VideoUrl)) return null;
        return _storage.TryParseObjectKey(episode.VideoUrl, out var key) ? (key, episode.VideoUrl) : null;
    }
}