using FluentAssertions;
using KursuTV.Business.DTOs;
using KursuTV.Business.Validators;
using Xunit;

namespace KursuTV.UnitTests.Validators;

public class UserRegisterValidatorTests
{
    private readonly UserRegisterValidator _validator = new();

    [Fact]
    public void UserRegisterValidator_WithValidData_ShouldPass()
    {
        var dto = new UserRegisterDto
        {
            Email = "yazar@kursutv.com",
            Password = "StrongPassword123",
            FullName = "Mehmet Kaya"
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@kursutv.com")]
    public void UserRegisterValidator_WithInvalidEmail_ShouldFail(string invalidEmail)
    {
        var dto = new UserRegisterDto
        {
            Email = invalidEmail,
            Password = "StrongPassword123",
            FullName = "Mehmet Kaya"
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UserRegisterDto.Email));
    }

    [Theory]
    [InlineData("short1A")] // < 8 chars
    [InlineData("nouppercase123")] // no uppercase
    [InlineData("NOLOWERCASE123")] // no lowercase
    [InlineData("NoDigitPassword")] // no digit
    public void UserRegisterValidator_WithWeakPassword_ShouldFail(string weakPassword)
    {
        var dto = new UserRegisterDto
        {
            Email = "yazar@kursutv.com",
            Password = weakPassword,
            FullName = "Mehmet Kaya"
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UserRegisterDto.Password));
    }

    [Theory]
    [InlineData("A")] // < 3 chars
    [InlineData("")]
    [InlineData(null)]
    public void UserRegisterValidator_WithInvalidFullName_ShouldFail(string? invalidName)
    {
        var dto = new UserRegisterDto
        {
            Email = "yazar@kursutv.com",
            Password = "StrongPassword123",
            FullName = invalidName!
        };

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UserRegisterDto.FullName));
    }
}
