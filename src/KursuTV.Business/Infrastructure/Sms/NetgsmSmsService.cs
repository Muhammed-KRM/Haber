using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using KursuTV.Business.Interfaces;

namespace KursuTV.Business.Infrastructure.Sms;

public class NetgsmSmsService : ISmsService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;
    private readonly ILogger<NetgsmSmsService> _logger;

    public NetgsmSmsService(IConfiguration config, IHttpClientFactory httpFactory, ILogger<NetgsmSmsService> logger)
    {
        _config = config;
        _http = httpFactory.CreateClient("Netgsm");
        _logger = logger;
    }

    public async Task SendAsync(string phoneNumber, string message)
    {
        var isEnabled = _config.GetValue<bool>("Netgsm:Enabled", false);
        if (!isEnabled)
        {
            _logger.LogInformation("SMS gÃ¶nderimi devre dÄ±ÅŸÄ±. Telefon: {Phone}, Mesaj: {Message}", phoneNumber, message);
            return;
        }

        var user = _config["Netgsm:Username"];
        var pass = _config["Netgsm:Password"];
        var header = _config["Netgsm:Header"] ?? "OZELDERS";

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            _logger.LogWarning("Netgsm kullanÄ±cÄ± adÄ± veya ÅŸifre eksik. SMS gÃ¶nderilemedi.");
            return;
        }

        try
        {
            // Telefon numarasÄ±nÄ± temizle (sadece rakamlar)
            var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // TÃ¼rkiye formatÄ±na Ã§evir (90 ile baÅŸlamalÄ±)
            if (cleanPhone.StartsWith("0"))
                cleanPhone = "90" + cleanPhone[1..];
            else if (!cleanPhone.StartsWith("90"))
                cleanPhone = "90" + cleanPhone;

            // Netgsm HTTP API
            var url = $"https://api.netgsm.com.tr/sms/send/get?" +
                      $"usercode={Uri.EscapeDataString(user)}&password={Uri.EscapeDataString(pass)}" +
                      $"&gsmno={cleanPhone}&message={Uri.EscapeDataString(message)}&msgheader={Uri.EscapeDataString(header)}";

            var response = await _http.GetAsync(url);
            var result = await response.Content.ReadAsStringAsync();

            if (result.StartsWith("00"))
            {
                _logger.LogInformation("SMS baÅŸarÄ±yla gÃ¶nderildi. Telefon: {Phone}", cleanPhone);
            }
            else
            {
                _logger.LogWarning("SMS gÃ¶nderimi baÅŸarÄ±sÄ±z. Telefon: {Phone}, Hata: {Error}", cleanPhone, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMS gÃ¶nderimi sÄ±rasÄ±nda hata. Telefon: {Phone}", phoneNumber);
        }
    }
}

