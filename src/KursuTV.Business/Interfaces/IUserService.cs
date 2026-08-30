using KursuTV.Business.DTOs;

namespace KursuTV.Business.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetProfileAsync(Guid userId);
    Task<UserDto> UpdateProfileAsync(Guid userId, UserProfileUpdateDto dto);
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
    Task<List<UserDto>> GetAuthorsAsync(CancellationToken cancellationToken = default);
}
