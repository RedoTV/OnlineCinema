using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Models.DTOs;
using OnlineCinema.Backend.Services.Watches;
using System.Security.Claims;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WatchController : ControllerBase
{
    private readonly IWatchService _watchService;

    public WatchController(IWatchService watchService)
    {
        _watchService = watchService;
    }

    private int? GetUserIdOrNull()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }

    // Единственный, «смысловой» эвент за просмотр: плеер шлёт его один раз
    // на контент, когда зритель реально досмотрел (конец, >=85% или 3 минуты
    // честного просмотра). Эндпоинт открыт и для гостей: анонимный просмотр
    // различается по ViewerKey из тела запроса.
    [HttpPost("report")]
    [AllowAnonymous]
    public async Task<IActionResult> Report([FromBody] ReportWatchDto dto)
    {
        var userId = GetUserIdOrNull();
        var ok = await _watchService.ReportAsync(userId, dto.ViewerKey, dto.MovieId, dto.EpisodeId, dto.WatchedSeconds, HttpContext.RequestAborted);
        return ok ? Ok() : BadRequest();
    }
}
