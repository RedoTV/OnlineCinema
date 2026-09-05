using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models.Enums;

namespace OnlineCinema.Backend.Controllers;

// Базовая статистика читает данные напрямую из основной БД.
// Событийные агрегаты и рекомендации доступны через analytics-сервис.
[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StatsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("top-rated")]
    public async Task<IActionResult> TopRated([FromQuery] int count = 10)
    {
        var movies = await _context.Movies
            .Include(m => m.Ratings)
            .Where(m => m.Ratings.Any())
            .OrderByDescending(m => m.Ratings.Average(r => r.RatingValue))
            .Take(count)
            .Select(m => new
            {
                m.Id,
                m.Title,
                avgRating = m.Ratings.Average(r => r.RatingValue),
                ratingCount = m.Ratings.Count
            })
            .ToListAsync();

        return Ok(movies);
    }

    [HttpGet("most-watched")]
    public async Task<IActionResult> MostWatched([FromQuery] int count = 10)
    {
        // считаем по статусу Watched у юзеров
        var movies = await _context.UserMovieStatuses
            .Where(s => s.Status == MovieStatus.Watched)
            .GroupBy(s => s.MovieId)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => new
            {
                movieId = g.Key,
                views = g.Count(),
                title = _context.Movies.Where(m => m.Id == g.Key).Select(m => m.Title).FirstOrDefault()
            })
            .ToListAsync();

        return Ok(movies);
    }

    [HttpGet("genres")]
    public async Task<IActionResult> GenreStats()
    {
        // сколько фильмов в каждом жанре
        var stats = await _context.Genres
            .Select(g => new
            {
                genre = g.Name,
                movieCount = g.Movies.Count
            })
            .OrderByDescending(x => x.movieCount)
            .ToListAsync();

        return Ok(stats);
    }
}
