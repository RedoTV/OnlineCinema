using OnlineCinema.Backend.Models.DTOs.Comments;

namespace OnlineCinema.Backend.Services.Comments;

public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetCommentsAsync(int? movieId, int? seriesId, string? sort, int? userId);
    Task<CommentDto> AddAsync(int userId, CreateCommentDto dto);
    Task ToggleLikeAsync(int commentId, int userId);
    Task HideAsync(int commentId, bool hide);
    Task DeleteAsync(int commentId);
}