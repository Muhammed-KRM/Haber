using MassTransit;
using KursuTV.Business.Events;
using KursuTV.Business.Interfaces;

namespace KursuTV.Worker.Consumers;

public class ListingDeletedConsumer : IConsumer<ListingDeletedEvent>
{
    private readonly ILogger<ListingDeletedConsumer> _logger;
    private readonly ISearchService _searchService;
    private readonly ICacheService _cacheService;

    public ListingDeletedConsumer(
        ILogger<ListingDeletedConsumer> logger,
        ISearchService searchService,
        ICacheService cacheService)
    {
        _logger = logger;
        _searchService = searchService;
        _cacheService = cacheService;
    }

    public async Task Consume(ConsumeContext<ListingDeletedEvent> context)
    {
        _logger.LogInformation("ListingDeletedEvent alÄ±ndÄ±: {ListingId}", context.Message.ListingId);
        try
        {
            await _searchService.DeleteListingIndexAsync(context.Message.ListingId);
            await _cacheService.RemoveByPatternAsync("search:*");
            await _cacheService.RemoveByPatternAsync("vitrin:*");
            _logger.LogInformation("Ä°lan ES indexinden silindi: {ListingId}", context.Message.ListingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ListingDeletedEvent iÅŸlenirken hata: {ListingId}", context.Message.ListingId);
            throw;
        }
    }
}
