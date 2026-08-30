using System.Text;

namespace KursuTV.Business.Infrastructure.Moderation;

public static class TurkishTextNormalizer
{
    // Kiril ve Yunan harflerini Latin karÅŸÄ±lÄ±klarÄ±na Ã§evirir (homoglyph bypass engeli)
    private static readonly Dictionary<char, char> HomoglyphMap = new()
    {
        ['\u0430'] = 'a', // Kiril Ğ° â†’ Latin a
        ['\u0435'] = 'e', // Kiril Ğµ â†’ Latin e
        ['\u043E'] = 'o', // Kiril Ğ¾ â†’ Latin o
        ['\u0440'] = 'p', // Kiril Ñ€ â†’ Latin p
        ['\u0441'] = 'c', // Kiril Ñ â†’ Latin c
        ['\u0445'] = 'x', // Kiril Ñ… â†’ Latin x
        ['\u03BF'] = 'o', // Yunan Î¿ â†’ Latin o
        ['\u03B1'] = 'a', // Yunan Î± â†’ Latin a
    };

    // TÃ¼rkÃ§e rakam kelimeleri â†’ rakam
    private static readonly Dictionary<string, string> NumberWords = new()
    {
        ["sÄ±fÄ±r"] = "0",
        ["bir"]   = "1",
        ["iki"]   = "2",
        ["Ã¼Ã§"]    = "3",
        ["dÃ¶rt"]  = "4",
        ["beÅŸ"]   = "5",
        ["altÄ±"]  = "6",
        ["yedi"]  = "7",
        ["sekiz"] = "8",
        ["dokuz"] = "9",
    };

    public static string Normalize(string text)
    {
        // 1. Homoglyph temizle
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
            sb.Append(HomoglyphMap.TryGetValue(c, out var clean) ? clean : c);
        var result = sb.ToString().ToLowerInvariant();

        // 2. TÃ¼rkÃ§e rakam kelimelerini sayÄ±ya Ã§evir
        foreach (var (word, digit) in NumberWords)
            result = result.Replace(word, digit);

        return result;
    }
}
