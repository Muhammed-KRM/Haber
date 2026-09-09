using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using KursuTV.API.Helpers;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Helpers;

public class JwtHelperTests
{
    [Fact]
    public void GenerateToken_ShouldCreateValidJwtWithExpectedClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "editor@kursutv.com";
        var fullName = "Ahmet Yılmaz";
        var role = "Editor";

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Key"]).Returns("ThisIsASecretSecurityKeyForTestingOnly1234567890!");
        configMock.Setup(c => c["Jwt:Issuer"]).Returns("https://kursutv.com");
        configMock.Setup(c => c["Jwt:Audience"]).Returns("https://kursutv.com");
        configMock.Setup(c => c["Jwt:AccessTokenExpirationMinutes"]).Returns("60");

        // Act
        var tokenString = JwtHelper.GenerateToken(userId, email, fullName, role, configMock.Object);

        // Assert
        tokenString.Should().NotBeNullOrWhiteSpace();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        token.Issuer.Should().Be("https://kursutv.com");
        token.Audiences.Should().Contain("https://kursutv.com");

        var claims = token.Claims.ToList();
        claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
        claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == email);
        claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == fullName);
        claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == role);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var refreshToken1 = JwtHelper.GenerateRefreshToken();
        var refreshToken2 = JwtHelper.GenerateRefreshToken();

        // Assert
        refreshToken1.Should().NotBeNullOrWhiteSpace();
        refreshToken2.Should().NotBeNullOrWhiteSpace();
        refreshToken1.Should().NotBe(refreshToken2);

        var bytes = Convert.FromBase64String(refreshToken1);
        bytes.Length.Should().Be(64);
    }
}
