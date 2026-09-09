using KursuTV.Data.Enums;

namespace KursuTV.Business.DTOs;

public class CreateCommentDto
{
    public Guid NewsId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? GuestName { get; set; }
    public Guid? ParentCommentId { get; set; }
}

public record CommentDto(
    Guid Id,
    string Content,
    Guid NewsId,
    Guid? UserId,
    string AuthorName,
    string? UserProfileImageUrl,
    CommentStatus Status,
    DateTime CreatedAt,
    List<CommentDto>? Replies
);

public record CommentModerateDto(
    Guid CommentId,
    CommentStatus Status
);
