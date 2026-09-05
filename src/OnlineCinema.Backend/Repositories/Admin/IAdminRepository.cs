namespace OnlineCinema.Backend.Repositories.Admin;

public record AdminDashboardDto(int Movies, int Series, int Actors, int Genres, int Users, int Comments, int HiddenComments, int Ratings, double AverageRating);
public record AdminCommentRow(int Id, string Text, bool IsHidden, DateTime CreatedAt, int? ParentId, string Username, string Target);
public record AdminRatingRow(int Id, int RatingValue, DateTime CreatedAt, string Username, string Target, string ContentType);

public interface IAdminRepository
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task<List<AdminCommentRow>> GetCommentsAsync(CancellationToken ct = default);
    Task<List<AdminRatingRow>> GetRatingsAsync(CancellationToken ct = default);
}