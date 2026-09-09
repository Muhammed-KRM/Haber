using System.Net.Http.Json;
using KursuTV.Business.DTOs;
using KursuTV.Data.Enums;

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
            return await _http.GetFromJsonAsync<List<HeadlineNewsDto>>($"api/news/headlines?count={count}", KursuTV.SharedUI.Extensions.HttpClientExtensions.DefaultOptions) ?? new();
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
            return await _http.GetFromJsonAsync<List<BreakingNewsDto>>($"api/news/breaking?count={count}", KursuTV.SharedUI.Extensions.HttpClientExtensions.DefaultOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<PagedResultDto<NewsListDto>?> GetPublishedNewsPagedAsync(
        int page = 1,
        int pageSize = 20,
        int? categoryId = null,
        NewsType? type = null,
        Guid? authorId = null,
        string? search = null)
    {
        try
        {
            var url = $"api/news?page={page}&pageSize={pageSize}";
            if (categoryId.HasValue) url += $"&categoryId={categoryId.Value}";
            if (type.HasValue) url += $"&type={type.Value}";
            if (authorId.HasValue) url += $"&authorId={authorId.Value}";
            if (!string.IsNullOrWhiteSpace(search)) url += $"&search={Uri.EscapeDataString(search)}";
            return await _http.GetFromJsonAsync<PagedResultDto<NewsListDto>>(url, KursuTV.SharedUI.Extensions.HttpClientExtensions.DefaultOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<NewsListDto>> GetColumnNewsAsync(int count = 6)
    {
        var result = await GetPublishedNewsPagedAsync(1, count, type: NewsType.Column);
        return result?.Items ?? new();
    }

    public async Task<List<NewsListDto>> GetVideoNewsAsync(int count = 4)
    {
        var result = await GetPublishedNewsPagedAsync(1, count, type: NewsType.Video);
        return result?.Items ?? new();
    }

    public async Task<NewsDetailDto?> GetBySlugAsync(string slug)
    {
        try
        {
            return await _http.GetFromJsonAsync<NewsDetailDto>($"api/news/slug/{slug}", KursuTV.SharedUI.Extensions.HttpClientExtensions.DefaultOptions);
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
            return await _http.GetFromJsonAsync<List<NewsListDto>>($"api/news/most-viewed?count={count}", KursuTV.SharedUI.Extensions.HttpClientExtensions.DefaultOptions) ?? new();
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
