using FluentAssertions;
using KursuTV.Business.Interfaces;
using KursuTV.Business.Services;
using KursuTV.Data.Entities;
using KursuTV.Data.Repositories;
using MassTransit;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Services;

public class ModerationManagerTests
{
    private readonly Mock<IRepository<User>> _userRepoMock = new();
    private readonly Mock<IRepository<ViolationLog>> _violationRepoMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ILogService> _logServiceMock = new();
    private readonly ModerationManager _manager;

    public ModerationManagerTests()
    {
        _manager = new ModerationManager(
            _userRepoMock.Object,
            _violationRepoMock.Object,
            _publishEndpointMock.Object,
            _logServiceMock.Object
        );
    }

    [Fact]
    public void CheckContent_WithCleanText_ShouldReturnClean()
    {
        var title = "Normal bir haber başlığı";
        var description = "Burada herhangi bir telefon veya yasaklı link yer almamaktadır.";

        var result = _manager.CheckContent(title, description);

        result.IsViolation.Should().BeFalse();
        result.ViolationType.Should().BeNull();
    }

    [Theory]
    [InlineData("Beni ara: 0532 111 22 33")]
    [InlineData("Numaram: 0555-444-33-22")]
    [InlineData("İletişim: 05441234567")]
    public void CheckContent_WithPhoneNumber_ShouldDetectViolation(string phoneText)
    {
        var result = _manager.CheckContent("İlan Başlığı", phoneText);

        result.IsViolation.Should().BeTrue();
        result.ViolationType.Should().Be("Phone");
    }

    [Theory]
    [InlineData("Bana mail at: test.user@example.com")]
    [InlineData("İletişim: info@sirket.com.tr")]
    public void CheckContent_WithEmail_ShouldDetectViolation(string emailText)
    {
        var result = _manager.CheckContent("İlan Başlığı", emailText);

        result.IsViolation.Should().BeTrue();
        result.ViolationType.Should().Be("Email");
    }

    [Theory]
    [InlineData("Sitemize git: https://spam-site.com/kayit")]
    [InlineData("Tıkla www.google-analiz.com")]
    public void CheckContent_WithExternalLink_ShouldDetectViolation(string linkText)
    {
        var result = _manager.CheckContent("İlan Başlığı", linkText);

        result.IsViolation.Should().BeTrue();
        result.ViolationType.Should().Be("Link");
    }
}
