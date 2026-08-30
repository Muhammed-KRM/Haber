using KursuTV.Business.DTOs;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Repositories;

namespace KursuTV.Business.Services;

public class TagManager : ITagService
{
    private readonly ITagRepository _tagRepository;

    public TagManager(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<List<TagDto>> SearchTagsAsync(string query, int count = 10, CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.SearchTagsAsync(query, count, cancellationToken);
        return tags.Select(t => new TagDto(t.Id, t.Name, t.Slug)).ToList();
    }

    public async Task<TagDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetBySlugAsync(slug, cancellationToken);
        return tag == null ? null : new TagDto(tag.Id, tag.Name, tag.Slug);
    }
}
