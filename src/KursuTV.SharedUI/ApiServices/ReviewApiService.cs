using System.Net.Http.Json;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;

namespace KursuTV.SharedUI.ApiServices;

public class ReviewApiService : IReviewService
{
    private readonly HttpClient _http;

    public ReviewApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ReviewDto>> GetByListingAsync(Guid listingId)
    {
        return await _http.GetFromJsonAsync<List<ReviewDto>>($"api/reviews/listing/{listingId}") ?? new List<ReviewDto>();
    }

    public async Task<ReviewDto> CreateAsync(ReviewCreateDto dto, Guid reviewerId)
    {
        var response = await _http.PostAsJsonAsync("api/reviews", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ReviewDto>() ?? new ReviewDto();
    }

    public Task ApproveReviewAsync(Guid reviewId)
    {
        throw new NotImplementedException("Yetkilendirme gerektiren Admin iÅŸlemi.");
    }
}
