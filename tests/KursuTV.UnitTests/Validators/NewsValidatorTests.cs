using FluentAssertions;
using KursuTV.Business.DTOs;
using KursuTV.Business.Validators;
using Xunit;

namespace KursuTV.UnitTests.Validators;

public class NewsValidatorTests
{
    private readonly NewsCreateValidator _createValidator = new();
    private readonly NewsUpdateValidator _updateValidator = new();

    [Fact]
    public void NewsCreateValidator_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new NewsCreateDto
        {
            Title = "Türkiye'de Yapay Zeka Devrimi Başladı",
            Content = "<p>Yapay zeka projeleri hızla büyüyor ve teknoloji sektörü dönüşüyor.</p>",
            Spot = "Yapay zeka alanındaki son gelişmeler ve ulusal teknoloji hamlesi.",
            CategoryIds = new List<int> { 1, 2 },
            TagNames = new List<string> { "Teknoloji", "AI" }
        };

        // Act
        var result = _createValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void NewsCreateValidator_WhenTitleIsEmpty_ShouldFailValidation(string? invalidTitle)
    {
        // Arrange
        var dto = new NewsCreateDto
        {
            Title = invalidTitle!,
            Content = "İçerik var",
            CategoryIds = new List<int> { 1 }
        };

        // Act
        var result = _createValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(NewsCreateDto.Title));
    }

    [Fact]
    public void NewsCreateValidator_WhenTitleExceeds300Chars_ShouldFailValidation()
    {
        // Arrange
        var dto = new NewsCreateDto
        {
            Title = new string('A', 301),
            Content = "İçerik var",
            CategoryIds = new List<int> { 1 }
        };

        // Act
        var result = _createValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(NewsCreateDto.Title));
    }

    [Fact]
    public void NewsCreateValidator_WhenContentIsEmpty_ShouldFailValidation()
    {
        // Arrange
        var dto = new NewsCreateDto
        {
            Title = "Geçerli Başlık",
            Content = "",
            CategoryIds = new List<int> { 1 }
        };

        // Act
        var result = _createValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(NewsCreateDto.Content));
    }

    [Fact]
    public void NewsCreateValidator_WhenCategoryIdsIsEmpty_ShouldFailValidation()
    {
        // Arrange
        var dto = new NewsCreateDto
        {
            Title = "Geçerli Başlık",
            Content = "Geçerli İçerik",
            CategoryIds = new List<int>()
        };

        // Act
        var result = _createValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(NewsCreateDto.CategoryIds));
    }

    [Fact]
    public void NewsUpdateValidator_WhenIdIsEmpty_ShouldFailValidation()
    {
        // Arrange
        var dto = new NewsUpdateDto
        {
            Id = Guid.Empty,
            Title = "Güncel Başlık",
            Content = "Güncel İçerik",
            CategoryIds = new List<int> { 1 }
        };

        // Act
        var result = _updateValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(NewsUpdateDto.Id));
    }

    [Fact]
    public void NewsUpdateValidator_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new NewsUpdateDto
        {
            Id = Guid.NewGuid(),
            Title = "Güncel Başlık",
            Content = "Güncel İçerik",
            Spot = "Güncel Spot",
            CategoryIds = new List<int> { 1 }
        };

        // Act
        var result = _updateValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
