using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCinema.Backend.Services.Admin;

namespace OnlineCinema.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin) => _admin = admin;

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() =>
        Ok(await _admin.GetDashboardAsync(HttpContext.RequestAborted));

    [HttpGet("comments")]
    public async Task<IActionResult> Comments() =>
        Ok(await _admin.GetCommentsAsync(HttpContext.RequestAborted));

    [HttpGet("ratings")]
    public async Task<IActionResult> Ratings() =>
        Ok(await _admin.GetRatingsAsync(HttpContext.RequestAborted));
}