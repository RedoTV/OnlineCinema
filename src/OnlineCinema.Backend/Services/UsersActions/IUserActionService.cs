using OnlineCinema.Backend.Models.DTOs.UserActions;

namespace OnlineCinema.Backend.Services.UsersActions;

public interface IUserActionService
{
    Task SetStatusAsync(int userId, SetStatusDto dto);
    Task SetRatingAsync(int userId, SetRatingDto dto);
    Task<IEnumerable<UserMovieDto>> GetUserMoviesAsync(int userId, string? statusStr);
}