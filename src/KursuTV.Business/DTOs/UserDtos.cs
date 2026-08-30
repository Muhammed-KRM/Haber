using System.ComponentModel.DataAnnotations;

namespace KursuTV.Business.DTOs;

// KullanÄ±cÄ± bilgilerini dÄ±ÅŸarÄ±ya taÅŸÄ±yan DTO
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsTeacherProfileComplete { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    public int TokenBalance { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// KayÄ±t formu
public class UserRegisterDto
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad Soyad 2-100 karakter arasÄ±nda olmalÄ±dÄ±r.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "GeÃ§erli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Åifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Åifre en az 6 karakter olmalÄ±dÄ±r.")]
    public string Password { get; set; } = string.Empty;
}

// GiriÅŸ formu
public class UserLoginDto
{
    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "GeÃ§erli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Åifre zorunludur.")]
    public string Password { get; set; } = string.Empty;
}

// JWT sonucu
public class AuthResultDto
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public string? ErrorMessage { get; set; }
}

// Profil gÃ¼ncelleme formu
public class UserProfileUpdateDto
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad Soyad 2-100 karakter arasÄ±nda olmalÄ±dÄ±r.")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Biyografi en fazla 500 karakter olabilir.")]
    public string? Bio { get; set; }

    [Phone(ErrorMessage = "GeÃ§erli bir telefon numarasÄ± giriniz.")]
    public string? Phone { get; set; }
}

// Refresh token isteÄŸi
public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class PersonalInfoDto
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "GeÃ§erli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "GeÃ§erli bir telefon numarasÄ± giriniz.")]
    public string Phone { get; set; } = string.Empty;

    public DateTime? BirthDate { get; set; }
}

public class PaymentInfoDto
{
    [Required(ErrorMessage = "IBAN zorunludur.")]
    [StringLength(34, MinimumLength = 26, ErrorMessage = "GeÃ§erli bir IBAN giriniz.")]
    public string IBAN { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hesap sahibi adÄ± zorunludur.")]
    public string AccountHolderName { get; set; } = string.Empty;
}

public class PasswordChangeDto
{
    [Required(ErrorMessage = "Mevcut ÅŸifre zorunludur.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni ÅŸifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Yeni ÅŸifre en az 6 karakter olmalÄ±dÄ±r.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Åifre tekrarÄ± zorunludur.")]
    [Compare("NewPassword", ErrorMessage = "Åifreler eÅŸleÅŸmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class NotificationSettingsDto
{
    public bool EmailNotifications { get; set; }
    public bool MarketingEmails { get; set; }
    public bool SmsNotifications { get; set; }
}

public class UserProfileDto
{
    public PersonalInfoDto PersonalInfo { get; set; } = new();
    public PaymentInfoDto PaymentInfo { get; set; } = new();
    public NotificationSettingsDto NotificationSettings { get; set; } = new();
}

public class UserActivityDto
{
    public string Icon { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
