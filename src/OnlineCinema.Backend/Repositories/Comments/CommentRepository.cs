using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;

namespace OnlineCinema.Backend.Repositories.Comments;

public class CommentRepository : ICommentRepository
{
    private readonly ApplicationDbContext _db;

    public CommentRepository(ApplicationDbContext db) => _db = db;

    public Task<List<Comment>> GetTopLevelAsync(int? movieId, int? seriesId, CancellationToken ct = default)
    {
        var query = _db.Comments
            .Include(c => c.User)
            .Include(c => c.Likes)
            .Include(c => c.Replies).ThenInclude(r => r.User)
            .Include(c => c.Replies).ThenInclude(r => r.Likes)
            .AsNoTracking()
            .Where(c => c.ParentId == null);

        if (movieId.HasValue) query = query.Where(c => c.MovieId == movieId);
        if (seriesId.HasValue) query = query.Where(c => c.SeriesId == seriesId);

        return query.ToListAsync(ct);
    }

    public void Add(Comment comment) => _db.Comments.Add(comment);

    public async Task<Comment?> FindAsync(int id, CancellationToken ct = default) =>
        await _db.Comments.FindAsync(new object[] { id }, ct);

    public Task<CommentLike?> FindLikeAsync(int commentId, int userId, CancellationToken ct = default) =>
        _db.CommentLikes.FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId, ct);

    public void AddLike(CommentLike like) => _db.CommentLikes.Add(like);

    public void RemoveLike(CommentLike like) => _db.CommentLikes.Remove(like);

    public void Remove(Comment comment) => _db.Comments.Remove(comment);

    public Task LoadUserAsync(Comment comment, CancellationToken ct = default) =>
        _db.Entry(comment).Reference(c => c.User).LoadAsync(ct);
}