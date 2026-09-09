using FluentAssertions;
using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Interfaces;
using KursuTV.Business.Services;
using KursuTV.Data.Entities;
using KursuTV.Data.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Services;

public class CategoryManagerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly Mock<ILogger<CategoryManager>> _loggerMock = new();
    private readonly CategoryManager _manager;

    public CategoryManagerTests()
    {
        _manager = new CategoryManager(
            _categoryRepoMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetAllActiveTreeAsync_WhenCacheHit_ShouldReturnCachedDataWithoutDbQuery()
    {
        // Arrange
        var cachedData = new List<CategoryDto>
        {
            new(1, "Gündem", "gundem", null, 1, true, null, null)
        };

        _cacheServiceMock
            .Setup(c => c.GetAsync<List<CategoryDto>>("categories:tree"))
            .ReturnsAsync(cachedData);

        // Act
        var result = await _manager.GetAllActiveTreeAsync();

        // Assert
        result.Should().BeEquivalentTo(cachedData);
        _categoryRepoMock.Verify(r => r.GetActiveCategoriesWithChildrenAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllActiveTreeAsync_WhenCacheMiss_ShouldFetchFromDbAndSetCache()
    {
        // Arrange
        _cacheServiceMock
            .Setup(c => c.GetAsync<List<CategoryDto>>("categories:tree"))
            .ReturnsAsync((List<CategoryDto>?)null);

        var dbCategories = new List<Category>
        {
            new() { Id = 1, Name = "Spor", Slug = "spor", IsActive = true, SortOrder = 1, Children = new List<Category>() }
        };

        _categoryRepoMock
            .Setup(r => r.GetActiveCategoriesWithChildrenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbCategories);

        // Act
        var result = await _manager.GetAllActiveTreeAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Spor");
        _cacheServiceMock.Verify(c => c.SetAsync("categories:tree", It.IsAny<List<CategoryDto>>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task GetBySlugAsync_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        _categoryRepoMock
            .Setup(r => r.GetBySlugAsync("olmayan-kategori", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _manager.GetBySlugAsync("olmayan-kategori");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldGenerateSlugAndSave()
    {
        var dto = new CreateCategoryDto("Teknoloji Dünyası", null, null, 1, true, null);
        _categoryRepoMock
            .Setup(r => r.SlugExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Callback<Category>(c => c.Id = 10)
            .ReturnsAsync((Category c) => c);

        _categoryRepoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var resultId = await _manager.CreateCategoryAsync(dto);

        // Assert
        resultId.Should().Be(10);
        _categoryRepoMock.Verify(r => r.AddAsync(It.Is<Category>(c => c.Slug == "teknoloji-dunyasi")), Times.Once);
        _categoryRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        var action = () => _manager.DeleteCategoryAsync(999);
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenValid_ShouldDeleteAndInvalidateCache()
    {
        // Arrange
        var category = new Category { Id = 5, Name = "Magazin", NewsCategories = new List<NewsCategory>() };
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepoMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        await _manager.DeleteCategoryAsync(5);

        // Assert
        _categoryRepoMock.Verify(r => r.Delete(category), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("categories:tree"), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("categories:flat"), Times.Once);
    }
}
