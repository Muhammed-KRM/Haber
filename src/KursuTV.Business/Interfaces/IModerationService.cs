namespace KursuTV.Business.Interfaces;

public interface IModerationService
{
    /// <summary>
    /// Ä°lan iÃ§eriÄŸini Regex ile tarar. HÄ±zlÄ±, sync Ã§alÄ±ÅŸÄ±r.
    /// </summary>
    ModerationResult CheckContent(string title, string description);

    /// <summary>
    /// KullanÄ±cÄ±ya ihlal ekler, gerekirse ban uygular.
    /// </summary>
    Task AddStrikeAsync(Guid userId, Guid? listingId, string listingTitle,
        string violationType, string detectedContent, string detectedBy);
}

public record ModerationResult(bool IsViolation, string? ViolationType, string? Message)
{
    public static ModerationResult Clean() => new(false, null, null);
    public static ModerationResult Violation(string type, string msg) => new(true, type, msg);
}
