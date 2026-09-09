using FluentAssertions;
using KursuTV.Business.Infrastructure.Moderation;
using Xunit;

namespace KursuTV.UnitTests.Infrastructure;

public class TurkishTextNormalizerTests
{
    [Fact]
    public void Normalize_ShouldConvertHomoglyphsToLatinCharacters()
    {
        // Cyrillic letters: \u0430 (a), \u0435 (e), \u043E (o), \u0440 (p), \u0441 (c), \u0445 (x)
        var maliciousInput = "\u0430\u0435\u043E\u0440\u0441\u0445"; // lookalikes for "aeopcx"

        // Act
        var normalized = TurkishTextNormalizer.Normalize(maliciousInput);

        // Assert
        normalized.Should().Be("aeopcx");
    }

    [Fact]
    public void Normalize_ShouldConvertTurkishNumberWordsToDigits()
    {
        // Arrange
        var text = "sıfır beş yüz otuz iki bir iki üç";

        // Act
        var normalized = TurkishTextNormalizer.Normalize(text);

        // Assert
        normalized.Should().Contain("0");
        normalized.Should().Contain("1");
        normalized.Should().Contain("2");
        normalized.Should().Contain("3");
        normalized.Should().NotContain("sıfır");
        normalized.Should().NotContain("bir");
        normalized.Should().NotContain("iki");
    }
}
