namespace OnlineCinema.Backend.Models.DTOs.Comments;

public class CreateCommentDto
{
    public int? MovieId { get; set; }
    public int? SeriesId { get; set; }
    public int? ParentId { get; set; }
    public string Text { get; set; } = string.Empty;
}
