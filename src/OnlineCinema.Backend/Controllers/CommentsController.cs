using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Models.DTOs.Comments;
using OnlineCinema.Backend.Services;
using System.Security.Claims;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    private int? GetUserIdOrNull()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments([FromQuery] int? movieId, [FromQuery] int? seriesId, [FromQuery] string? sort)
    {
        var userId = GetUserIdOrNull();
        return Ok(await _commentService.GetCommentsAsync(movieId, seriesId, sort, userId));
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto)
    {
        var userId = GetUserIdOrNull();
        if (userId == null) return Unauthorized();

        try
        {
            return Ok(await _commentService.AddAsync(userId.Value, dto));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/like"), Authorize]
    public async Task<IActionResult> ToggleLike(int id)
    {
        var userId = GetUserIdOrNull();
        if (userId == null) return Unauthorized();

        await _commentService.ToggleLikeAsync(id, userId.Value);
        return Ok();
    }

    [HttpPost("{id}/hide"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> HideComment(int id, [FromQuery] bool hide = true)
    {
        try
        {
            await _commentService.HideAsync(id, hide);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        await _commentService.DeleteAsync(id);
        return NoContent();
    }
}
