namespace KursuTV.Business.Events;

public class SendNotificationEvent
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public string? ActionUrl { get; set; }
    public bool SendEmail { get; set; } = true;
    public bool SendSms { get; set; } = false;
    public bool SendPush { get; set; } = true;
    // E-posta/SMS/Push iÃ§in kullanÄ±cÄ± bilgisi (DB'ye gitmemek iÃ§in)
    public string? UserEmail { get; set; }
    public string? UserPhone { get; set; }
    public string? FcmToken { get; set; }
    /// <summary>Duplicate Ã¶nleme â€” aynÄ± key ile ikinci kez bildirim yazÄ±lmaz</summary>
    public string? IdempotencyKey { get; set; }
}
