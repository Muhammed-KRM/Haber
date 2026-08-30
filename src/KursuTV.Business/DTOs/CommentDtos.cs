using KursuTV.Data.Enums;

namespace KursuTV.Business.DTOs;

public record CreateCommentDto(
    Guid NewsId,
    string Content,
    string? GuestName,
    Guid? ParentCommentId
);

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
