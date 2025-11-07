using OnlineCinema.Backend.Models.DTOs;

namespace OnlineCinema.Backend.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}