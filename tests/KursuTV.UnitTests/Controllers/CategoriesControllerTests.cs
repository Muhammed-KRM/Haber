using FluentAssertions;
using KursuTV.API.Controllers;
using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace KursuTV.UnitTests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _categoryServiceMock = new();
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _controller = new CategoriesController(_categoryServiceMock.Object);
    }

    [Fact]
    public async Task GetCategoryTree_ShouldReturnOkWithTree()
    {
        var tree = new List<CategoryDto> { new(1, "Gündem", "gundem", null, 1, true, null, null) };
        _categoryServiceMock
            .Setup(s => s.GetAllActiveTreeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tree);

        var result = await _controller.GetCategoryTree();

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().BeEquivalentTo(tree);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNotFound()
    {
        _categoryServiceMock
            .Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryDto?)null);

        var result = await _controller.GetById(999);

        var notFoundResult = result.Result as NotFoundResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtAction()
    {
        var dto = new CreateCategoryDto("Ekonomi", null, null, 1, true, null);
        _categoryServiceMock
            .Setup(s => s.CreateCategoryAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(42);

        var result = await _controller.Create(dto);

        var createdResult = result.Result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
        createdResult.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(42);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent()
    {
        var result = await _controller.Delete(5);

        var noContentResult = result as NoContentResult;
        noContentResult.Should().NotBeNull();
        noContentResult!.StatusCode.Should().Be(204);
        _categoryServiceMock.Verify(s => s.DeleteCategoryAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }
}
