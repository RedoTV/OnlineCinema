using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Services;

namespace OnlineCinema.Backend.Controllers;

/// <summary>
/// Отдаёт presigned URL для прямой стриминговой отдачи видео с MinIO.
/// Браузер качает напрямую, бэкенд не прогоняет через себя байты.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storage;

    public StreamingController(ApplicationDbContext context, IStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    [HttpGet("movie/{id}")]
    public async Task<IActionResult> GetMovieStreamUrl(int id)
    {
        var movie = await _context.Movies.FindAsync(id);
        if (movie == null) return NotFound();
        if (string.IsNullOrEmpty(movie.VideoUrl)) return NotFound("Video not uploaded yet");

        if (!_storage.TryParseObjectKey(movie.VideoUrl, out var key))
            return BadRequest("Broken video reference");

        var url = _storage.GetPresignedUrl(key, 3600);
        return Ok(new { url });
    }

    [HttpGet("episode/{id}")]
    public async Task<IActionResult> GetEpisodeStreamUrl(int id)
    {
        var episode = await _context.Episodes.FindAsync(id);
        if (episode == null) return NotFound();
        if (string.IsNullOrEmpty(episode.VideoUrl)) return NotFound("Video not uploaded yet");

        if (!_storage.TryParseObjectKey(episode.VideoUrl, out var key))
            return BadRequest("Broken video reference");

        var url = _storage.GetPresignedUrl(key, 3600);
        return Ok(new { url });
    }
}
