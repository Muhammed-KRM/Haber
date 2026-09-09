using System.Net.Http.Json;
using System.Text.Json;
using KursuTV.Business.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KursuTV.Business.Services;

public class GeminiModerationService : IAiModerationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiModerationService> _logger;

    public GeminiModerationService(HttpClient httpClient, IConfiguration config, ILogger<GeminiModerationService> logger)
    {
        _httpClient = httpClient;
        _apiKey = config["AiSettings:GeminiApiKey"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<AiModerationResult> AnalyzeContentAsync(string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "YOUR_API_KEY_HERE")
        {
            // Fallback mock if no API key is provided
            _logger.LogWarning("Gemini API anahtarı bulunamadı. Basit kelime filtresi kullanılıyor.");
            var badWords = new[] { "aptal", "salak", "küfür", "hakaret", "gerizekalı", "şerefsiz", "yalan", "dolandırıcı" };
            var isSafeFallback = !badWords.Any(w => content.Contains(w, StringComparison.OrdinalIgnoreCase));
            return new AiModerationResult { IsSafe = isSafeFallback, Reason = isSafeFallback ? "" : "Uygunsuz kelime tespiti (Yerel Filtre)" };
        }

        var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
        
        var prompt = $"Aşağıdaki kullanıcı yorumu küfür, hakaret, aşağılama, nefret söylemi, spam veya yasadışı içerik barındırıyor mu? Sadece 'EVET' veya 'HAYIR' olarak cevap ver. Asla açıklama yapma. Yorum: \"{content}\"";

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(requestUrl, requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
            var textResponse = responseJson
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text").GetString()?.Trim().ToUpperInvariant() ?? "";

            var isSafe = !textResponse.Contains("EVET");

            return new AiModerationResult
            {
                IsSafe = isSafe,
                Reason = isSafe ? "" : "Yapay zeka tarafından uygunsuz içerik tespit edildi."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API çağrısı sırasında hata oluştu.");
            // Hata durumunda yorumun geçmesine izin veriyoruz, standart sürece düşüp admin onayı bekleyecek
            return new AiModerationResult { IsSafe = true, Reason = "API hatası, otomatik onay." };
        }
    }
}
