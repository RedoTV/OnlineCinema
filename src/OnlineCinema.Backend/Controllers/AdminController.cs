using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminController(ApplicationDbContext db) => _db = db;

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var ratings = await _db.Ratings.AsNoTracking().Select(x => x.RatingValue).ToListAsync();
        return Ok(new
        {
            movies = await _db.Movies.CountAsync(),
            series = await _db.Series.CountAsync(),
            actors = await _db.Actors.CountAsync(),
            genres = await _db.Genres.CountAsync(),
            users = await _db.Users.CountAsync(),
            comments = await _db.Comments.CountAsync(),
            hiddenComments = await _db.Comments.CountAsync(x => x.IsHidden),
            ratings = ratings.Count,
            averageRating = ratings.Count == 0 ? 0 : Math.Round(ratings.Average(), 2)
        });
    }

    [HttpGet("comments")]
    public async Task<IActionResult> Comments()
    {
        var items = await _db.Comments.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Movie)
            .Include(x => x.Series)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Text,
                x.IsHidden,
                x.CreatedAt,
                x.ParentId,
                username = x.User.Username,
                target = x.Movie != null ? x.Movie.Title : x.Series != null ? x.Series.Title : "—"
            })
            .ToListAsync();
        return Ok(items);
    }

    [HttpGet("ratings")]
    public async Task<IActionResult> Ratings()
    {
        var items = await _db.Ratings.AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Movie)
            .Include(x => x.Series)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new
            {
                x.Id,
                x.RatingValue,
                x.CreatedAt,
                username = x.User.Username,
                target = x.Movie != null ? x.Movie.Title : x.Series != null ? x.Series.Title : "—",
                contentType = x.MovieId != null ? "Фильм" : "Сериал"
            })
            .ToListAsync();
        return Ok(items);
    }
}
