using FluentAssertions;
using KursuTV.Business.Helpers;
using Xunit;

namespace KursuTV.UnitTests.Helpers;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ShouldReturnHashedString_DifferentFromOriginal()
    {
        // Arrange
        var password = "SuperSecretPassword123!";

        // Act
        var hash = PasswordHasher.Hash(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
        hash.Should().StartWith("$2"); // BCrypt prefix
    }

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "AdminPassword456*";
        var hash = PasswordHasher.Hash(password);

        // Act
        var isValid = PasswordHasher.Verify(password, hash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "CorrectPassword1!";
        var wrongPassword = "WrongPassword2!";
        var hash = PasswordHasher.Hash(password);

        // Act
        var isValid = PasswordHasher.Verify(wrongPassword, hash);

        // Assert
        isValid.Should().BeFalse();
    }
}
