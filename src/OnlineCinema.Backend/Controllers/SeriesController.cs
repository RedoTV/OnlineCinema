using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Models.DTOs.Series;
using OnlineCinema.Backend.Services;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeriesController : ControllerBase
{
    private readonly ISeriesService _seriesService;

    public SeriesController(ISeriesService seriesService)
    {
        _seriesService = seriesService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SeriesDto>>> GetSeries([FromQuery] string? search, [FromQuery] int? genreId)
    {
        return Ok(await _seriesService.GetAllAsync(search, genreId));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SeriesDto>> GetSeriesById(int id)
    {
        var series = await _seriesService.GetByIdAsync(id);
        if (series == null) return NotFound();
        return Ok(series);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<ActionResult<SeriesDto>> CreateSeries([FromBody] CreateSeriesDto dto)
    {
        var created = await _seriesService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetSeriesById), new { id = created.Id }, created);
    }

    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<SeriesDto>> UpdateSeries(int id, [FromBody] UpdateSeriesDto dto)
    {
        try
        {
            return Ok(await _seriesService.UpdateAsync(id, dto));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/seasons"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<SeasonDto>> AddSeason(int id, [FromBody] CreateSeasonDto dto)
    {
        try
        {
            return Ok(await _seriesService.AddSeasonAsync(id, dto));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("seasons/{seasonId}/episodes"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<EpisodeDto>> AddEpisode(int seasonId, [FromBody] CreateEpisodeDto dto)
    {
        try
        {
            return Ok(await _seriesService.AddEpisodeAsync(seasonId, dto));
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
            return Ok(new { Url = await _seriesService.UploadPosterAsync(id, file) });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("episodes/{episodeId}/upload-video"), Authorize(Roles = "Admin"), DisableRequestSizeLimit]
    public async Task<IActionResult> UploadEpisodeVideo(int episodeId, IFormFile file)
    {
        try
        {
            return Ok(new { Url = await _seriesService.UploadEpisodeVideoAsync(episodeId, file) });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteSeries(int id)
    {
        try
        {
            await _seriesService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
