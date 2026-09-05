using OnlineCinema.Backend.Models.DTOs.Auth;

namespace OnlineCinema.Backend.Services.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}