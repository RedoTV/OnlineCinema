using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Services;

/// <summary>
/// Idempotently imports the coursework sample library into MinIO and assigns every file.
/// Existing user uploads are never overwritten; repeat starts only fill missing media.
/// </summary>
public sealed class SampleMediaImporter
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SampleMediaImporter> _logger;

    public SampleMediaImporter(ApplicationDbContext db, IStorageService storage, IConfiguration configuration, ILogger<SampleMediaImporter> logger)
    {
        _db = db;
        _storage = storage;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ImportAsync(CancellationToken ct = default)
    {
        var root = _configuration["SampleMedia:Path"] ?? Path.Combine(AppContext.BaseDirectory, "samples");
        var posterDirectory = Path.Combine(root, "posters");
        var filmDirectory = Path.Combine(root, "films");
        if (!Directory.Exists(posterDirectory) && !Directory.Exists(filmDirectory))
        {
            _logger.LogWarning("Sample media directory {Root} does not exist; import skipped", root);
            return;
        }

        var movies = await _db.Movies.OrderBy(x => x.Id).ToListAsync(ct);
        var series = await _db.Series.OrderBy(x => x.Id).ToListAsync(ct);
        var posters = Directory.Exists(posterDirectory)
            ? Directory.EnumerateFiles(posterDirectory).Where(IsImage).OrderBy(Path.GetFileName).ToArray()
            : Array.Empty<string>();
        var videos = Directory.Exists(filmDirectory)
            ? Directory.EnumerateFiles(filmDirectory).Where(IsVideo).OrderBy(Path.GetFileName).ToArray()
            : Array.Empty<string>();

        if (movies.Count + series.Count > 0 && posters.Length > 0)
        {
            var targets = movies.Select(m => new PosterTarget(m.Id, "movie", () => m.PosterUrl, value => m.PosterUrl = value))
                .Concat(series.Select(s => new PosterTarget(s.Id, "series", () => s.PosterUrl, value => s.PosterUrl = value)))
                .ToArray();

            for (var index = 0; index < targets.Length; index++)
            {
                var target = targets[index];
                // Prefer real coursework artwork over generated demo SVG posters.
                if (!string.IsNullOrWhiteSpace(target.Current()) && !target.Current()!.Contains("posters/demo/", StringComparison.OrdinalIgnoreCase)) continue;
                var source = posters[(index * 7 + target.Id) % posters.Length]; // deterministic spread, visually random across both types
                var key = $"samples/posters/{target.Kind}-{target.Id}-{Path.GetFileName(source)}";
                await using var stream = File.OpenRead(source);
                target.Assign(await _storage.SaveAsync(stream, key, ImageContentType(source), ct));
            }
        }

        // Every supplied film is uploaded and assigned once. Known names map to matching titles;
        // remaining files fill missing movies, then missing episodes so series upload can be demonstrated.
        var availableMovies = movies.Where(x => string.IsNullOrWhiteSpace(x.VideoUrl)).ToList();
        var episodes = await _db.Episodes.OrderBy(x => x.Id).ToListAsync(ct);
        var assignedKeys = movies.Select(x => x.VideoUrl)
            .Concat(episodes.Select(x => x.VideoUrl))
            .Where(x => _storage.TryParseObjectKey(x, out _))
            .Select(x => { _storage.TryParseObjectKey(x, out var key); return key; })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var aliases = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Avengers"] = new[] { "Мстители", "Avengers" },
            ["Interception"] = new[] { "Начало", "Inception", "Interception" },
            ["John_Wick"] = new[] { "Джон Уик", "John Wick" }
        };
        foreach (var source in videos)
        {
            var key = $"samples/films/{Path.GetFileName(source)}";
            if (assignedKeys.Contains(key)) continue;

            var stem = Path.GetFileNameWithoutExtension(source);
            var candidates = aliases.TryGetValue(stem, out var known) ? known : new[] { stem };
            var movie = availableMovies.FirstOrDefault(x => candidates.Any(candidate =>
                Normalize(x.Title).Contains(Normalize(candidate)) || Normalize(candidate).Contains(Normalize(x.Title))))
                ?? availableMovies.FirstOrDefault();
            await using var stream = File.OpenRead(source);
            var stored = await _storage.SaveAsync(stream, key, VideoContentType(source), ct);
            if (movie != null)
            {
                movie.VideoUrl = stored;
                availableMovies.Remove(movie);
            }
            else
            {
                var episode = episodes.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.VideoUrl));
                if (episode != null) episode.VideoUrl = stored;
            }
            assignedKeys.Add(key);
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Imported {PosterCount} sample posters and {VideoCount} sample films", posters.Length, videos.Length);
    }

    private sealed record PosterTarget(int Id, string Kind, Func<string?> Current, Action<string> Assign);
    private static string Normalize(string value) => new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
    private static bool IsImage(string path) => new[] { ".jpg", ".jpeg", ".png", ".webp", ".svg" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
    private static bool IsVideo(string path) => new[] { ".mp4", ".webm", ".ogg", ".mkv", ".mov" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
    private static string ImageContentType(string path) => Path.GetExtension(path).ToLowerInvariant() switch { ".png" => "image/png", ".webp" => "image/webp", ".svg" => "image/svg+xml", _ => "image/jpeg" };
    private static string VideoContentType(string path) => Path.GetExtension(path).ToLowerInvariant() switch { ".webm" => "video/webm", ".ogg" => "video/ogg", ".mkv" => "video/x-matroska", _ => "video/mp4" };
}
