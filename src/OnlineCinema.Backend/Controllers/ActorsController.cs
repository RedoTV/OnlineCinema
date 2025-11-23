using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Models.DTOs.Actors;
using OnlineCinema.Backend.Services;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActorsController : ControllerBase
{
    private readonly IActorService _actorService;

    public ActorsController(IActorService actorService)
    {
        _actorService = actorService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActorDto>>> GetActors()
    {
        return Ok(await _actorService.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActorDto>> GetActor(int id)
    {
        var actor = await _actorService.GetByIdAsync(id);
        if (actor == null) return NotFound();
        return Ok(actor);
    }

    [HttpPost, Authorize(Roles = "Admin")]
    public async Task<ActionResult<ActorDto>> CreateActor(CreateActorDto dto)
    {
        var newActor = await _actorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetActor), new { id = newActor.Id }, newActor);
    }

    [HttpPut("{id}"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<ActorDto>> UpdateActor(int id, UpdateActorDto dto)
    {
        try
        {
            var updatedActor = await _actorService.UpdateAsync(id, dto);
            return Ok(updatedActor);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/upload-photo"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        try
        {
            var url = await _actorService.UploadPhotoAsync(id, file);
            return Ok(new { Url = url });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteActor(int id)
    {
        try
        {
            await _actorService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}