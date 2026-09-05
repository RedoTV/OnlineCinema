using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Services;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storage;
    private readonly IPosterPreviewService _previews;

    public MediaController(ApplicationDbContext context, IStorageService storage, IPosterPreviewService previews)
    {
        _context = context;
        _storage = storage;
        _previews = previews;
    }

    // size=preview отдаёт уменьшенную копию для регулярного просмотра сетки
    // (320px по ширине, JPEG q60). Без параметра — оригинал редиректом в MinIO.
    [HttpGet("poster/{movieId}")]
    public async Task<IActionResult> GetPosterUrl(int movieId, [FromQuery] string? size = null)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null || string.IsNullOrEmpty(movie.PosterUrl)) return NotFound();

        if (!_storage.TryParseObjectKey(movie.PosterUrl, out var key)) return BadRequest();
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
        var actor = await _context.Actors.FindAsync(actorId);
        if (actor == null || string.IsNullOrEmpty(actor.PhotoUrl)) return NotFound();

        if (!_storage.TryParseObjectKey(actor.PhotoUrl, out var key)) return BadRequest();
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
        var series = await _context.Series.FindAsync(seriesId);
        if (series == null || string.IsNullOrEmpty(series.PosterUrl)) return NotFound();

        if (!_storage.TryParseObjectKey(series.PosterUrl, out var key)) return BadRequest();
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
