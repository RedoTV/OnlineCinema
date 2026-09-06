using AutoMapper;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.UserActions;
using OnlineCinema.Backend.Models.Enums;
using OnlineCinema.Backend.Repositories.UnitOfWork;
using OnlineCinema.Backend.Repositories.UserActions;

namespace OnlineCinema.Backend.Services.UsersActions;

public class UserActionService : IUserActionService
{
    private readonly IUserActionRepository _actions;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UserActionService(IUserActionRepository actions, IUnitOfWork uow, IMapper mapper)
    {
        _actions = actions;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task SetStatusAsync(int userId, SetStatusDto dto)
    {
        if (!Enum.TryParse<MovieStatus>(dto.Status, true, out var statusEnum))
            throw new ArgumentException($"Invalid status value: {dto.Status}");

        var existingStatus = await _actions.FindStatusAsync(userId, dto.MovieId);

        if (existingStatus != null)
        {
            existingStatus.Status = statusEnum;
            existingStatus.AddedAt = DateTime.UtcNow;
        }
        else
        {
            _actions.AddStatus(new UserMovieStatus
            {
                UserId = userId,
                MovieId = dto.MovieId,
                Status = statusEnum,
                AddedAt = DateTime.UtcNow
            });
        }
        await _uow.SaveChangesAsync();
    }

    public async Task SetRatingAsync(int userId, SetRatingDto dto)
    {
        if (dto.Rating < 1 || dto.Rating > 10)
            throw new ArgumentException("Rating must be between 1 and 10");

        var existingRating = await _actions.FindRatingAsync(userId, dto.MovieId, dto.SeriesId);

        if (existingRating != null)
        {
            existingRating.RatingValue = dto.Rating;
            existingRating.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            _actions.AddRating(new Rating
            {
                UserId = userId,
                MovieId = dto.MovieId,
                SeriesId = dto.SeriesId,
                RatingValue = dto.Rating,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await _uow.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserMovieDto>> GetUserMoviesAsync(int userId, string? statusStr)
    {
        var userMovieStatuses = await _actions.GetUserMoviesAsync(userId, statusStr);
        return _mapper.Map<IEnumerable<UserMovieDto>>(userMovieStatuses);
    }
}