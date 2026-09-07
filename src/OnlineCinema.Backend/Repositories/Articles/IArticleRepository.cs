using OnlineCinema.Backend.Models.DTOs.Articles;

namespace OnlineCinema.Backend.Repositories.Articles;

public interface IArticleRepository
{
    Task<List<ArticleDto>> GetPublishedAsync(CancellationToken ct = default);
    Task<List<ArticleDto>> GetAllAsync(CancellationToken ct = default);
    Task<ArticleDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ArticleDto> CreateAsync(int authorId, CreateArticleDto dto, CancellationToken ct = default);
    Task<ArticleDto?> UpdateAsync(int id, CreateArticleDto dto, CancellationToken ct = default);
    Task<bool> SetPublishedAsync(int id, bool published, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
