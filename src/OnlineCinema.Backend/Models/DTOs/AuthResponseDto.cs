namespace OnlineCinema.Backend.Models.DTOs;

public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
}