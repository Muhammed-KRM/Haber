using System.Net.Http.Json;
using KursuTV.Business.DTOs;

namespace KursuTV.SharedUI.ApiServices;

public class CommentApiService
{
    private readonly HttpClient _http;

    public CommentApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CommentDto>> GetApprovedCommentsByNewsIdAsync(Guid newsId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<CommentDto>>($"api/comments/news/{newsId}") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> AddCommentAsync(CreateCommentDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/comments", dto);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
