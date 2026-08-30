using System.ComponentModel.DataAnnotations;
using KursuTV.Data.Enums;

namespace KursuTV.Business.DTOs;

// Ä°lan detay gÃ¶rÃ¼ntÃ¼leme
public class ListingDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string? OwnerImageUrl { get; set; }
    public ListingType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HourlyPrice { get; set; }
    public LessonType LessonType { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public bool IsVitrin { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public ListingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> ImageUrls { get; set; } = new();

    // Madde 7 â€” Yeni alanlar
    public string? EducationLevel { get; set; }
    public int? ExperienceYears { get; set; }
    public int LessonDurationMinutes { get; set; } = 60;
    public bool IsGroupLesson { get; set; }
    public int? MaxGroupSize { get; set; }
    public bool HasTrialLesson { get; set; }
    public string? EducationBackground { get; set; }
    // SÄ±nÄ±f aralÄ±ÄŸÄ±
    public int? GradeMin { get; set; }
    public int? GradeMax { get; set; }
    // Moderasyon mesajÄ± â€” ilan Pending'e dÃ¼ÅŸtÃ¼ÄŸÃ¼nde kullanÄ±cÄ±ya gÃ¶sterilir
    public string? ModerationMessage { get; set; }
}

// Ä°lan oluÅŸturma formu
public class ListingCreateDto
{
    public ListingType Type { get; set; }

    [Required(ErrorMessage = "Ä°lan baÅŸlÄ±ÄŸÄ± zorunludur.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "BaÅŸlÄ±k 5-100 karakter arasÄ±nda olmalÄ±dÄ±r.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "AÃ§Ä±klama zorunludur.")]
    [StringLength(1000, MinimumLength = 20, ErrorMessage = "AÃ§Ä±klama 20-1000 karakter arasÄ±nda olmalÄ±dÄ±r.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Saatlik Ã¼cret zorunludur.")]
    [Range(1, 10000, ErrorMessage = "Saatlik Ã¼cret 1-10.000 TL arasÄ±nda olmalÄ±dÄ±r.")]
    public int HourlyPrice { get; set; }

    public LessonType LessonType { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "LÃ¼tfen bir branÅŸ seÃ§iniz.")]
    public int BranchId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "LÃ¼tfen bir ilÃ§e seÃ§iniz.")]
    public int DistrictId { get; set; }

    // Madde 7 â€” Yeni alanlar (CreateDto)
    public string? EducationLevel { get; set; }
    [Range(0, 50)] public int? ExperienceYears { get; set; }
    public int LessonDurationMinutes { get; set; } = 60;
    public bool IsGroupLesson { get; set; } = false;
    public int? MaxGroupSize { get; set; }
    public bool HasTrialLesson { get; set; } = false;
    public string? EducationBackground { get; set; }
    public int? GradeMin { get; set; }
    public int? GradeMax { get; set; }
    public List<string> ImageUrls { get; set; } = new();
}

// Ä°lan gÃ¼ncelleme formu
public class ListingUpdateDto
{
    public ListingType Type { get; set; }

    [Required(ErrorMessage = "Ä°lan baÅŸlÄ±ÄŸÄ± zorunludur.")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "BaÅŸlÄ±k 5-100 karakter arasÄ±nda olmalÄ±dÄ±r.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "AÃ§Ä±klama zorunludur.")]
    [StringLength(1000, MinimumLength = 20, ErrorMessage = "AÃ§Ä±klama 20-1000 karakter arasÄ±nda olmalÄ±dÄ±r.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Saatlik Ã¼cret zorunludur.")]
    [Range(1, 10000, ErrorMessage = "Saatlik Ã¼cret 1-10.000 TL arasÄ±nda olmalÄ±dÄ±r.")]
    public int HourlyPrice { get; set; }

    public LessonType LessonType { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "LÃ¼tfen bir branÅŸ seÃ§iniz.")]
    public int BranchId { get; set; }

    public int CityId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "LÃ¼tfen bir ilÃ§e seÃ§iniz.")]
    public int DistrictId { get; set; }

    public bool IsActive { get; set; }

    // Madde 7 â€” Yeni alanlar (UpdateDto)
    public string? EducationLevel { get; set; }
    [Range(0, 50)] public int? ExperienceYears { get; set; }
    public int LessonDurationMinutes { get; set; } = 60;
    public bool IsGroupLesson { get; set; } = false;
    public int? MaxGroupSize { get; set; }
    public bool HasTrialLesson { get; set; } = false;
    public string? EducationBackground { get; set; }
    public int? GradeMin { get; set; }
    public int? GradeMax { get; set; }
}
