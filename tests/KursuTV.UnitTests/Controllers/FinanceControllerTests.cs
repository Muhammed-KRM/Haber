using FluentAssertions;
using KursuTV.API.Controllers;
using KursuTV.Business.DTOs;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace KursuTV.UnitTests.Controllers;

public class FinanceControllerTests
{
    private readonly FinanceController _controller;

    public FinanceControllerTests()
    {
        _controller = new FinanceController();
    }

    [Fact]
    public void GetRates_ShouldReturnOkWithMarketRates()
    {
        // Act
        var result = _controller.GetRates();

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var rates = okResult.Value as List<MarketRateDto>;
        rates.Should().NotBeNull();
        rates!.Should().HaveCountGreaterThanOrEqualTo(5);
        rates.Should().Contain(r => r.Code == "BTC");
        rates.Should().Contain(r => r.Code == "USD");
        rates.Should().Contain(r => r.Code == "EUR");
    }
}
