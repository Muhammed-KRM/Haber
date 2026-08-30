using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var userId = GetCurrentUserId();
        var profile = await _userService.GetProfileAsync(userId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UserProfileUpdateDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _userService.UpdateProfileAsync(userId, dto);
        return Ok(result);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = GetCurrentUserId();
        await _userService.ChangePasswordAsync(userId, dto);
        return NoContent();
    }

    [HttpGet("authors")]
    [AllowAnonymous]
    public async Task<ActionResult<List<UserDto>>> GetAuthors(CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetAuthorsAsync(cancellationToken);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
    }
}
