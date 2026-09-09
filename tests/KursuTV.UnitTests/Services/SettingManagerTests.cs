using FluentAssertions;
using KursuTV.Business.Services;
using KursuTV.Data.Entities;
using KursuTV.Data.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace KursuTV.UnitTests.Services;

public class SettingManagerTests
{
    private readonly Mock<IRepository<GlobalSetting>> _settingRepoMock = new();
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    private readonly SettingManager _manager;

    public SettingManagerTests()
    {
        _manager = new SettingManager(_settingRepoMock.Object, _memoryCache);
    }

    [Fact]
    public async Task GetSettingAsync_WhenSettingExistsInDb_ShouldReturnValAndCacheIt()
    {
        // Arrange
        var setting = new GlobalSetting { Key = "SiteTitle", Value = "Kürsü TV Haber" };
        _settingRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<GlobalSetting, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GlobalSetting> { setting });

        // Act
        var result = await _manager.GetSettingAsync("SiteTitle");

        // Assert
        result.Should().Be("Kürsü TV Haber");

        // Second call should return from memory cache without db query
        _settingRepoMock.Invocations.Clear();
        var cachedResult = await _manager.GetSettingAsync("SiteTitle");
        cachedResult.Should().Be("Kürsü TV Haber");
        _settingRepoMock.Verify(r => r.FindAsync(It.IsAny<Expression<Func<GlobalSetting, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSettingAsync_WhenSettingNotFound_ShouldReturnDefaultValue()
    {
        _settingRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<GlobalSetting, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GlobalSetting>());

        var result = await _manager.GetSettingAsync("NonExistentKey", "DefaultVal");

        result.Should().Be("DefaultVal");
    }

    [Fact]
    public async Task SetSettingAsync_WhenSettingDoesNotExist_ShouldAddNewSetting()
    {
        _settingRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<GlobalSetting, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GlobalSetting>());

        _settingRepoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        await _manager.SetSettingAsync("LogoUrl", "https://kursutv.com/logo.png", "Site Logosu");

        _settingRepoMock.Verify(r => r.AddAsync(It.Is<GlobalSetting>(s => s.Key == "LogoUrl" && s.Value == "https://kursutv.com/logo.png")), Times.Once);
        _settingRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
