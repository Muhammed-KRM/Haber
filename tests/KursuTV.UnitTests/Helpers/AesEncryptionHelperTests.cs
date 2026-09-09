using FluentAssertions;
using KursuTV.Business.Helpers;
using Xunit;

namespace KursuTV.UnitTests.Helpers;

public class AesEncryptionHelperTests
{
    private const string SecretKey = "KursuTVSecretKeyMustBe32CharsLong!";

    [Fact]
    public void EncryptAndDecrypt_ShouldReturnOriginalText()
    {
        // Arrange
        var originalText = "12345678901 - TR330006100511123456789012";

        // Act
        var encrypted = AesEncryptionHelper.Encrypt(originalText, SecretKey);
        var decrypted = AesEncryptionHelper.Decrypt(encrypted, SecretKey);

        // Assert
        encrypted.Should().NotBeNullOrWhiteSpace();
        encrypted.Should().NotBe(originalText);
        decrypted.Should().Be(originalText);
    }

    [Fact]
    public void Decrypt_WithWrongKey_ShouldThrowCryptographicException()
    {
        // Arrange
        var originalText = "SensitiveInformationData";
        var encrypted = AesEncryptionHelper.Encrypt(originalText, SecretKey);
        var wrongKey = "DifferentWrongKey32Characters123!";

        // Act & Assert
        var action = () => AesEncryptionHelper.Decrypt(encrypted, wrongKey);
        action.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }
}
