using OnlineCinema.Backend.Models.DTOs.Auth;

namespace OnlineCinema.Backend.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}