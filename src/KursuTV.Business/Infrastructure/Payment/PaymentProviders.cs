using Microsoft.Extensions.Logging;
using KursuTV.Business.Interfaces;

namespace KursuTV.Business.Infrastructure.Payment;

/// <summary>
/// PayTR Stub: GerÃ§ek PayTR API'si entegre edilene kadar loglama yapan geÃ§ici saÄŸlayÄ±cÄ±.
/// PayTR hesabÄ±nÄ±z hazÄ±r olduÄŸunda bu sÄ±nÄ±fÄ±n iÃ§ini gerÃ§ek API Ã§aÄŸrÄ±larÄ±yla dolduracaksÄ±nÄ±z.
/// </summary>
public class PayTRPaymentService : IPaymentService
{
    private readonly ILogger<PayTRPaymentService> _logger;

    public PayTRPaymentService(ILogger<PayTRPaymentService> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "PayTR";

    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        _logger.LogInformation("[PayTR] Ã–deme baÅŸlatÄ±lÄ±yor: {Amount} {Currency} - KullanÄ±cÄ±: {UserId}",
            request.Amount, request.Currency, request.UserId);

        // TODO: GerÃ§ek PayTR API entegrasyonu
        // 1. PayTR Merchant Key, Salt gibi bilgiler appsettings.json'dan alÄ±nacak
        // 2. PayTR iframe token API'sine istek atÄ±lacak
        // 3. DÃ¶nen token ile iframe URL oluÅŸturulacak

        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = $"STUB-{Guid.NewGuid():N}",
            RedirectUrl = $"/fake-payment?returnUrl={System.Net.WebUtility.UrlEncode(request.ReturnUrl)}",
            ErrorMessage = null
        });
    }

    public Task<bool> VerifyCallbackAsync(Dictionary<string, string> callbackData)
    {
        _logger.LogInformation("Webhook doÄŸrulanÄ±yor...");
        return Task.FromResult(true);
    }

    public Task<bool> RefundAsync(string transactionId, decimal amount)
    {
        _logger.LogInformation("Ä°ade iÅŸlemi: {TransactionId}, Tutar: {Amount}", transactionId, amount);
        return Task.FromResult(true);
    }
}

public class StripePaymentService : IPaymentService
{
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(ILogger<StripePaymentService> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Stripe";

    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        return Task.FromResult(new PaymentResult
        {
            Success = true,
            TransactionId = $"STRIPE-STUB-{Guid.NewGuid():N}",
            RedirectUrl = $"/fake-payment?returnUrl={System.Net.WebUtility.UrlEncode(request.ReturnUrl)}",
            ErrorMessage = null
        });
    }

    public Task<bool> VerifyCallbackAsync(Dictionary<string, string> callbackData)
    {
        return Task.FromResult(true);
    }

    public Task<bool> RefundAsync(string transactionId, decimal amount)
    {
        return Task.FromResult(true);
    }
}

/// <summary>
/// Factory Pattern: Ãœlke koduna gÃ¶re Ã¶deme saÄŸlayÄ±cÄ±sÄ±nÄ± seÃ§er.
/// TR â†’ PayTR, diÄŸer Ã¼lkeler â†’ Stripe
/// </summary>
public class PaymentServiceFactory : IPaymentServiceFactory
{
    private readonly IEnumerable<IPaymentService> _services;
    private readonly ILogger<PaymentServiceFactory> _logger;

    public PaymentServiceFactory(IEnumerable<IPaymentService> services, ILogger<PaymentServiceFactory> logger)
    {
        _services = services;
        _logger = logger;
    }

    public IPaymentService GetPaymentService(string countryCode = "TR")
    {
        var providerName = countryCode == "TR" ? "PayTR" : "Stripe";
        var service = _services.FirstOrDefault(s => s.ProviderName == providerName);

        if (service == null)
        {
            _logger.LogWarning("Ã–deme saÄŸlayÄ±cÄ±sÄ± bulunamadÄ±: {Provider}, varsayÄ±lan (PayTR) kullanÄ±lÄ±yor.", providerName);
            service = _services.First();
        }

        _logger.LogInformation("Ã–deme saÄŸlayÄ±cÄ±sÄ± seÃ§ildi: {Provider} (Ãœlke: {Country})", service.ProviderName, countryCode);
        return service;
    }
}
