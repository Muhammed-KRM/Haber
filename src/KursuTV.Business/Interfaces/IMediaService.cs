using KursuTV.Business.DTOs;

namespace KursuTV.Business.Interfaces;

public interface IMediaService
{
    Task<MediaDto> UploadSingleFileAsync(
        Stream stream,
        string fileName,
        string contentType,
        long fileLength,
        string? altText,
        Guid? newsId,
        Guid uploadedByUserId,
        CancellationToken cancellationToken = default);

    Task<PagedResultDto<MediaDto>> GetPagedMediaAsync(int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
    Task DeleteMediaAsync(Guid mediaId, CancellationToken cancellationToken = default);
}
