using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Configuration;
using KursuTV.Business.DTOs;
using KursuTV.Business.Infrastructure.Search.Models;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Enums;

namespace KursuTV.Business.Infrastructure.Search;

public class ElasticsearchService : ISearchService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public ElasticsearchService(ElasticsearchClient client, IConfiguration config)
    {
        _client = client;
        _indexName = config["Elasticsearch:DefaultIndex"] ?? "news_index";
    }

    public async Task IndexNewsAsync(NewsDetailDto news, CancellationToken cancellationToken = default)
    {
        var doc = new NewsDocument
        {
            Id = news.Id.ToString(),
            Title = news.Title,
            Slug = news.Slug,
            Spot = news.Spot,
            Content = news.Content,
            CoverImageUrl = news.CoverImageUrl,
            AuthorName = news.AuthorName,
            CategoryNames = news.Categories.Select(c => c.Name).ToList(),
            CategorySlugs = news.Categories.Select(c => c.Slug).ToList(),
            TagNames = news.Tags.Select(t => t.Name).ToList(),
            ViewCount = news.ViewCount,
            PublishedAt = news.PublishedAt
        };

        await _client.IndexAsync(doc, idx => idx.Index(_indexName).Id(doc.Id), cancellationToken);
    }

    public async Task DeleteNewsIndexAsync(Guid newsId, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<NewsDocument>(newsId.ToString(), d => d.Index(_indexName), cancellationToken);
    }

    public async Task<PagedResultDto<NewsListDto>> SearchNewsAsync(NewsSearchFilterDto filters, CancellationToken cancellationToken = default)
    {
        var mustQueries = new List<Action<QueryDescriptor<NewsDocument>>>();

        if (!string.IsNullOrWhiteSpace(filters.Query))
        {
            mustQueries.Add(q => q.QueryString(qs => qs
                .Fields(new[] { "title^3", "spot^2", "content", "tagNames^2" })
                .Query($"*{filters.Query}*")
            ));
        }

        if (!string.IsNullOrWhiteSpace(filters.CategorySlug))
        {
            mustQueries.Add(q => q.Term(t => t.Field(f => f.CategorySlugs).Value(filters.CategorySlug)));
        }

        var searchResponse = await _client.SearchAsync<NewsDocument>(s => s
            .Indices(_indexName)
            .From((filters.Page - 1) * filters.PageSize)
            .Size(filters.PageSize)
            .Query(q => q.Bool(b => b.Must(mustQueries.ToArray())))
            .Sort(srt => srt
                .Field(f => f.PublishedAt, sort => sort.Order(SortOrder.Desc))
            ),
            cancellationToken
        );

        var totalCount = (int)searchResponse.Total;
        var items = searchResponse.Documents.Select(d => new NewsListDto(
            Guid.Parse(d.Id),
            d.Title,
            d.Slug,
            d.Spot,
            d.CoverImageUrl,
            null,
            NewsStatus.Published,
            NewsType.Article,
            false,
            0,
            d.ViewCount,
            d.PublishedAt,
            d.PublishedAt ?? DateTime.UtcNow,
            d.AuthorName,
            d.CategoryNames.Zip(d.CategorySlugs, (name, slug) => new CategoryDto(0, name, slug, null, 0, true, null, null)).ToList()
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize);
        return new PagedResultDto<NewsListDto>(items, totalCount, filters.Page, filters.PageSize, totalPages);
    }
}
