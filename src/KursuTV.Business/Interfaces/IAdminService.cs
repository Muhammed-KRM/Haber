using KursuTV.Business.DTOs;
using KursuTV.Data.Enums;

namespace KursuTV.Business.Interfaces;

public interface IAdminService
{
    Task<List<AdminUserDto>> GetAllUsersAsync(string? search = null, UserRole? role = null, bool? isActive = null);
    Task SuspendUserAsync(Guid userId, string? reason);
    Task ActivateUserAsync(Guid userId);
    Task UpdateUserRoleAsync(Guid userId, UserRole newRole);
}
