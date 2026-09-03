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

    public MediaController(ApplicationDbContext context, IStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    // Отдаёт presigned URL для постера. Браузер ходит напрямую в MinIO.
    [HttpGet("poster/{movieId}")]
    public async Task<IActionResult> GetPosterUrl(int movieId)
    {
        var movie = await _context.Movies.FindAsync(movieId);
        if (movie == null || string.IsNullOrEmpty(movie.PosterUrl)) return NotFound();

        if (!_storage.TryParseObjectKey(movie.PosterUrl, out var key)) return BadRequest();
        return Redirect(_storage.GetPresignedUrl(key, 3600));
    }

    [HttpGet("poster/actor/{actorId}")]
    public async Task<IActionResult> GetActorPhotoUrl(int actorId)
    {
        var actor = await _context.Actors.FindAsync(actorId);
        if (actor == null || string.IsNullOrEmpty(actor.PhotoUrl)) return NotFound();

        if (!_storage.TryParseObjectKey(actor.PhotoUrl, out var key)) return BadRequest();
        return Redirect(_storage.GetPresignedUrl(key, 3600));
    }

    [HttpGet("poster/series/{seriesId}")]
    public async Task<IActionResult> GetSeriesPosterUrl(int seriesId)
    {
        var series = await _context.Series.FindAsync(seriesId);
        if (series == null || string.IsNullOrEmpty(series.PosterUrl)) return NotFound();

        if (!_storage.TryParseObjectKey(series.PosterUrl, out var key)) return BadRequest();
        return Redirect(_storage.GetPresignedUrl(key, 3600));
    }
}
