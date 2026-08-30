using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KursuTV.Business.Infrastructure.Search.Models;

namespace KursuTV.Business.Infrastructure.Search;

public static class ElasticsearchExtensions
{
    public static IServiceCollection AddElasticsearch(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var url = configuration["Elasticsearch:Url"] ?? "http://localhost:9200";
            var defaultIndex = configuration["Elasticsearch:DefaultIndex"] ?? "news_index";

            var settings = new ElasticsearchClientSettings(new Uri(url))
                .DefaultIndex(defaultIndex)
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll);

            var client = new ElasticsearchClient(settings);
            
            try
            {
                CreateIndexIfNotExists(client, defaultIndex).GetAwaiter().GetResult();
            }
            catch
            {
                // Elasticsearch ilk çalıştırmada erişilemezse uygulamanın çökmesini engelle
            }
            
            return client;
        });

        return services;
    }

    private static async Task CreateIndexIfNotExists(ElasticsearchClient client, string indexName)
    {
        var existsResponse = await client.Indices.ExistsAsync(indexName);
        if (existsResponse.IsValidResponse && existsResponse.Exists)
            return;

        await client.Indices.CreateAsync(indexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("turkish_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filter(new[] { "lowercase", "turkish_stop", "turkish_stemmer" })
                        )
                    )
                )
            )
            .Mappings(m => m
                .Properties<NewsDocument>(p => p
                    .Keyword(k => k.Id)
                    .Text(t => t.Title, t => t.Analyzer("turkish_analyzer"))
                    .Keyword(k => k.Slug)
                    .Text(t => t.Spot, t => t.Analyzer("turkish_analyzer"))
                    .Text(t => t.Content, t => t.Analyzer("turkish_analyzer"))
                    .Text(t => t.AuthorName, t => t.Analyzer("turkish_analyzer"))
                    .Keyword(k => k.CategorySlugs)
                    .Text(t => t.TagNames, t => t.Analyzer("turkish_analyzer"))
                    .IntegerNumber(n => n.ViewCount)
                    .Date(d => d.PublishedAt)
                )
            )
        );
    }
}
