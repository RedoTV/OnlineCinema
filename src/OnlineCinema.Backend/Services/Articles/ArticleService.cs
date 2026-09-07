using OnlineCinema.Backend.Models.DTOs.Articles;
using OnlineCinema.Backend.Repositories.Articles;

namespace OnlineCinema.Backend.Services.Articles;

public class ArticleService : IArticleService
{
    private readonly IArticleRepository _articles;

    public ArticleService(IArticleRepository articles) => _articles = articles;

    public Task<List<ArticleDto>> GetPublishedAsync(CancellationToken ct = default) =>
        _articles.GetPublishedAsync(ct);

    public Task<List<ArticleDto>> GetAllAsync(CancellationToken ct = default) =>
        _articles.GetAllAsync(ct);

    public Task<ArticleDto?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _articles.GetByIdAsync(id, ct);

    public Task<ArticleDto> CreateAsync(int authorId, CreateArticleDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
            throw new ArgumentException("Название и текст статьи обязательны");
        return _articles.CreateAsync(authorId, dto, ct);
    }

    public Task<ArticleDto?> UpdateAsync(int id, CreateArticleDto dto, CancellationToken ct = default) =>
        _articles.UpdateAsync(id, dto, ct);

    public Task<bool> SetPublishedAsync(int id, bool published, CancellationToken ct = default) =>
        _articles.SetPublishedAsync(id, published, ct);

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
        _articles.DeleteAsync(id, ct);
}
