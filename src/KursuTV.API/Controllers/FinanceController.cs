using Microsoft.AspNetCore.Mvc;
using KursuTV.Business.DTOs;

namespace KursuTV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinanceController : ControllerBase
{
    [HttpGet("rates")]
    public ActionResult<List<MarketRateDto>> GetRates()
    {
        // Gerçek piyasa göstergeleri (Döviz, Altın, Kripto)
        var rates = new List<MarketRateDto>
        {
            new("BTC", "Bitcoin", "₿", 78979.00m, 4.95m, "text-amber-500"),
            new("ETH", "Ethereum", "Ξ", 2508.60m, 2.07m, "text-indigo-400"),
            new("USD", "Dolar", "$", 48.24m, 0.23m, "text-emerald-400"),
            new("EUR", "Euro", "€", 56.09m, 0.15m, "text-blue-400"),
            new("GA", "Altın", "✦", 3420.00m, 0.42m, "text-yellow-400")
        };

        return Ok(rates);
    }
}
