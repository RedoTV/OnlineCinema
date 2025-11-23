using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.UserActions;
using OnlineCinema.Backend.Models.Enums;

namespace OnlineCinema.Backend.Services;

public class UserActionService : IUserActionService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UserActionService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task SetStatusAsync(int userId, SetStatusDto dto)
    {
        if (!Enum.TryParse<MovieStatus>(dto.Status, true, out var statusEnum))
            throw new ArgumentException($"Invalid status value: {dto.Status}");

        var existingStatus = await _context.UserMovieStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId && s.MovieId == dto.MovieId);

        if (existingStatus != null)
        {
            existingStatus.Status = statusEnum;
            existingStatus.AddedAt = DateTime.UtcNow;
        }
        else
        {
            var newStatus = new UserMovieStatus
            {
                UserId = userId,
                MovieId = dto.MovieId,
                Status = statusEnum,
                AddedAt = DateTime.UtcNow
            };
            _context.UserMovieStatuses.Add(newStatus);
        }
        await _context.SaveChangesAsync();
    }

    public async Task SetRatingAsync(int userId, SetRatingDto dto)
    {
        if (dto.Rating < 1 || dto.Rating > 10)
            throw new ArgumentException("Rating must be between 1 and 10");

        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == dto.MovieId);

        if (existingRating != null)
        {
            existingRating.RatingValue = dto.Rating;
            existingRating.Review = dto.Review;
            existingRating.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var newRating = new Rating
            {
                UserId = userId,
                MovieId = dto.MovieId,
                RatingValue = dto.Rating,
                Review = dto.Review,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Ratings.Add(newRating);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserMovieDto>> GetUserMoviesAsync(int userId, string? statusStr)
    {
        var query = _context.UserMovieStatuses
            .Include(s => s.Movie)
            .Where(s => s.UserId == userId)
            .AsNoTracking();

        if (!string.IsNullOrEmpty(statusStr) && Enum.TryParse<MovieStatus>(statusStr, true, out var statusEnum))
        {
            query = query.Where(s => s.Status == statusEnum);
        }

        var userMovieStatuses = await query.ToListAsync();

        return _mapper.Map<IEnumerable<UserMovieDto>>(userMovieStatuses);
    }

    Task<IEnumerable<UserMovieDto>> IUserActionService.GetUserMoviesAsync(int userId, string? statusStr)
    {
        throw new NotImplementedException();
    }
}
