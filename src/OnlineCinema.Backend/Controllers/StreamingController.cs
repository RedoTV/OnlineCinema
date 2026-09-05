using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Services.Media;
using OnlineCinema.Backend.Services.Storage;

namespace OnlineCinema.Backend.Controllers;

/// <summary>
/// Отдаёт presigned URL для прямой стриминговой отдачи видео с MinIO.
/// Браузер качает напрямую, бэкенд не прогоняет через себя байты.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StreamingController : ControllerBase
{
    private readonly IMediaService _media;
    private readonly IStorageService _storage;

    public StreamingController(IMediaService media, IStorageService storage)
    {
        _media = media;
        _storage = storage;
    }

    [HttpGet("movie/{id}")]
    public async Task<IActionResult> GetMovieStreamUrl(int id)
    {
        var video = await _media.GetMovieVideoAsync(id, HttpContext.RequestAborted);
        if (video == null) return NotFound("Video not uploaded yet");

        if (!_storage.TryParseObjectKey(video.Value.VideoUrl, out var key))
            return BadRequest("Broken video reference");

        var url = _storage.BuildBrowserMediaUrl(key, 3600);
        return Ok(new { url });
    }

    [HttpGet("episode/{id}")]
    public async Task<IActionResult> GetEpisodeStreamUrl(int id)
    {
        var video = await _media.GetEpisodeVideoAsync(id, HttpContext.RequestAborted);
        if (video == null) return NotFound("Video not uploaded yet");

        if (!_storage.TryParseObjectKey(video.Value.VideoUrl, out var key))
            return BadRequest("Broken video reference");

        var url = _storage.BuildBrowserMediaUrl(key, 3600);
        return Ok(new { url });
    }
}