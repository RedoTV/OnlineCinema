using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Models.DTOs;
using OnlineCinema.Backend.Services.Watches;
using System.Security.Claims;

namespace OnlineCinema.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class WatchController : ControllerBase
{
    private readonly IWatchService _watchService;

    public WatchController(IWatchService watchService)
    {
        _watchService = watchService;
    }

    private int GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(idClaim, out var userId) ? userId : throw new UnauthorizedAccessException();
    }

    // Единственный, «смысловой» эвент за просмотр: плеер шлёт его один раз
    // на контент, когда пользователь реально досмотрел (конец или >=85%).
    [HttpPost("report")]
    public async Task<IActionResult> Report([FromBody] ReportWatchDto dto)
    {
        var userId = GetUserId();
        var ok = await _watchService.ReportAsync(userId, dto.MovieId, dto.EpisodeId, dto.WatchedSeconds, HttpContext.RequestAborted);
        return ok ? Ok() : BadRequest();
    }
}
