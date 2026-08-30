using MassTransit;
using KursuTV.Business.Events;
using KursuTV.Business.Interfaces;

namespace KursuTV.Worker.Consumers;

public class ListingUpdatedConsumer : IConsumer<ListingUpdatedEvent>
{
    private readonly ILogger<ListingUpdatedConsumer> _logger;
    private readonly IListingService _listingService;
    private readonly ISearchService _searchService;
    private readonly ICacheService _cacheService;

    public ListingUpdatedConsumer(
        ILogger<ListingUpdatedConsumer> logger,
        IListingService listingService,
        ISearchService searchService,
        ICacheService cacheService)
    {
        _logger = logger;
        _listingService = listingService;
        _searchService = searchService;
        _cacheService = cacheService;
    }

    public async Task Consume(ConsumeContext<ListingUpdatedEvent> context)
    {
        _logger.LogInformation("ListingUpdatedEvent alÄ±ndÄ±: {ListingId}", context.Message.ListingId);
        try
        {
            var listing = await _listingService.GetByIdAsync(context.Message.ListingId);
            if (listing != null)
            {
                await _searchService.IndexListingAsync(listing);
                await _cacheService.RemoveByPatternAsync("search:*");
                await _cacheService.RemoveAsync($"listing:{listing.Slug}");
                _logger.LogInformation("Ä°lan ES indexi gÃ¼ncellendi: {ListingId}", listing.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ListingUpdatedEvent iÅŸlenirken hata: {ListingId}", context.Message.ListingId);
            throw;
        }
    }
}
