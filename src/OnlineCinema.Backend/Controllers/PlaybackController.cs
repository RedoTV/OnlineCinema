using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Models.DTOs;
using OnlineCinema.Backend.Services.Playback;
using System.Security.Claims;

namespace OnlineCinema.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PlaybackController : ControllerBase
{
    private readonly IPlaybackService _playbackService;

    public PlaybackController(IPlaybackService playbackService)
    {
        _playbackService = playbackService;
    }

    private int GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(idClaim, out var userId) ? userId : throw new UnauthorizedAccessException();
    }

    [HttpPost("progress")]
    public async Task<IActionResult> SaveProgress([FromBody] SaveProgressDto dto)
    {
        var userId = GetUserId();
        await _playbackService.SaveProgressAsync(userId, dto.MovieId, dto.EpisodeId, dto.PositionSeconds, dto.DurationSeconds);
        return Ok();
    }

    [HttpGet("continue-watching")]
    public async Task<IActionResult> GetContinueWatching()
    {
        var userId = GetUserId();
        var items = await _playbackService.GetContinueWatchingAsync(userId);

        return Ok(items.Select(p => new
        {
            p.Id,
            p.MovieId,
            p.EpisodeId,
            p.PositionSeconds,
            p.DurationSeconds,
            movieTitle = p.Movie?.Title,
            episodeTitle = p.Episode != null ? $"{p.Episode.Season.Series.Title} С{p.Episode.Season.SeasonNumber}Э{p.Episode.EpisodeNumber}" : null
        }));
    }

    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress([FromQuery] int? movieId, [FromQuery] int? episodeId)
    {
        var userId = GetUserId();
        var progress = await _playbackService.GetProgressAsync(userId, movieId, episodeId);
        if (progress == null) return Ok(new { positionSeconds = 0 });
        return Ok(new { progress.PositionSeconds });
    }
}
