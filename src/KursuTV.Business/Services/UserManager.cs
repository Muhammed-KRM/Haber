using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Repositories;

namespace KursuTV.Business.Services;

public class UserManager : IUserService
{
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    // HATA KODLARI â€” UserManager (Prefix: UM)
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
    private const string EC_GETPROFILE      = "UM-001"; // GetProfileAsync
    private const string EC_UPDATEPERSONAL  = "UM-002"; // UpdatePersonalInfoAsync
    private const string EC_UPDATEPAYMENT   = "UM-003"; // UpdatePaymentInfoAsync
    private const string EC_CHANGEPASSWORD  = "UM-004"; // ChangePasswordAsync
    private const string EC_UPDATENOTIF     = "UM-005"; // UpdateNotificationSettingsAsync
    // â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

    private readonly IRepository<User> _userRepo;
    private readonly IAuthService _authService;
    private readonly ILogService _logService;

    public UserManager(IRepository<User> userRepo, IAuthService authService, ILogService logService)
    {
        _userRepo = userRepo;
        _authService = authService;
        _logService = logService;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        try
        {
        var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("KullanÄ±cÄ±", userId);
        return new UserProfileDto
        {
            PersonalInfo = new PersonalInfoDto { FullName = user.FullName, Email = user.Email, BirthDate = user.BirthDate, Phone = user.PhoneEncrypted ?? "" },
            PaymentInfo = new PaymentInfoDto { IBAN = user.IBANEncrypted ?? "", AccountHolderName = user.FullName },
            NotificationSettings = new NotificationSettingsDto { EmailNotifications = user.EmailNotifications, MarketingEmails = user.MarketingEmails, SmsNotifications = user.SmsNotifications }
        };
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_GETPROFILE, ex, userId, userId); throw; }
    }

    public async Task UpdatePersonalInfoAsync(Guid userId, PersonalInfoDto dto)
    {
        try
        {
        var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("KullanÄ±cÄ±", userId);
        user.FullName = dto.FullName; user.Email = dto.Email; user.BirthDate = dto.BirthDate; user.PhoneEncrypted = dto.Phone;
        _userRepo.Update(user); await _userRepo.SaveChangesAsync();
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_UPDATEPERSONAL, ex, dto, userId); throw; }
    }

    public async Task UpdatePaymentInfoAsync(Guid userId, PaymentInfoDto dto)
    {
        try
        {
        var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("KullanÄ±cÄ±", userId);
        user.IBANEncrypted = dto.IBAN;
        _userRepo.Update(user); await _userRepo.SaveChangesAsync();
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_UPDATEPAYMENT, ex, userId, userId); throw; }
    }

    public async Task ChangePasswordAsync(Guid userId, PasswordChangeDto dto)
    {
        try
        {
        if (dto.NewPassword != dto.ConfirmPassword) throw new BusinessException("Yeni ÅŸifreler eÅŸleÅŸmiyor");
        var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("KullanÄ±cÄ±", userId);
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash)) throw new BusinessException("Mevcut ÅŸifre hatalÄ±");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _userRepo.Update(user); await _userRepo.SaveChangesAsync();
        }
        catch (BusinessException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_CHANGEPASSWORD, ex, userId, userId); throw; }
    }

    public async Task UpdateNotificationSettingsAsync(Guid userId, NotificationSettingsDto dto)
    {
        try
        {
        var user = await _userRepo.GetByIdAsync(userId) ?? throw new NotFoundException("KullanÄ±cÄ±", userId);
        user.EmailNotifications = dto.EmailNotifications; user.MarketingEmails = dto.MarketingEmails; user.SmsNotifications = dto.SmsNotifications;
        _userRepo.Update(user); await _userRepo.SaveChangesAsync();
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex) { await _logService.LogFunctionErrorAsync(EC_UPDATENOTIF, ex, dto, userId); throw; }
    }
}
