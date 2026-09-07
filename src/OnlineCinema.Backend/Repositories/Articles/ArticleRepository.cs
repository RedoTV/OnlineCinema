using Microsoft.EntityFrameworkCore;
using OnlineCinema.Backend.Data;
using OnlineCinema.Backend.Models;
using OnlineCinema.Backend.Models.DTOs.Articles;

namespace OnlineCinema.Backend.Repositories.Articles;

public class ArticleRepository : IArticleRepository
{
    private readonly ApplicationDbContext _db;

    public ArticleRepository(ApplicationDbContext db) => _db = db;

    private static ArticleDto Map(Article a) => new ArticleDto
    {
        Id = a.Id,
        AuthorId = a.AuthorId,
        AuthorUsername = a.Author?.Username ?? "—",
        Title = a.Title,
        Content = a.Content,
        IsPublished = a.IsPublished,
        CreatedAt = a.CreatedAt,
    };

    public async Task<List<ArticleDto>> GetPublishedAsync(CancellationToken ct = default) =>
        await _db.Articles.AsNoTracking()
            .Include(a => a.Author)
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ArticleDto
            {
                Id = a.Id,
                AuthorId = a.AuthorId,
                AuthorUsername = a.Author.Username,
                Title = a.Title,
                Content = a.Content,
                IsPublished = a.IsPublished,
                CreatedAt = a.CreatedAt,
            })
            .ToListAsync(ct);

    public async Task<List<ArticleDto>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Articles.AsNoTracking()
            .Include(a => a.Author)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new ArticleDto
            {
                Id = a.Id,
                AuthorId = a.AuthorId,
                AuthorUsername = a.Author.Username,
                Title = a.Title,
                Content = a.Content,
                IsPublished = a.IsPublished,
                CreatedAt = a.CreatedAt,
            })
            .ToListAsync(ct);

    public async Task<ArticleDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var a = await _db.Articles.AsNoTracking().Include(x => x.Author).FirstOrDefaultAsync(x => x.Id == id, ct);
        return a == null ? null : Map(a);
    }

    public async Task<ArticleDto> CreateAsync(int authorId, CreateArticleDto dto, CancellationToken ct = default)
    {
        var article = new Article
        {
            AuthorId = authorId,
            Title = dto.Title.Trim(),
            Content = dto.Content.Trim(),
            IsPublished = false,
        };
        _db.Articles.Add(article);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(article).Reference(a => a.Author).LoadAsync(ct);
        return Map(article);
    }

    public async Task<ArticleDto?> UpdateAsync(int id, CreateArticleDto dto, CancellationToken ct = default)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (article == null) return null;

        article.Title = dto.Title.Trim();
        article.Content = dto.Content.Trim();
        article.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(article).Reference(a => a.Author).LoadAsync(ct);
        return Map(article);
    }

    public async Task<bool> SetPublishedAsync(int id, bool published, CancellationToken ct = default)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (article == null) return false;

        article.IsPublished = published;
        article.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var article = await _db.Articles.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (article == null) return false;

        _db.Articles.Remove(article);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
