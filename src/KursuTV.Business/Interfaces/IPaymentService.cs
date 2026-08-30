namespace KursuTV.Business.Interfaces;

/// <summary>
/// Strategy Pattern: Ã–deme saÄŸlayÄ±cÄ±larÄ±nÄ±n ortak arayÃ¼zÃ¼.
/// PayTR, Iyzico, Stripe gibi farklÄ± saÄŸlayÄ±cÄ±lar bu arayÃ¼zÃ¼ implemente eder.
/// </summary>
public interface IPaymentService
{
    /// <summary>SaÄŸlayÄ±cÄ± adÄ± (PayTR, Iyzico, Stripe vb.)</summary>
    string ProviderName { get; }

    /// <summary>Ã–deme baÅŸlatÄ±r ve yÃ¶nlendirme URL'i dÃ¶ndÃ¼rÃ¼r.</summary>
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);

    /// <summary>Ã–deme callback'ini (webhook) doÄŸrular.</summary>
    Task<bool> VerifyCallbackAsync(Dictionary<string, string> callbackData);

    /// <summary>Ä°ade iÅŸlemi yapar.</summary>
    Task<bool> RefundAsync(string transactionId, decimal amount);
}

/// <summary>
/// Factory Pattern: Ãœlke koduna gÃ¶re doÄŸru Ã¶deme saÄŸlayÄ±cÄ±sÄ±nÄ± seÃ§er.
/// </summary>
public interface IPaymentServiceFactory
{
    IPaymentService GetPaymentService(string countryCode = "TR");
}

// â”€â”€â”€ DTO'lar â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

public class PaymentRequest
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public string Description { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string? BuyerEmail { get; set; }
    public string? BuyerName { get; set; }
    public string? BuyerIp { get; set; }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? RedirectUrl { get; set; }
    public string? ErrorMessage { get; set; }
}
