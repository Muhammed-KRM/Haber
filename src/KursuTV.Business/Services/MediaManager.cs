using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace KursuTV.Business.Services;

public class MediaManager : IMediaService
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<MediaManager> _logger;

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/gif", "video/mp4", "video/webm"
    };

    public MediaManager(
        IMediaRepository mediaRepository,
        IFileStorageService fileStorageService,
        ILogger<MediaManager> logger)
    {
        _mediaRepository = mediaRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<MediaDto> UploadSingleFileAsync(
        Stream stream,
        string fileName,
        string contentType,
        long fileLength,
        string? altText,
        Guid? newsId,
        Guid uploadedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (stream == null || fileLength == 0)
        {
            throw new BusinessException("Yüklenecek geçerli bir dosya bulunamadı.");
        }

        // 50 MB sınır
        if (fileLength > 50 * 1024 * 1024)
        {
            throw new BusinessException("Dosya boyutu 50 MB'tan büyük olamaz.");
        }

        // MIME doğrulama
        if (!AllowedMimeTypes.Contains(contentType))
        {
            throw new BusinessException("Desteklenmeyen dosya türü. Yalnızca JPEG, PNG, WEBP, GIF ve MP4 dosyaları yüklenebilir.");
        }

        // Magic Bytes doğrulaması (dosya uzantısı taklitlerine karşı)
        if (!IsValidImageOrVideoHeader(stream, contentType))
        {
            throw new BusinessException("Dosya içeriği geçersiz veya bozuk.");
        }
        stream.Position = 0;

        // Dosya adı güvenli hale getirilir
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var safeFileName = $"{Guid.NewGuid()}{extension}";

        var uploadedUrl = await _fileStorageService.UploadAsync(stream, safeFileName, contentType);

        var media = new Media
        {
            Id = Guid.NewGuid(),
            OriginalUrl = uploadedUrl,
            ThumbnailUrl = uploadedUrl,
            ListUrl = uploadedUrl,
            AltText = altText?.Trim() ?? Path.GetFileNameWithoutExtension(fileName),
            MimeType = contentType,
            FileSizeBytes = fileLength,
            NewsId = newsId,
            UploadedByUserId = uploadedByUserId,
            UploadedAt = DateTime.UtcNow
        };

        await _mediaRepository.AddAsync(media);
        await _mediaRepository.SaveChangesAsync();

        _logger.LogInformation("Medya dosyası yüklendi: {MediaId} - {Url}", media.Id, media.OriginalUrl);

        return new MediaDto(
            media.Id,
            media.OriginalUrl,
            media.ThumbnailUrl,
            media.ListUrl,
            media.AltText,
            media.MimeType,
            media.FileSizeBytes,
            media.NewsId,
            media.UploadedAt,
            "Yönetici"
        );
    }

    public async Task<PagedResultDto<MediaDto>> GetPagedMediaAsync(int pageNumber, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _mediaRepository.GetPagedMediaAsync(pageNumber, pageSize, searchTerm, cancellationToken);
        var dtos = items.Select(m => new MediaDto(
            m.Id,
            m.OriginalUrl,
            m.ThumbnailUrl,
            m.ListUrl,
            m.AltText,
            m.MimeType,
            m.FileSizeBytes,
            m.NewsId,
            m.UploadedAt,
            m.UploadedByUser?.FullName ?? "Kullanıcı"
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedResultDto<MediaDto>(dtos, totalCount, pageNumber, pageSize, totalPages);
    }

    public async Task DeleteMediaAsync(Guid mediaId, CancellationToken cancellationToken = default)
    {
        var media = await _mediaRepository.GetByIdAsync(mediaId, cancellationToken);
        if (media == null)
        {
            throw new NotFoundException("Silinecek medya dosyası bulunamadı.");
        }

        await _fileStorageService.DeleteAsync(media.OriginalUrl);
        _mediaRepository.Delete(media);
        await _mediaRepository.SaveChangesAsync();

        _logger.LogInformation("Medya silindi: {MediaId}", mediaId);
    }

    private static bool IsValidImageOrVideoHeader(Stream stream, string mimeType)
    {
        var buffer = new byte[8];
        var read = stream.Read(buffer, 0, buffer.Length);
        stream.Position = 0;

        if (read < 4) return false;

        // JPEG: FF D8 FF
        if (mimeType == "image/jpeg" && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
            return true;

        // PNG: 89 50 4E 47
        if (mimeType == "image/png" && buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
            return true;

        // GIF: 47 49 46 38
        if (mimeType == "image/gif" && buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38)
            return true;

        // WEBP: 52 49 46 46 (RIFF)
        if (mimeType == "image/webp" && buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46)
            return true;

        // MP4 / WebM
        if (mimeType.StartsWith("video/"))
            return true;

        return false;
    }
}
