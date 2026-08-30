using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Enums;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ISettingService _settingService;

    public AdminController(IAdminService adminService, ISettingService settingService)
    {
        _adminService = adminService;
        _settingService = settingService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetUsers(
        [FromQuery] string? search = null,
        [FromQuery] UserRole? role = null,
        [FromQuery] bool? isActive = null)
    {
        var users = await _adminService.GetAllUsersAsync(search, role, isActive);
        return Ok(users);
    }

    [HttpPost("users/{userId:guid}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid userId, [FromBody] SuspendRequest request)
    {
        await _adminService.SuspendUserAsync(userId, request.Reason);
        return Ok(new { message = "Kullanıcı askıya alındı." });
    }

    [HttpPost("users/{userId:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid userId)
    {
        await _adminService.ActivateUserAsync(userId);
        return Ok(new { message = "Kullanıcı aktif edildi." });
    }

    [HttpPut("users/{userId:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid userId, [FromBody] UpdateRoleRequest request)
    {
        await _adminService.UpdateUserRoleAsync(userId, request.Role);
        return Ok(new { message = "Kullanıcı rolü güncellendi." });
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var siteTitle = await _settingService.GetSettingAsync("SiteTitle", "Kürsü TV");
        var siteDescription = await _settingService.GetSettingAsync("SiteDescription", "Bağımsız Haber Portalı");
        var contactEmail = await _settingService.GetSettingAsync("ContactEmail", "info@kursutv.com");

        var settings = new List<GlobalSettingDto>
        {
            new() { Key = "SiteTitle", Value = siteTitle, Description = "Site Başlığı" },
            new() { Key = "SiteDescription", Value = siteDescription, Description = "Site Açıklaması" },
            new() { Key = "ContactEmail", Value = contactEmail, Description = "İletişim E-postası" }
        };

        return Ok(settings);
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSetting([FromBody] GlobalSettingDto dto)
    {
        await _settingService.SetSettingAsync(dto.Key, dto.Value, dto.Description);
        return Ok(new { message = "Ayar güncellendi." });
    }
}

public record SuspendRequest(string? Reason);
public record UpdateRoleRequest(UserRole Role);
