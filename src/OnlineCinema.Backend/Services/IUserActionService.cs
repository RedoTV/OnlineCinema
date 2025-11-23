using OnlineCinema.Backend.Models.DTOs.UserActions;

namespace OnlineCinema.Backend.Services;

public interface IUserActionService
{
    Task SetStatusAsync(int userId, SetStatusDto dto);
    Task<IEnumerable<UserMovieDto>> GetUserMoviesAsync(int userId, string? statusStr);
    Task SetRatingAsync(int userId, SetRatingDto dto);
}