using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Services.Media;
using OnlineCinema.Backend.Services.Storage;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediaService _media;
    private readonly IStorageService _storage;
    private readonly IPosterPreviewService _previews;

    public MediaController(IMediaService media, IStorageService storage, IPosterPreviewService previews)
    {
        _media = media;
        _storage = storage;
        _previews = previews;
    }

    // size=preview отдаёт уменьшенную копию для регулярного просмотра сетки
    // (320px по ширине, JPEG q60). Без параметра — оригинал редиректом в MinIO.
    [HttpGet("poster/{movieId}")]
    public async Task<IActionResult> GetPosterUrl(int movieId, [FromQuery] string? size = null)
    {
        var key = await _media.GetMoviePosterKeyAsync(movieId, HttpContext.RequestAborted);
        if (key == null) return NotFound();

        if (IsPreview(size))
        {
            var (stream, contentType, _) = await _previews.GetPreviewAsync(key, HttpContext.RequestAborted);
            return File(stream, contentType);
        }
        return Redirect(_storage.BuildBrowserMediaUrl(key, 3600));
    }

    [HttpGet("poster/actor/{actorId}")]
    public async Task<IActionResult> GetActorPhotoUrl(int actorId, [FromQuery] string? size = null)
    {
        var key = await _media.GetActorPhotoKeyAsync(actorId, HttpContext.RequestAborted);
        if (key == null) return NotFound();

        if (IsPreview(size))
        {
            var (stream, contentType, _) = await _previews.GetPreviewAsync(key, HttpContext.RequestAborted);
            return File(stream, contentType);
        }
        return Redirect(_storage.BuildBrowserMediaUrl(key, 3600));
    }

    [HttpGet("poster/series/{seriesId}")]
    public async Task<IActionResult> GetSeriesPosterUrl(int seriesId, [FromQuery] string? size = null)
    {
        var key = await _media.GetSeriesPosterKeyAsync(seriesId, HttpContext.RequestAborted);
        if (key == null) return NotFound();

        if (IsPreview(size))
        {
            var (stream, contentType, _) = await _previews.GetPreviewAsync(key, HttpContext.RequestAborted);
            return File(stream, contentType);
        }
        return Redirect(_storage.BuildBrowserMediaUrl(key, 3600));
    }

    private static bool IsPreview(string? size) =>
        string.Equals(size, "preview", StringComparison.OrdinalIgnoreCase);
}