using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Helpers;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using KursuTV.Data.Repositories;

namespace KursuTV.Business.Services;

public class UserManager : IUserService
{
    private readonly IUserRepository _userRepo;

    public UserManager(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<UserDto?> GetProfileAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UserProfileUpdateDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("Kullanıcı", userId);

        user.FullName = dto.FullName.Trim();
        user.Bio = dto.Bio?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("Kullanıcı", userId);

        if (!PasswordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new BusinessException("Mevcut şifreniz hatalı.");
        }

        user.PasswordHash = PasswordHasher.Hash(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync();
    }

    public async Task<List<UserDto>> GetAuthorsAsync(CancellationToken cancellationToken = default)
    {
        var authors = await _userRepo.FindAsync(u => u.IsActive && (u.Role == UserRole.ColumnWriter || u.Role == UserRole.Reporter));
        return authors.Select(MapToDto).ToList();
    }

    private static UserDto MapToDto(User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        FullName = u.FullName,
        ProfileImageUrl = u.ProfileImageUrl,
        Bio = u.Bio,
        Role = u.Role.ToString(),
        CreatedAt = u.CreatedAt
    };
}
