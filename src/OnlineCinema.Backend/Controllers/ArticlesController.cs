using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Models.DTOs.Articles;
using OnlineCinema.Backend.Services.Articles;
using System.Security.Claims;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articles;

    public ArticlesController(IArticleService articles) => _articles = articles;

    private int? GetUserIdOrNull()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }

    // Публичный: только опубликованные статьи (all=true — все, только для персонала).
    [HttpGet]
    public async Task<IActionResult> GetPublished([FromQuery] bool all = false)
    {
        if (all && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
            return Forbid();
        return Ok(all
            ? await _articles.GetAllAsync(HttpContext.RequestAborted)
            : await _articles.GetPublishedAsync(HttpContext.RequestAborted));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var article = await _articles.GetByIdAsync(id, HttpContext.RequestAborted);
        if (article == null) return NotFound();
        if (!article.IsPublished && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
            return NotFound();
        return Ok(article);
    }

    // Любой авторизованный пользователь может предложить статью (она не публикуется сразу).
    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] CreateArticleDto dto)
    {
        var userId = GetUserIdOrNull();
        if (userId == null) return Unauthorized();

        try
        {
            var created = await _articles.CreateAsync(userId.Value, dto, HttpContext.RequestAborted);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Модерация: опубликовать/скрыть/удалить.
    [HttpPost("{id:int}/publish"), Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Publish(int id, [FromQuery] bool publish = true)
    {
        var ok = await _articles.SetPublishedAsync(id, publish, HttpContext.RequestAborted);
        return ok ? Ok() : NotFound();
    }

    [HttpDelete("{id:int}"), Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _articles.DeleteAsync(id, HttpContext.RequestAborted);
        return ok ? NoContent() : NotFound();
    }
}
