using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Movies;
using OnlineCinema.Backend.Services;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies([FromQuery] string? search, [FromQuery] int? genreId)
    {
        var movies = await _movieService.GetAllAsync(search, genreId);
        return Ok(movies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovieDto>> GetMovie(int id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        if (movie == null) return NotFound();
        return Ok(movie);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<ActionResult<MovieDto>> CreateMovie([FromBody] CreateMovieDto dto)
    {
        var createdMovie = await _movieService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetMovie), new { id = createdMovie.Id }, createdMovie);
    }

    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<MovieDto>> UpdateMovie(int id, [FromBody] UpdateMovieDto dto)
    {
        try
        {
            var updatedMovie = await _movieService.UpdateAsync(id, dto);
            return Ok(updatedMovie);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/upload-poster"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadPoster(int id, IFormFile file)
    {
        try
        {
            var url = await _movieService.UploadPosterAsync(id, file);
            return Ok(new { Url = url });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/upload-video"), Authorize(Roles = "Admin"), DisableRequestSizeLimit]
    public async Task<IActionResult> UploadVideo(int id, IFormFile file)
    {
        try
        {
            var url = await _movieService.UploadVideoAsync(id, file);
            return Ok(new { Url = url });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        try
        {
            await _movieService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
