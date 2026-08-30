using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using KursuTV.Business.Interfaces;

namespace KursuTV.Business.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly ILogger<LocalFileStorageService> _logger;

    // Ä°zin verilen dosya tÃ¼rleri ve maksimum boyut
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public LocalFileStorageService(IConfiguration config, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _uploadPath = config["FileStorage:UploadPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        
        // KlasÃ¶r yoksa oluÅŸtur
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
            _logger.LogInformation("Upload dizini oluÅŸturuldu: {Path}", _uploadPath);
        }
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        // â”€â”€ GÃ¼venlik Kontrolleri â”€â”€
        
        // 1. Dosya boyutu kontrolÃ¼
        if (fileStream.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"Dosya boyutu Ã§ok bÃ¼yÃ¼k. Maksimum {MaxFileSizeBytes / (1024 * 1024)} MB yÃ¼klenebilir.");
        }

        // 2. Dosya uzantÄ±sÄ± kontrolÃ¼
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"Desteklenmeyen dosya tÃ¼rÃ¼: {extension}. Ä°zin verilenler: {string.Join(", ", AllowedExtensions)}");
        }

        // 3. Content-Type kontrolÃ¼
        var allowedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };
        if (!allowedContentTypes.Contains(contentType))
        {
            throw new InvalidOperationException($"Desteklenmeyen iÃ§erik tÃ¼rÃ¼: {contentType}");
        }

        // â”€â”€ Dosya Kaydetme â”€â”€
        
        // Benzersiz dosya adÄ± oluÅŸtur (GUID bazlÄ±, path traversal engellenir)
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadPath, safeFileName);

        await using var outputStream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(outputStream);

        _logger.LogInformation("Dosya yÃ¼klendi: {FileName} â†’ {FilePath} ({Size} bytes)", fileName, safeFileName, fileStream.Length);

        // DosyanÄ±n URL'ini dÃ¶ndÃ¼r (relative path)
        return $"/uploads/{safeFileName}";
    }

    public Task DeleteAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.CompletedTask;

        // URL'den dosya adÄ±nÄ± Ã§Ä±kar
        var fileName = Path.GetFileName(fileUrl);
        var filePath = Path.Combine(_uploadPath, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Dosya silindi: {FilePath}", filePath);
        }

        return Task.CompletedTask;
    }
}
