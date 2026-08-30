using KursuTV.Business.DTOs;
using KursuTV.Business.Exceptions;
using KursuTV.Business.Helpers;
using KursuTV.Business.Interfaces;
using KursuTV.Data.Entities;
using KursuTV.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace KursuTV.Business.Services;

public class CategoryManager : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CategoryManager> _logger;

    private const string ActiveCategoriesTreeCacheKey = "categories:tree";
    private const string AllCategoriesFlatCacheKey = "categories:flat";

    public CategoryManager(
        ICategoryRepository categoryRepository,
        ICacheService cacheService,
        ILogger<CategoryManager> logger)
    {
        _categoryRepository = categoryRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetAllActiveTreeAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<List<CategoryDto>>(ActiveCategoriesTreeCacheKey);
        if (cached != null)
        {
            return cached;
        }

        var categories = await _categoryRepository.GetActiveCategoriesWithChildrenAsync(cancellationToken);
        var result = categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Slug,
            c.Description,
            c.SortOrder,
            c.IsActive,
            c.ParentId,
            c.Children.Select(ch => new CategoryDto(
                ch.Id,
                ch.Name,
                ch.Slug,
                ch.Description,
                ch.SortOrder,
                ch.IsActive,
                ch.ParentId,
                null
            )).ToList()
        )).ToList();

        await _cacheService.SetAsync(ActiveCategoriesTreeCacheKey, result, TimeSpan.FromHours(1));
        return result;
    }

    public async Task<List<CategoryDto>> GetAllFlatAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<List<CategoryDto>>(AllCategoriesFlatCacheKey);
        if (cached != null)
        {
            return cached;
        }

        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        var result = categories
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.SortOrder,
                c.IsActive,
                c.ParentId,
                null
            )).ToList();

        await _cacheService.SetAsync(AllCategoriesFlatCacheKey, result, TimeSpan.FromMinutes(30));
        return result;
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null) return null;

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.SortOrder,
            category.IsActive,
            category.ParentId,
            null
        );
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetBySlugAsync(slug, cancellationToken);
        if (category == null) return null;

        return new CategoryDto(
            category.Id,
            category.Name,
            category.Slug,
            category.Description,
            category.SortOrder,
            category.IsActive,
            category.ParentId,
            category.Children?.Select(ch => new CategoryDto(
                ch.Id,
                ch.Name,
                ch.Slug,
                ch.Description,
                ch.SortOrder,
                ch.IsActive,
                ch.ParentId,
                null
            )).ToList()
        );
    }

    public async Task<int> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Name)
            : SlugHelper.GenerateSlug(dto.Slug);

        if (await _categoryRepository.SlugExistsAsync(slug, null, cancellationToken))
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks % 1000}";
        }

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Slug = slug,
            Description = dto.Description?.Trim(),
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            ParentId = dto.ParentId,
            CreatedAt = DateTime.UtcNow
        };

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        await InvalidateCacheAsync();
        _logger.LogInformation("Kategori eklendi: {CategoryId} - {Name}", category.Id, category.Name);

        return category.Id;
    }

    public async Task UpdateCategoryAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException("Kategori bulunamadı.");
        }

        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Name)
            : SlugHelper.GenerateSlug(dto.Slug);

        if (await _categoryRepository.SlugExistsAsync(slug, dto.Id, cancellationToken))
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks % 1000}";
        }

        category.Name = dto.Name.Trim();
        category.Slug = slug;
        category.Description = dto.Description?.Trim();
        category.SortOrder = dto.SortOrder;
        category.IsActive = dto.IsActive;
        category.ParentId = dto.ParentId;
        category.UpdatedAt = DateTime.UtcNow;

        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();

        await InvalidateCacheAsync();
        _logger.LogInformation("Kategori güncellendi: {CategoryId} - {Name}", category.Id, category.Name);
    }

    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException("Silinecek kategori bulunamadı.");
        }

        _categoryRepository.Delete(category);
        await _categoryRepository.SaveChangesAsync();

        await InvalidateCacheAsync();
        _logger.LogInformation("Kategori silindi: {CategoryId}", id);
    }

    private async Task InvalidateCacheAsync()
    {
        await _cacheService.RemoveAsync(ActiveCategoriesTreeCacheKey);
        await _cacheService.RemoveAsync(AllCategoriesFlatCacheKey);
    }
}
