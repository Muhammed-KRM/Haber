using FluentAssertions;
using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Interfaces;
using KursuTV.Business.Services;
using KursuTV.Data.Entities;
using KursuTV.Data.Enums;
using KursuTV.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Services;

public class NewsManagerTests
{
    private readonly Mock<INewsRepository> _newsRepoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<ITagRepository> _tagRepoMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly Mock<ILogger<NewsManager>> _loggerMock = new();
    private readonly NewsManager _manager;

    public NewsManagerTests()
    {
        _manager = new NewsManager(
            _newsRepoMock.Object,
            _categoryRepoMock.Object,
            _tagRepoMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetHeadlinesAsync_WhenCacheHit_ShouldReturnCachedData()
    {
        // Arrange
        var cachedHeadlines = new List<HeadlineNewsDto>
        {
            new(Guid.NewGuid(), "Manşet 1", "manset-1", "Özet", "img.jpg", "alt", 1, DateTime.UtcNow, "Gündem", "gundem")
        };

        _cacheServiceMock
            .Setup(c => c.GetAsync<List<HeadlineNewsDto>>("news:headlines"))
            .ReturnsAsync(cachedHeadlines);

        // Act
        var result = await _manager.GetHeadlinesAsync(7);

        // Assert
        result.Should().BeEquivalentTo(cachedHeadlines);
        _newsRepoMock.Verify(r => r.GetHeadlinesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetHeadlinesAsync_WhenCacheMiss_ShouldFetchFromDbAndSetCache()
    {
        // Arrange
        _cacheServiceMock
            .Setup(c => c.GetAsync<List<HeadlineNewsDto>>("news:headlines"))
            .ReturnsAsync((List<HeadlineNewsDto>?)null);

        var newsList = new List<News>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Haber Başlığı",
                Slug = "haber-basligi",
                Spot = "Spot",
                Status = NewsStatus.Published,
                HeadlineOrder = 1,
                NewsCategories = new List<NewsCategory>()
            }
        };

        _newsRepoMock
            .Setup(r => r.GetHeadlinesAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newsList);

        // Act
        var result = await _manager.GetHeadlinesAsync(7);

        // Assert
        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Haber Başlığı");
        _cacheServiceMock.Verify(c => c.SetAsync("news:headlines", It.IsAny<List<HeadlineNewsDto>>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task GetBySlugAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        _newsRepoMock
            .Setup(r => r.GetBySlugAsync("olmayan-haber", false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((News?)null);

        // Act
        var result = await _manager.GetBySlugAsync("olmayan-haber");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateNewsAsync_WithValidData_ShouldCreateNewsAndSave()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var dto = new NewsCreateDto
        {
            Title = "Geleceğin Teknolojileri",
            Content = "<p>Kapsamlı teknoloji analizi</p>",
            Spot = "Kısa özet",
            CategoryIds = new List<int> { 1 },
            TagNames = new List<string> { "Teknoloji" },
            Status = NewsStatus.Published
        };

        _newsRepoMock
            .Setup(r => r.GetBySlugAsync(It.IsAny<string>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((News?)null);

        _newsRepoMock
            .Setup(r => r.AddAsync(It.IsAny<News>()))
            .ReturnsAsync((News n) => n);

        _newsRepoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _tagRepoMock
            .Setup(r => r.GetOrCreateTagsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tag> { new() { Id = 1, Name = "Teknoloji", Slug = "teknoloji" } });

        // Act
        var newsId = await _manager.CreateNewsAsync(dto, authorId);

        // Assert
        newsId.Should().NotBeEmpty();
        _newsRepoMock.Verify(r => r.AddAsync(It.Is<News>(n => n.AuthorId == authorId && n.Slug == "gelecegin-teknolojileri")), Times.Once);
        _newsRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPatternAsync("news:*"), Times.Once);
    }

    [Fact]
    public async Task DeleteNewsAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var newsId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();

        _newsRepoMock
            .Setup(r => r.GetByIdAsync(newsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((News?)null);

        // Act & Assert
        var action = () => _manager.DeleteNewsAsync(newsId, currentUserId);
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteNewsAsync_WhenExists_ShouldDeleteAndInvalidateCache()
    {
        // Arrange
        var newsId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var news = new News { Id = newsId, Title = "Silinecek Haber", AuthorId = currentUserId };

        _newsRepoMock
            .Setup(r => r.GetByIdAsync(newsId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(news);

        _newsRepoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _manager.DeleteNewsAsync(newsId, currentUserId);

        // Assert
        _newsRepoMock.Verify(r => r.Delete(news), Times.Once);
        _newsRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPatternAsync("news:*"), Times.Once);
    }

    [Fact]
    public async Task PublishScheduledNewsAsync_ShouldCallRepositoryAndInvalidateCache()
    {
        // Arrange
        _newsRepoMock
            .Setup(r => r.PublishScheduledNewsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var count = await _manager.PublishScheduledNewsAsync();

        // Assert
        count.Should().Be(3);
        _cacheServiceMock.Verify(c => c.RemoveAsync("news:headlines"), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("news:breaking"), Times.Once);
    }
}
