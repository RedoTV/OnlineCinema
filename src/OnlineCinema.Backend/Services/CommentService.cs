using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Events;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Comments;
using Microsoft.EntityFrameworkCore;

namespace OnlineCinema.Backend.Services;

public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetCommentsAsync(int? movieId, int? seriesId, string? sort, int? userId);
    Task<CommentDto> AddAsync(int userId, CreateCommentDto dto);
    Task ToggleLikeAsync(int commentId, int userId);
    Task HideAsync(int commentId, bool hide);
    Task DeleteAsync(int commentId);
}

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _context;
    private readonly IEventPublisher _publisher;

    public CommentService(ApplicationDbContext context, IEventPublisher publisher)
    {
        _context = context;
        _publisher = publisher;
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsAsync(int? movieId, int? seriesId, string? sort, int? userId)
    {
        var query = _context.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .Include(c => c.Replies).ThenInclude(r => r.User)
            .Include(c => c.Replies).ThenInclude(r => r.Likes)
            .AsNoTracking()
            .Where(c => c.ParentId == null);

        if (movieId.HasValue) query = query.Where(c => c.MovieId == movieId);
        if (seriesId.HasValue) query = query.Where(c => c.SeriesId == seriesId);

        var comments = await query.ToListAsync();

        // сортировка: по дате (старые/новые) или по популярности (лайки)
        if (sort == "popular")
            comments = comments.OrderByDescending(c => c.Likes.Count).ToList();
        else if (sort == "newest")
            comments = comments.OrderByDescending(c => c.CreatedAt).ToList();
        else
            comments = comments.OrderBy(c => c.CreatedAt).ToList();

        return comments.Select(c => ToDto(c, userId));
    }

    public async Task<CommentDto> AddAsync(int userId, CreateCommentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
            throw new ArgumentException("Text cannot be empty");

        var comment = new Comment
        {
            UserId = userId,
            MovieId = dto.MovieId,
            SeriesId = dto.SeriesId,
            ParentId = dto.ParentId,
            Text = dto.Text
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        // подгружаем юзера для ответа
        await _context.Entry(comment).Reference(c => c.User).LoadAsync();

        await _publisher.PublishAsync("comment.created", new
        {
            userId,
            commentId = comment.Id,
            movieId = comment.MovieId,
            seriesId = comment.SeriesId,
            parentId = comment.ParentId,
            happenedAt = comment.CreatedAt
        });
        return ToDto(comment, userId);
    }

    public async Task ToggleLikeAsync(int commentId, int userId)
    {
        var existing = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

        if (existing != null)
        {
            _context.CommentLikes.Remove(existing);
        }
        else
        {
            _context.CommentLikes.Add(new CommentLike { CommentId = commentId, UserId = userId });
        }
        await _context.SaveChangesAsync();
    }

    public async Task HideAsync(int commentId, bool hide)
    {
        var comment = await _context.Comments.FindAsync(commentId) ?? throw new KeyNotFoundException("Comment not found");
        comment.IsHidden = hide;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) return;
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
    }

    private CommentDto ToDto(Comment c, int? userId)
    {
        var dto = new CommentDto
        {
            Id = c.Id,
            UserId = c.UserId,
            Username = c.User?.Username ?? "unknown",
            Text = c.IsHidden ? "[удалено модератором]" : c.Text,
            CreatedAt = c.CreatedAt,
            IsHidden = c.IsHidden,
            LikeCount = c.Likes.Count,
            LikedByMe = userId.HasValue && c.Likes.Any(l => l.UserId == userId),
            Replies = c.Replies
                .OrderBy(r => r.CreatedAt)
                .Select(r => ToDto(r, userId))
                .ToList()
        };
        return dto;
    }
}
