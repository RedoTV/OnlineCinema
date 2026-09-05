using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.UserActions;

namespace OnlineCinema.Backend.Repositories.UserActions;

public interface IUserActionRepository
{
    Task<UserMovieStatus?> FindStatusAsync(int userId, int movieId, CancellationToken ct = default);
    void AddStatus(UserMovieStatus status);
    Task<Rating?> FindRatingAsync(int userId, int? movieId, int? seriesId, CancellationToken ct = default);
    void AddRating(Rating rating);
    void RemoveRating(Rating rating);
    Task<List<UserMovieStatus>> GetUserMoviesAsync(int userId, string? status, CancellationToken ct = default);
}