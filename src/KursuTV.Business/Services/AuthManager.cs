using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Configuration;
using KursuTV.Business.DTOs;
using KursuTV.Business.Events;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Helpers;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using KursuTV.Data.Repositories;

namespace KursuTV.Business.Services;

public class AuthManager : IAuthService
{
    private const string EC_REGISTER       = "AM-001";
    private const string EC_LOGIN          = "AM-002";
    private const string EC_GETCURRENT     = "AM-003";
    private const string EC_UPDATEPROFILE  = "AM-004";
    private const string EC_REFRESHTOKEN   = "AM-005";
    private const string EC_UPDATEREFRESH  = "AM-006";
    private const string EC_SETSTATUS      = "AM-007";

    private readonly IUserRepository _userRepo;
    private readonly IValidator<UserRegisterDto> _registerValidator;
    private readonly IEmailService _emailService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IConfiguration _config;
    private readonly ILogService _logService;

    public AuthManager(
        IUserRepository userRepo,
        IValidator<UserRegisterDto> registerValidator,
        IEmailService emailService,
        IPublishEndpoint publishEndpoint,
        IConfiguration config,
        ILogService logService)
    {
        _userRepo = userRepo;
        _registerValidator = registerValidator;
        _emailService = emailService;
        _publishEndpoint = publishEndpoint;
        _config = config;
        _logService = logService;
    }

    public async Task<AuthResultDto> RegisterAsync(UserRegisterDto dto)
    {
        try
        {
            var validationResult = await _registerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return new AuthResultDto { Success = false, ErrorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)) };

            if (!await _userRepo.IsEmailUniqueAsync(dto.Email))
                return new AuthResultDto { Success = false, ErrorMessage = "Bu e-posta adresi zaten kayıtlı." };

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = PasswordHasher.Hash(dto.Password),
                FullName = dto.FullName,
                Role = UserRole.Reporter,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();

            _ = _emailService.SendTemplatedEmailAsync(
                user.Email,
                "Kürsü TV'ye Hoş Geldiniz!",
                new Dictionary<string, string> { { "FullName", user.FullName } }
            );

            try
            {
                await _publishEndpoint.Publish(new SendNotificationEvent
                {
                    UserId = user.Id,
                    Type = "Welcome",
                    Title = "Kürsü TV'ye Hoş Geldiniz!",
                    Message = "Hesabınız başarıyla oluşturuldu.",
                    ActionUrl = "/",
                    SendEmail = false,
                    IdempotencyKey = $"welcome-{user.Id}"
                });
            }
            catch (Exception ex)
            {
                await _logService.LogFunctionErrorAsync("AM-NOTIF", ex, new { userId = user.Id });
            }

            return new AuthResultDto { Success = true, User = MapToDto(user), Token = "", RefreshToken = "" };
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_REGISTER, ex, new { dto.Email, dto.FullName });
            throw;
        }
    }

    public async Task<AuthResultDto> LoginAsync(UserLoginDto dto)
    {
        try
        {
            var user = await _userRepo.GetByEmailAsync(dto.Email);
            if (user is null || !PasswordHasher.Verify(dto.Password, user.PasswordHash))
                return new AuthResultDto { Success = false, ErrorMessage = "E-posta veya şifre hatalı." };

            if (!user.IsActive)
                return new AuthResultDto { Success = false, ErrorMessage = "Hesabınız askıya alınmıştır." };

            return new AuthResultDto { Success = true, User = MapToDto(user), Token = "", RefreshToken = "" };
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_LOGIN, ex, new { dto.Email });
            throw;
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId);
            return user is null ? null : MapToDto(user);
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_GETCURRENT, ex, userId, userId);
            throw;
        }
    }

    public async Task<UserDto> UpdateProfileAsync(Guid userId, UserProfileUpdateDto dto)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new NotFoundException("Kullanıcı", userId);

            user.FullName = dto.FullName;
            user.Bio = dto.Bio;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();

            return MapToDto(user);
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_UPDATEPROFILE, ex, dto, userId);
            throw;
        }
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var user = (await _userRepo.FindAsync(u =>
                u.RefreshToken == refreshToken &&
                u.RefreshTokenExpiryTime > DateTime.UtcNow)).FirstOrDefault();

            if (user is null || !user.IsActive)
                return new AuthResultDto { Success = false, ErrorMessage = "Geçersiz veya süresi dolmuş refresh token." };

            return new AuthResultDto { Success = true, User = MapToDto(user), Token = "", RefreshToken = "" };
        }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_REFRESHTOKEN, ex);
            throw;
        }
    }

    public async Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiryTime)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new NotFoundException("Kullanıcı", userId);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiryTime;
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_UPDATEREFRESH, ex, userId, userId);
            throw;
        }
    }

    public async Task SetUserStatusAsync(Guid userId, bool isActive)
    {
        try
        {
            var user = await _userRepo.GetByIdAsync(userId)
                ?? throw new NotFoundException("Kullanıcı", userId);

            user.IsActive = isActive;
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync();
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex)
        {
            await _logService.LogFunctionErrorAsync(EC_SETSTATUS, ex, new { userId, isActive }, userId);
            throw;
        }
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
