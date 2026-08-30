using MassTransit;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace KursuTV.Worker.Consumers;

/// <summary>
/// Yüklenen görsellerin WebP formatına dönüştürülmesini tetikleyen RabbitMQ consumer.
/// Görsel dosyası fiziksel olarak wwwroot/uploads klasöründe bulunmalıdır.
/// SixLabors.ImageSharp kullanılarak kayıpsız WebP dönüşümü yapılır.
/// </summary>
public class ImageConversionConsumer : IConsumer<ImageUploadedEvent>
{
    private readonly ILogger<ImageConversionConsumer> _logger;
    private readonly string _uploadsBasePath;

    public ImageConversionConsumer(ILogger<ImageConversionConsumer> logger)
    {
        _logger = logger;
        // Gerçek ortamda appsettings'ten okunmalı; dev için fallback
        _uploadsBasePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads");
    }

    public async Task Consume(ConsumeContext<ImageUploadedEvent> context)
    {
        var @event = context.Message;
        _logger.LogInformation("[ImageConversionConsumer] Görsel dönüşüm başlıyor. Dosya: {FileName}", @event.FileName);

        var sourcePath = Path.Combine(_uploadsBasePath, @event.RelativePath);
        if (!File.Exists(sourcePath))
        {
            _logger.LogWarning("[ImageConversionConsumer] Kaynak dosya bulunamadı: {Path}", sourcePath);
            return;
        }

        var webpPath = Path.ChangeExtension(sourcePath, ".webp");
        if (File.Exists(webpPath))
        {
            _logger.LogInformation("[ImageConversionConsumer] WebP zaten mevcut, atlanıyor: {Path}", webpPath);
            return;
        }

        try
        {
            using var image = await Image.LoadAsync(sourcePath, context.CancellationToken);

            var encoder = new WebpEncoder
            {
                Quality = 82,
                FileFormat = WebpFileFormatType.Lossy
            };

            await image.SaveAsync(webpPath, encoder, context.CancellationToken);

            _logger.LogInformation("[ImageConversionConsumer] WebP dönüşümü başarılı: {WebpPath}", webpPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ImageConversionConsumer] WebP dönüşüm hatası. Dosya: {FileName}", @event.FileName);
        }
    }
}

/// <summary>Yeni görsel yükleme olayı mesaj kontratı.</summary>
public record ImageUploadedEvent(
    Guid EntityId,
    string EntityType,     // "News", "Author" vb.
    string FileName,
    string RelativePath    // "news/2024/01/cover.jpg" gibi
);
