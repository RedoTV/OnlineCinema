using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Models.DTOs.UserActions;
using OnlineCinema.Backend.Services.UsersActions;
using System.Security.Claims;

namespace OnlineCinema.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserActionsController : ControllerBase
{
    private readonly IUserActionService _userActionService;

    public UserActionsController(IUserActionService userActionService)
    {
        _userActionService = userActionService;
    }

    private int GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing User ID in token");
        }

        return userId;
    }

    [HttpPost("status")]
    public async Task<IActionResult> SetStatus([FromBody] SetStatusDto dto)
    {
        try
        {
            var userId = GetUserId();
            await _userActionService.SetStatusAsync(userId, dto);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("my-movies")]
    public async Task<ActionResult<IEnumerable<UserMovieDto>>> GetMyMovies([FromQuery] string? status)
    {
        var userId = GetUserId();
        var movies = await _userActionService.GetUserMoviesAsync(userId, status);
        return Ok(movies);
    }

    [HttpPost("rating")]
    public async Task<IActionResult> SetRating([FromBody] SetRatingDto dto)
    {
        try
        {
            var userId = GetUserId();
            await _userActionService.SetRatingAsync(userId, dto);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}