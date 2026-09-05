using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Comments;

public interface ICommentRepository
{
    Task<List<Comment>> GetTopLevelAsync(int? movieId, int? seriesId, CancellationToken ct = default);
    void Add(Comment comment);
    Task<Comment?> FindAsync(int id, CancellationToken ct = default);
    Task<CommentLike?> FindLikeAsync(int commentId, int userId, CancellationToken ct = default);
    void AddLike(CommentLike like);
    void RemoveLike(CommentLike like);
    void Remove(Comment comment);
    Task LoadUserAsync(Comment comment, CancellationToken ct = default);
}