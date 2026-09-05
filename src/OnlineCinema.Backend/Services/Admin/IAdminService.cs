using OnlineCinema.Backend.Repositories.Admin;

namespace OnlineCinema.Backend.Services.Admin;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default);
    Task<List<AdminCommentRow>> GetCommentsAsync(CancellationToken ct = default);
    Task<List<AdminRatingRow>> GetRatingsAsync(CancellationToken ct = default);
}