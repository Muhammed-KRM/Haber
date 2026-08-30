using Microsoft.Extensions.AI;
using OllamaSharp;

namespace KursuTV.Worker.Services;

public class OllamaService
{
    private readonly OllamaApiClient _client;
    private readonly ILogger<OllamaService> _logger;
    private const string Model = "phi3:mini";

    // Few-shot prompt â€” eÄŸitim yok, sadece Ã¶rnekler
    private const string SystemPrompt = """
        Sen bir iÃ§erik moderatÃ¶rÃ¼sÃ¼n. TÃ¼rkÃ§e ilan metinlerinde telefon numarasÄ±,
        e-posta adresi veya harici link olup olmadÄ±ÄŸÄ±nÄ± tespit ediyorsun.
        Sadece JSON formatÄ±nda yanÄ±t ver, baÅŸka hiÃ§bir ÅŸey yazma.

        Ã–rnekler:
        Metin: "Matematik dersi veriyorum, 10 yÄ±l deneyimim var"
        YanÄ±t: {"violation": false}

        Metin: "0532 123 45 67 numaralÄ± telefonu arayÄ±n"
        YanÄ±t: {"violation": true, "type": "phone"}

        Metin: "sÄ±fÄ±r beÅŸ Ã¼Ã§ iki bir iki Ã¼Ã§ dÃ¶rt beÅŸ altÄ± yedi"
        YanÄ±t: {"violation": true, "type": "phone"}

        Metin: "bilgi@gmail.com adresine yazÄ±n"
        YanÄ±t: {"violation": true, "type": "email"}

        Metin: "www.sitem.com adresimi ziyaret edin"
        YanÄ±t: {"violation": true, "type": "link"}

        Metin: "s-Ä±-f-Ä±-r b-e-ÅŸ Ã¼Ã§ iki..."
        YanÄ±t: {"violation": true, "type": "phone"}
        """;

    public OllamaService(IConfiguration config, ILogger<OllamaService> logger)
    {
        var ollamaUrl = config["Ollama:BaseUrl"] ?? "http://ollama:11434";
        _client = new OllamaApiClient(new Uri(ollamaUrl));
        _client.SelectedModel = Model;
        _logger = logger;
    }

    public async Task<OllamaModerationResult> AnalyzeAsync(string title, string description,
        CancellationToken ct = default)
    {
        try
        {
            var userMessage = $"Metin: \"{title} {description}\"";

            // OllamaSharp 5.x â€” IChatClient interface Ã¼zerinden chat
            IChatClient chatClient = _client;

            var messages = new List<ChatMessage>
            {
                new(ChatRole.System, SystemPrompt),
                new(ChatRole.User, userMessage)
            };

            var response = await chatClient.GetResponseAsync(messages, cancellationToken: ct);
            var json = response.Text?.Trim() ?? "{}";

            // JSON parse
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            var violation = root.TryGetProperty("violation", out var v) && v.GetBoolean();
            var type = root.TryGetProperty("type", out var t) ? t.GetString() : null;

            return new OllamaModerationResult(violation, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ollama analizi baÅŸarÄ±sÄ±z, temiz kabul ediliyor");
            // Ollama hata verirse ilanÄ± engelleme â€” false negative tercih edilir
            return new OllamaModerationResult(false, null);
        }
    }
}

public record OllamaModerationResult(bool IsViolation, string? ViolationType);
