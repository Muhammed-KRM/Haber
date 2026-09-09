using FluentAssertions;
using KursuTV.API.Controllers;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _configMock.Setup(c => c["Jwt:Key"]).Returns("TestSecretKeyForJwtTokenGenerationMustBeLongEnough123!");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("https://kursutv.com");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("https://kursutv.com");
        _configMock.Setup(c => c["Jwt:AccessTokenExpirationMinutes"]).Returns("60");

        _controller = new AuthController(_authServiceMock.Object, _configMock.Object);
    }

    [Fact]
    public async Task Register_WhenRegistrationFails_ShouldReturnBadRequest()
    {
        var dto = new UserRegisterDto { Email = "fail@kursu.tv" };
        _authServiceMock
            .Setup(s => s.RegisterAsync(dto))
            .ReturnsAsync(new AuthResultDto { Success = false, ErrorMessage = "E-posta zaten kayıtlı." });

        var result = await _controller.Register(dto);

        var badRequestResult = result.Result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Register_WhenSuccessful_ShouldGenerateTokenAndReturnOk()
    {
        var dto = new UserRegisterDto { Email = "new@kursu.tv", FullName = "Yeni Muhabir" };
        var authUser = new UserDto { Id = Guid.NewGuid(), Email = dto.Email, FullName = dto.FullName, Role = "Reporter" };
        _authServiceMock
            .Setup(s => s.RegisterAsync(dto))
            .ReturnsAsync(new AuthResultDto { Success = true, User = authUser });

        var result = await _controller.Register(dto);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var responseDto = okResult.Value as AuthResultDto;
        responseDto.Should().NotBeNull();
        responseDto!.Token.Should().NotBeNullOrWhiteSpace();
        responseDto.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WhenCredentialsInvalid_ShouldReturnUnauthorized()
    {
        var dto = new UserLoginDto { Email = "bad@kursu.tv", Password = "wrong" };
        _authServiceMock
            .Setup(s => s.LoginAsync(dto))
            .ReturnsAsync(new AuthResultDto { Success = false, ErrorMessage = "Hatalı şifre." });

        var result = await _controller.Login(dto);

        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Login_WhenCredentialsValid_ShouldGenerateTokensAndReturnOk()
    {
        var dto = new UserLoginDto { Email = "admin@kursu.tv", Password = "correct" };
        var authUser = new UserDto { Id = Guid.NewGuid(), Email = dto.Email, FullName = "Admin", Role = "Admin" };
        _authServiceMock
            .Setup(s => s.LoginAsync(dto))
            .ReturnsAsync(new AuthResultDto { Success = true, User = authUser });

        var result = await _controller.Login(dto);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);

        var responseDto = okResult.Value as AuthResultDto;
        responseDto.Should().NotBeNull();
        responseDto!.Token.Should().NotBeNullOrWhiteSpace();
        responseDto.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }
}
