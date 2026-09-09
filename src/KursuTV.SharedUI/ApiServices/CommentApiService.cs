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

    public async Task<(bool Success, string ErrorMessage)> AddCommentAsync(CreateCommentDto dto)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("api/comments", dto);
            if (res.IsSuccessStatusCode)
                return (true, string.Empty);

            var errorResponse = await res.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            if (errorResponse.ValueKind != System.Text.Json.JsonValueKind.Undefined && errorResponse.TryGetProperty("detail", out var detail))
            {
                return (false, detail.GetString() ?? "Yorum gönderilemedi.");
            }
            return (false, "Yorum gönderilemedi.");
        }
        catch
        {
            return (false, "Beklenmeyen bir hata oluştu.");
        }
    }
}
