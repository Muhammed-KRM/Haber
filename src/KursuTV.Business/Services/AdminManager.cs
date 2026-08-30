using Microsoft.EntityFrameworkCore;
using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Context;
using KursuTV.Data.Enums;
using Microsoft.Extensions.Logging;

namespace KursuTV.Business.Services;

public class AdminManager : IAdminService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminManager> _logger;

    public AdminManager(AppDbContext context, ILogger<AdminManager> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<AdminUserDto>> GetAllUsersAsync(string? search = null, UserRole? role = null, bool? isActive = null)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(s) || u.Email.ToLower().Contains(s));
        }

        if (role.HasValue)
        {
            query = query.Where(u => u.Role == role.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        return await query
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,
                CreatedAt = u.CreatedAt,
                NewsCount = u.News.Count
            })
            .ToListAsync();
    }

    public async Task SuspendUserAsync(Guid userId, string? reason)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        user.IsActive = false;
        user.BanReason = reason;
        await _context.SaveChangesAsync();

        _logger.LogWarning("Kullanıcı askıya alındı: {UserId} - Sebep: {Reason}", userId, reason);
    }

    public async Task ActivateUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        user.IsActive = true;
        user.BanReason = null;
        user.BannedUntil = null;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Kullanıcı aktif edildi: {UserId}", userId);
    }

    public async Task UpdateUserRoleAsync(Guid userId, UserRole newRole)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new NotFoundException("Kullanıcı bulunamadı.");
        }

        user.Role = newRole;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Kullanıcı rolü güncellendi: {UserId} -> {Role}", userId, newRole);
    }
}
