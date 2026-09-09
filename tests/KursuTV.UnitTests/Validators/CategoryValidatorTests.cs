using FluentAssertions;
using KursuTV.Business.DTOs;
using KursuTV.Business.Validators;
using Xunit;

namespace KursuTV.UnitTests.Validators;

public class CategoryValidatorTests
{
    private readonly CreateCategoryValidator _createValidator = new();
    private readonly UpdateCategoryValidator _updateValidator = new();

    [Fact]
    public void CreateCategoryValidator_WithValidName_ShouldPass()
    {
        var dto = new CreateCategoryDto("Gündem", null, null, 1, true, null);
        var result = _createValidator.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateCategoryValidator_WithEmptyName_ShouldFail(string? name)
    {
        var dto = new CreateCategoryDto(name!, null, null, 1, true, null);
        var result = _createValidator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCategoryDto.Name));
    }

    [Fact]
    public void CreateCategoryValidator_WithNameExceeding100Chars_ShouldFail()
    {
        var dto = new CreateCategoryDto(new string('K', 101), null, null, 1, true, null);
        var result = _createValidator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCategoryDto.Name));
    }

    [Fact]
    public void UpdateCategoryValidator_WithValidData_ShouldPass()
    {
        var dto = new UpdateCategoryDto(5, "Ekonomi ve Finans", null, null, 1, true, null);
        var result = _updateValidator.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateCategoryValidator_WithInvalidId_ShouldFail()
    {
        var dto = new UpdateCategoryDto(0, "Ekonomi", null, null, 1, true, null);
        var result = _updateValidator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateCategoryDto.Id));
    }
}
