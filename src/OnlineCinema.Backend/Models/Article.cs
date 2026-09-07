namespace OnlineCinema.Backend.Models;

// Новости/статьи пользователей про фильмы и сериалы.
// Создаются любым авторизованным пользователем, но публикуются
// только после одобрения администратором или модератором.
public class Article
{
    public int Id { get; set; }
    public int AuthorId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User Author { get; set; } = null!;
}
