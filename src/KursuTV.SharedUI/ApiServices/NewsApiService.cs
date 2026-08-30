using System.Net.Http.Json;
using KursuTV.Business.DTOs;

namespace KursuTV.SharedUI.ApiServices;

public class NewsApiService
{
    private readonly HttpClient _http;

    public NewsApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<HeadlineNewsDto>> GetHeadlinesAsync(int count = 12)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<HeadlineNewsDto>>($"api/news/headlines?count={count}") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<List<BreakingNewsDto>> GetBreakingNewsAsync(int count = 10)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<BreakingNewsDto>>($"api/news/breaking?count={count}") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<PagedResultDto<NewsListDto>?> GetPublishedNewsPagedAsync(int page = 1, int pageSize = 20, int? categoryId = null)
    {
        try
        {
            var url = $"api/news?page={page}&pageSize={pageSize}";
            if (categoryId.HasValue) url += $"&categoryId={categoryId.Value}";
            return await _http.GetFromJsonAsync<PagedResultDto<NewsListDto>>(url);
        }
        catch
        {
            return null;
        }
    }

    public async Task<NewsDetailDto?> GetBySlugAsync(string slug)
    {
        try
        {
            return await _http.GetFromJsonAsync<NewsDetailDto>($"api/news/slug/{slug}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<NewsListDto>> GetMostViewedAsync(int count = 10)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<NewsListDto>>($"api/news/most-viewed?count={count}") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task RecordViewAsync(Guid newsId)
    {
        try
        {
            await _http.PostAsync($"api/news/{newsId}/view", null);
        }
        catch
        {
            // Sessizce geç
        }
    }
}
