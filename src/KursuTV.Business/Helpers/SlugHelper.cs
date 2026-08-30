using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KursuTV.Business.Helpers;

public static class SlugHelper
{
    public static string GenerateSlug(string text)
    {
        // TÃ¼rkÃ§e karakterleri dÃ¶nÃ¼ÅŸtÃ¼r
        var slug = text.ToLowerInvariant();
        slug = slug.Replace("Ä±", "i").Replace("ÄŸ", "g").Replace("Ã¼", "u")
                   .Replace("ÅŸ", "s").Replace("Ã¶", "o").Replace("Ã§", "c");
        
        // AksanlarÄ± kaldÄ±r
        slug = RemoveDiacritics(slug);
        
        // AlfanÃ¼merik olmayan karakterleri tire ile deÄŸiÅŸtir
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');
        
        return slug;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
