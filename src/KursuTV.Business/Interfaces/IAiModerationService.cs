namespace KursuTV.Business.Interfaces;

public class AiModerationResult
{
    public bool IsSafe { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public interface IAiModerationService
{
    Task<AiModerationResult> AnalyzeContentAsync(string content, CancellationToken cancellationToken = default);
}
