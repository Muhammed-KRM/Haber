using FluentAssertions;
using KursuTV.Business.DTOs;
using KursuTV.Business.Validators;
using Xunit;

namespace KursuTV.UnitTests.Validators;

public class CommentValidatorTests
{
    private readonly CreateCommentValidator _validator = new();

    [Fact]
    public void CreateCommentValidator_WithValidData_ShouldPass()
    {
        var dto = new CreateCommentDto(
            NewsId: Guid.NewGuid(),
            Content: "Çok bilgilendirici ve güzel bir haber olmuş, teşekkürler.",
            GuestName: "Ali Veli",
            ParentCommentId: null
        );

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateCommentValidator_WhenNewsIdIsEmpty_ShouldFail()
    {
        var dto = new CreateCommentDto(
            NewsId: Guid.Empty,
            Content: "İyi haber",
            GuestName: null,
            ParentCommentId: null
        );

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCommentDto.NewsId));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreateCommentValidator_WhenContentIsEmpty_ShouldFail(string? content)
    {
        var dto = new CreateCommentDto(
            NewsId: Guid.NewGuid(),
            Content: content!,
            GuestName: null,
            ParentCommentId: null
        );

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCommentDto.Content));
    }

    [Fact]
    public void CreateCommentValidator_WhenContentExceeds2000Chars_ShouldFail()
    {
        var dto = new CreateCommentDto(
            NewsId: Guid.NewGuid(),
            Content: new string('Y', 2001),
            GuestName: null,
            ParentCommentId: null
        );

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateCommentDto.Content));
    }
}
