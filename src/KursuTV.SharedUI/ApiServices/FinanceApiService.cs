using System.Net.Http.Json;
using KursuTV.Business.DTOs;

namespace KursuTV.SharedUI.ApiServices;

/// <summary>
/// Backend'den canlı/önbellekli piyasa döviz ve kripto verilerini çeken servis.
/// </summary>
public class FinanceApiService
{
    private readonly HttpClient _http;

    public FinanceApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<MarketRateDto>> GetRatesAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketRateDto>>("api/finance/rates") ?? new();
        }
        catch
        {
            return new();
        }
    }
}
