using KursuTV.Data.Context;
using KursuTV.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KursuTV.Data.Repositories;

public class TagRepository : GenericRepository<Tag>, ITagRepository
{
    public TagRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<List<Tag>> GetOrCreateTagsAsync(List<string> tagNames, CancellationToken cancellationToken = default)
    {
        var result = new List<Tag>();
        var cleanNames = tagNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (cleanNames.Count == 0) return result;

        var existingTags = await _dbSet
            .Where(t => cleanNames.Contains(t.Name))
            .ToListAsync(cancellationToken);

        result.AddRange(existingTags);

        var existingNames = existingTags.Select(t => t.Name.ToLower()).ToHashSet();

        foreach (var name in cleanNames)
        {
            if (!existingNames.Contains(name.ToLower()))
            {
                // Basit slug üretimi (daha gelişmişi Business Helper ile yapılacak)
                var slug = name.ToLower()
                    .Replace(" ", "-")
                    .Replace("ı", "i")
                    .Replace("ğ", "g")
                    .Replace("ü", "u")
                    .Replace("ş", "s")
                    .Replace("ö", "o")
                    .Replace("ç", "c");

                var newTag = new Tag
                {
                    Name = name,
                    Slug = slug,
                    CreatedAt = DateTime.UtcNow
                };

                await _dbSet.AddAsync(newTag, cancellationToken);
                result.Add(newTag);
            }
        }

        return result;
    }

    public async Task<List<Tag>> SearchTagsAsync(string query, int count = 10, CancellationToken cancellationToken = default)
    {
        var q = query.Trim().ToLower();
        return await _dbSet.AsNoTracking()
            .Where(t => t.Name.ToLower().Contains(q))
            .OrderBy(t => t.Name)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
