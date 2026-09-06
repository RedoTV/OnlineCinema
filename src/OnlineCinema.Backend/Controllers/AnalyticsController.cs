using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Services.Analytics;
using OnlineCinema.Backend.Services.Stats;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    private readonly IStatsService _stats;

    public AnalyticsController(IAnalyticsService analytics, IStatsService stats)
    {
        _analytics = analytics;
        _stats = stats;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> Overview() =>
        Ok(await _analytics.GetOverviewAsync(HttpContext.RequestAborted));

    [HttpGet("trending")]
    public async Task<IActionResult> Trending([FromQuery] int days = 7, [FromQuery] int count = 12) =>
        Ok(await _analytics.GetTrendingAsync(TimeSpan.FromDays(days), count, HttpContext.RequestAborted));

    [HttpGet("activity")]
    public async Task<IActionResult> Activity([FromQuery] int? userId = null, [FromQuery] int limit = 30) =>
        Ok(await _analytics.GetActivityAsync(userId, limit, HttpContext.RequestAborted));

    [HttpGet("picks/{userId:int}")]
    public async Task<IActionResult> Picks(int userId, [FromQuery] int count = 8) =>
        Ok(await _analytics.GetPicksAsync(userId, count, HttpContext.RequestAborted));

    [HttpGet("top-rated")]
    public async Task<IActionResult> TopRated([FromQuery] int count = 10) =>
        Ok(await _stats.GetTopRatedAsync(count, HttpContext.RequestAborted));

    [HttpGet("genres")]
    public async Task<IActionResult> Genres() =>
        Ok(await _stats.GetGenreStatsAsync(HttpContext.RequestAborted));
}
