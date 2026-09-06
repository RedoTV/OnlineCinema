using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Services.Stats;

namespace OnlineCinema.Backend.Controllers;

// Базовая статистика каталога. Событийные тренды/активность/подборки — в AnalyticsController.
[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _stats;

    public StatsController(IStatsService stats)
    {
        _stats = stats;
    }

    [HttpGet("top-rated")]
    public async Task<IActionResult> TopRated([FromQuery] int count = 10) =>
        Ok(await _stats.GetTopRatedAsync(count, HttpContext.RequestAborted));

    [HttpGet("most-watched")]
    public async Task<IActionResult> MostWatched([FromQuery] int count = 10) =>
        Ok(await _stats.GetMostWatchedAsync(count, HttpContext.RequestAborted));

    [HttpGet("genres")]
    public async Task<IActionResult> GenreStats() =>
        Ok(await _stats.GetGenreStatsAsync(HttpContext.RequestAborted));
}