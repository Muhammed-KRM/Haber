using System.Net.Http.Json;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;

namespace KursuTV.SharedUI.ApiServices;

public class UserApiService : IUserService
{
    private readonly HttpClient _http;

    public UserApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<UserDto?> GetProfileAsync(Guid userId)
    {
        return await _http.GetFromJsonAsync<UserDto>("api/users/profile");
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UserProfileUpdateDto dto)
    {
        var response = await _http.PutAsJsonAsync("api/users/profile", dto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>() ?? new UserDto();
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var response = await _http.PutAsJsonAsync("api/users/change-password", dto);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<UserDto>> GetAuthorsAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<List<UserDto>>("api/users/authors", cancellationToken) ?? new();
    }
}
