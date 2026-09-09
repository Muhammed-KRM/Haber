using System.Net.Http.Json;
using KursuTV.Business.DTOs;

namespace KursuTV.SharedUI.ApiServices;

public class CategoryApiService
{
    private readonly HttpClient _http;

    public CategoryApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CategoryDto>> GetActiveTreeAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<CategoryDto>>("api/categories/tree", KursuTV.SharedUI.Extensions.HttpClientExtensions.DefaultOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<List<CategoryDto>> GetFlatAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<CategoryDto>>("api/categories/flat", KursuTV.SharedUI.Extensions.HttpClientExtensions.DefaultOptions) ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        try
        {
            return await _http.GetFromJsonAsync<CategoryDto>($"api/categories/slug/{slug}", KursuTV.SharedUI.Extensions.HttpClientExtensions.DefaultOptions);
        }
        catch
        {
            return null;
        }
    }
}
