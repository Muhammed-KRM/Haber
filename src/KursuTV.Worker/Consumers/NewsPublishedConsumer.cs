using MassTransit;
using Microsoft.Extensions.Logging;

namespace KursuTV.Worker.Consumers;

/// <summary>
/// RabbitMQ üzerinden gelen "haber yayınlandı" olaylarını dinleyen consumer.
/// Gelecekte Elasticsearch veya başka bir indeksleme servisi entegre edilebilir.
/// </summary>
public class NewsPublishedConsumer : IConsumer<NewsPublishedEvent>
{
    private readonly ILogger<NewsPublishedConsumer> _logger;

    public NewsPublishedConsumer(ILogger<NewsPublishedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<NewsPublishedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation(
            "[NewsPublishedConsumer] Haber yayınlandı: Id={NewsId}, Slug={Slug}, Başlık={Title}, Tarih={PublishedAt}",
            @event.NewsId,
            @event.Slug,
            @event.Title,
            @event.PublishedAt);

        // TODO: Elasticsearch, push notification, sosyal medya paylaşımı gibi
        //       ileri işlemler buraya eklenecektir.

        return Task.CompletedTask;
    }
}

/// <summary>Haber yayın olayı mesaj kontratı.</summary>
public record NewsPublishedEvent(
    Guid NewsId,
    string Slug,
    string Title,
    string? CoverImageUrl,
    DateTime PublishedAt
);
