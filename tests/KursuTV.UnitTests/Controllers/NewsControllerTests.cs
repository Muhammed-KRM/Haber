using FluentAssertions;
using KursuTV.API.Controllers;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Controllers;

public class NewsControllerTests
{
    private readonly Mock<INewsService> _newsServiceMock = new();
    private readonly NewsController _controller;

    public NewsControllerTests()
    {
        _controller = new NewsController(_newsServiceMock.Object);
    }

    [Fact]
    public async Task GetHeadlines_ShouldReturnOkResult_WithHeadlines()
    {
        // Arrange
        var headlines = new List<HeadlineNewsDto>
        {
            new(Guid.NewGuid(), "Başlık 1", "baslik-1", "Spot", "img.jpg", "alt", 1, DateTime.UtcNow, "Gündem", "gundem")
        };

        _newsServiceMock
            .Setup(s => s.GetHeadlinesAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(headlines);

        // Act
        var result = await _controller.GetHeadlines(7);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(headlines);
    }

    [Fact]
    public async Task GetBreakingNews_ShouldReturnOkResult_WithBreakingNews()
    {
        // Arrange
        var breakingNews = new List<BreakingNewsDto>
        {
            new(Guid.NewGuid(), "Son Dakika Gelişmesi", "son-dakika", DateTime.UtcNow)
        };

        _newsServiceMock
            .Setup(s => s.GetBreakingNewsAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(breakingNews);

        // Act
        var result = await _controller.GetBreakingNews(10);

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetPublishedNews_WithFilters_ShouldCallServiceAndReturnOk()
    {
        // Arrange
        var pagedResult = new PagedResultDto<NewsListDto>(new List<NewsListDto>(), 0, 1, 10, 0);
        _newsServiceMock
            .Setup(s => s.GetPublishedNewsPagedAsync(1, 10, null, NewsType.Column, null, "ekonomi", It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _controller.GetPublishedNews(page: 1, pageSize: 10, type: NewsType.Column, search: "ekonomi");

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(pagedResult);
    }

    [Fact]
    public async Task GetBySlug_WhenNewsExists_ShouldReturnOkWithDetail()
    {
        // Arrange
        var detail = new NewsDetailDto(
            Id: Guid.NewGuid(),
            Title: "Haber Başlığı",
            Slug: "haber-basligi",
            Spot: "Spot",
            Content: "<p>İçerik</p>",
            CoverImageUrl: "img.jpg",
            CoverImageAlt: "alt",
            MetaTitle: "Meta",
            MetaDescription: "Meta Desc",
            Status: NewsStatus.Published,
            Type: NewsType.Article,
            IsBreaking: false,
            HeadlineOrder: 0,
            ViewCount: 0,
            PublishedAt: DateTime.UtcNow,
            CreatedAt: DateTime.UtcNow,
            AuthorId: Guid.NewGuid(),
            AuthorName: "Editör",
            AuthorProfileImageUrl: null,
            AuthorBio: "Bio",
            Categories: new List<CategoryDto>(),
            Tags: new List<TagDto>(),
            Comments: new List<CommentDto>()
        );

        _newsServiceMock
            .Setup(s => s.GetBySlugAsync("haber-basligi", It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

        // Act
        var result = await _controller.GetBySlug("haber-basligi");

        // Assert
        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(detail);
    }

    [Fact]
    public async Task GetBySlug_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _newsServiceMock
            .Setup(s => s.GetBySlugAsync("olmayan-haber", It.IsAny<CancellationToken>()))
            .ReturnsAsync((NewsDetailDto?)null);

        // Act
        var result = await _controller.GetBySlug("olmayan-haber");

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task RecordView_ShouldCallServiceAndReturnNoContent()
    {
        // Arrange
        var newsId = Guid.NewGuid();

        // Act
        var result = await _controller.RecordView(newsId);

        // Assert
        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
        _newsServiceMock.Verify(s => s.RecordViewAsync(newsId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
