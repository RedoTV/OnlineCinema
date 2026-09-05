using OnlineCinema.Backend.Repositories.Admin;

namespace OnlineCinema.Backend.Services.Admin;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _admin;

    public AdminService(IAdminRepository admin) => _admin = admin;

    public Task<AdminDashboardDto> GetDashboardAsync(CancellationToken ct = default) =>
        _admin.GetDashboardAsync(ct);

    public Task<List<AdminCommentRow>> GetCommentsAsync(CancellationToken ct = default) =>
        _admin.GetCommentsAsync(ct);

    public Task<List<AdminRatingRow>> GetRatingsAsync(CancellationToken ct = default) =>
        _admin.GetRatingsAsync(ct);
}