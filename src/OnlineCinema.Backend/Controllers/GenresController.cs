using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Models.DTOs.Genres;
using OnlineCinema.Backend.Services.Genres;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController : ControllerBase
{
    private readonly IGenreService _genreService;

    public GenresController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GenreDto>>> GetGenres()
    {
        return Ok(await _genreService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GenreDto>> GetGenre(int id)
    {
        var genre = await _genreService.GetByIdAsync(id);
        if (genre == null) return NotFound();
        return Ok(genre);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<ActionResult<GenreDto>> CreateGenre(CreateGenreDto dto)
    {
        var newGenre = await _genreService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetGenre), new { id = newGenre.Id }, newGenre);
    }

    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<GenreDto>> UpdateGenre(int id, UpdateGenreDto dto)
    {
        try
        {
            var updatedGenre = await _genreService.UpdateAsync(id, dto);
            return Ok(updatedGenre);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        try
        {
            await _genreService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}