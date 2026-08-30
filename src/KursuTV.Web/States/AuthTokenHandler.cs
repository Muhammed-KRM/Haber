using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace KursuTV.Web.States;

/// <summary>
/// Her HTTP isteÄŸine otomatik olarak Authorization: Bearer header'Ä± ekleyen handler.
/// LocalStorage'dan JWT Token'Ä± alÄ±r ve request header'larÄ±na koyar.
/// </summary>
public class AuthTokenHandler : DelegatingHandler
{
    private readonly ProtectedLocalStorage _localStorage;
    // JS Runtime kaldÄ±rÄ±ldÄ± Ã§Ã¼nkÃ¼ prerendering sÄ±rasÄ±nda circuit Ã§Ã¶kmesine neden oluyor

    public AuthTokenHandler(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _localStorage.GetAsync<UserSession>("UserSession");
            if (result.Success && result.Value != null && !string.IsNullOrEmpty(result.Value.Token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Value.Token);
            }
        }
        catch (Exception)
        {
            // Prerender sÄ±rasÄ±nda LocalStorage eriÅŸilemiyorsa sessizce geÃ§
        }

        var response = await base.SendAsync(request, cancellationToken);
        
        // 401/403 hatalarÄ±nÄ± yutma â€” Ã§aÄŸÄ±ran servisin ele almasÄ± iÃ§in response dÃ¶ndÃ¼r
        // Sadece gerÃ§ek API hatalarÄ± (400, 500 vb.) iÃ§in exception fÄ±rlat
        if (!response.IsSuccessStatusCode 
            && response.StatusCode != System.Net.HttpStatusCode.Unauthorized 
            && response.StatusCode != System.Net.HttpStatusCode.Forbidden)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"API HatasÄ± ({response.StatusCode}): {errorContent}");
        }

        return response;
    }
}
