namespace OnlineCinema.Backend.Models.DTOs.Comments;

public class CommentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsHidden { get; set; }
    public int LikeCount { get; set; }
    public bool LikedByMe { get; set; }
    public List<CommentDto> Replies { get; set; } = new();
}
