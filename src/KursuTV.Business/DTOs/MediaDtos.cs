namespace KursuTV.Business.DTOs;

public record MediaDto(
    Guid Id,
    string OriginalUrl,
    string? ThumbnailUrl,
    string? ListUrl,
    string? AltText,
    string MimeType,
    long FileSizeBytes,
    Guid? NewsId,
    DateTime UploadedAt,
    string UploadedByUserName
);

public record ChunkUploadInitDto(
    string FileName,
    long TotalSizeBytes,
    string MimeType,
    int TotalChunks
);

public record ChunkUploadInitResponseDto(
    string UploadId,
    string TempFilePath
);

public record ChunkUploadProgressDto(
    string UploadId,
    int ChunkIndex,
    int TotalChunks,
    string? AltText,
    Guid? NewsId
);
