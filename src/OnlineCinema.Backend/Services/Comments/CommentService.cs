using OnlineCinema.Backend.Events;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Comments;
using OnlineCinema.Backend.Repositories.Comments;
using OnlineCinema.Backend.Repositories.UnitOfWork;

namespace OnlineCinema.Backend.Services.Comments;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _comments;
    private readonly IUnitOfWork _uow;
    private readonly IEventPublisher _publisher;

    public CommentService(ICommentRepository comments, IUnitOfWork uow, IEventPublisher publisher)
    {
        _comments = comments;
        _uow = uow;
        _publisher = publisher;
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsAsync(int? movieId, int? seriesId, string? sort, int? userId)
    {
        var comments = await _comments.GetTopLevelAsync(movieId, seriesId);

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

        _comments.Add(comment);
        await _uow.SaveChangesAsync();

        // подгружаем юзера для ответа
        await _comments.LoadUserAsync(comment);

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
        var existing = await _comments.FindLikeAsync(commentId, userId);

        if (existing != null)
        {
            _comments.RemoveLike(existing);
        }
        else
        {
            _comments.AddLike(new CommentLike { CommentId = commentId, UserId = userId });
        }
        await _uow.SaveChangesAsync();
    }

    public async Task HideAsync(int commentId, bool hide)
    {
        var comment = await _comments.FindAsync(commentId) ?? throw new KeyNotFoundException("Comment not found");
        comment.IsHidden = hide;
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(int commentId)
    {
        var comment = await _comments.FindAsync(commentId);
        if (comment == null) return;
        _comments.Remove(comment);
        await _uow.SaveChangesAsync();
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