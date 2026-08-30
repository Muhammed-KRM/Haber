using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KursuTV.Data.Context;
using KursuTV.Data.Enums;

namespace KursuTV.API.Controllers;

[ApiController]
public class SitemapController : ControllerBase
{
    private readonly AppDbContext _context;

    public SitemapController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Standart XML Sitemap - Google ve arama motorları için tüm yayınlanmış haber ve kategorileri listeler.
    /// </summary>
    [HttpGet("sitemap.xml")]
    [Produces("application/xml")]
    public async Task<IActionResult> GetSitemap()
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

        var urlset = new XElement(ns + "urlset");

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        // Anasayfa
        urlset.Add(new XElement(ns + "url",
            new XElement(ns + "loc", baseUrl),
            new XElement(ns + "changefreq", "hourly"),
            new XElement(ns + "priority", "1.0")
        ));

        // Aktif Kategoriler
        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .ToListAsync();

        foreach (var cat in categories)
        {
            urlset.Add(new XElement(ns + "url",
                new XElement(ns + "loc", $"{baseUrl}/kategori/{cat.Slug}"),
                new XElement(ns + "changefreq", "daily"),
                new XElement(ns + "priority", "0.8")
            ));
        }

        // Son 1000 yayınlanmış haber
        var newsList = await _context.News
            .AsNoTracking()
            .Where(n => n.Status == NewsStatus.Published)
            .OrderByDescending(n => n.PublishedAt)
            .Take(1000)
            .Select(n => new { n.Slug, n.PublishedAt, n.UpdatedAt })
            .ToListAsync();

        foreach (var news in newsList)
        {
            urlset.Add(new XElement(ns + "url",
                new XElement(ns + "loc", $"{baseUrl}/haber/{news.Slug}"),
                new XElement(ns + "lastmod", (news.UpdatedAt ?? news.PublishedAt ?? DateTime.UtcNow).ToString("yyyy-MM-ddTHH:mm:sszzz")),
                new XElement(ns + "changefreq", "daily"),
                new XElement(ns + "priority", "0.9")
            ));
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(doc.ToString(), "application/xml", Encoding.UTF8);
    }

    /// <summary>
    /// Google News XML Sitemap - Son 48 saatte yayınlanan haberleri Google Haberler formatında sunar.
    /// </summary>
    [HttpGet("news-sitemap.xml")]
    [Produces("application/xml")]
    public async Task<IActionResult> GetNewsSitemap()
    {
        XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XNamespace newsNs = "http://www.google.com/schemas/sitemap-news/0.9";

        var urlset = new XElement(ns + "urlset", new XAttribute(XNamespace.Xmlns + "news", newsNs));

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var last48Hours = DateTime.UtcNow.AddHours(-48);

        var recentNews = await _context.News
            .AsNoTracking()
            .Where(n => n.Status == NewsStatus.Published && n.PublishedAt >= last48Hours)
            .OrderByDescending(n => n.PublishedAt)
            .Select(n => new { n.Title, n.Slug, n.PublishedAt })
            .ToListAsync();

        foreach (var news in recentNews)
        {
            urlset.Add(new XElement(ns + "url",
                new XElement(ns + "loc", $"{baseUrl}/haber/{news.Slug}"),
                new XElement(newsNs + "news",
                    new XElement(newsNs + "publication",
                        new XElement(newsNs + "name", "Kürsü TV"),
                        new XElement(newsNs + "language", "tr")
                    ),
                    new XElement(newsNs + "publication_date", news.PublishedAt?.ToString("yyyy-MM-ddTHH:mm:sszzz")),
                    new XElement(newsNs + "title", news.Title)
                )
            ));
        }

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), urlset);
        return Content(doc.ToString(), "application/xml", Encoding.UTF8);
    }
}
