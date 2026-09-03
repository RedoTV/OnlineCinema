namespace OnlineCinema.Backend.Models;

public class Comment
{
    public int Id { get; set; }
    public int UserId { get; set; }

    // на фильм или на сериал
    public int? MovieId { get; set; }
    public int? SeriesId { get; set; }

    // null для корневых, иначе id родительского коммента
    public int? ParentId { get; set; }

    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // модерация: скрыт админом
    public bool IsHidden { get; set; }

    public User User { get; set; } = null!;
    public Movie? Movie { get; set; }
    public Series? Series { get; set; }
    public Comment? Parent { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
}

// лайки комментариев
public class CommentLike
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public int UserId { get; set; }

    public Comment Comment { get; set; } = null!;
    public User User { get; set; } = null!;
}
