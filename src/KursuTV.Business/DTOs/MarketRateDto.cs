namespace KursuTV.Business.DTOs;

/// <summary>
/// Finans ve piyasa göstergelerini (Döviz, Altın, Kripto) taşıyan DTO.
/// </summary>
public record MarketRateDto(
    string Code,
    string Name,
    string Symbol,
    decimal Price,
    decimal ChangeRate,
    string ColorClass
);
