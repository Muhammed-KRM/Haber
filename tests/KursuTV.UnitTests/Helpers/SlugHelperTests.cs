using FluentAssertions;
using KursuTV.Business.Helpers;
using Xunit;

namespace KursuTV.UnitTests.Helpers;

public class SlugHelperTests
{
    [Theory]
    [InlineData("Şampiyonlar Ligi'nde Büyük Zafer!", "sampiyonlar-liginde-buyuk-zafer")]
    [InlineData("Türkçe Karakterler: Ğ, Ü, Ş, İ, Ö, Ç", "turkce-karakterler-g-u-s-i-o-c")]
    [InlineData("İstanbul ve Ağrı Arasında Köprü", "istanbul-ve-agri-arasinda-kopru")]
    [InlineData("Özel Haber: 2026 Seçimleri & Ekonomi Analizi", "ozel-haber-2026-secimleri-ekonomi-analizi")]
    [InlineData("   Birden    Fazla     Boşluk   ", "birden-fazla-bosluk")]
    [InlineData("---Tire---İle---Başlayan---", "tire-ile-baslayan")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData(null, "")]
    public void GenerateSlug_ShouldProduceCleanSeoFriendlySlug(string? input, string expected)
    {
        // Act
        var result = SlugHelper.GenerateSlug(input!);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateSlug_ShouldNotContainUppercaseLettersOrInvalidChars()
    {
        // Arrange
        var input = "Son Dakika: Deprem Bölgesinde #AFAD Çalışmaları Sürüyor! 100% Güvenli.";

        // Act
        var result = SlugHelper.GenerateSlug(input);

        // Assert
        result.Should().NotContainAny(" ", "#", "!", "%", ":", ".", "Ğ", "Ü", "Ş", "İ", "Ö", "Ç");
        result.Should().Be(result.ToLowerInvariant());
    }
}
