namespace KursuTV.Business.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    int SortOrder,
    bool IsActive,
    int? ParentId,
    List<CategoryDto>? Children
);

public record CreateCategoryDto(
    string Name,
    string? Slug,
    string? Description,
    int SortOrder,
    bool IsActive,
    int? ParentId
);

public record UpdateCategoryDto(
    int Id,
    string Name,
    string? Slug,
    string? Description,
    int SortOrder,
    bool IsActive,
    int? ParentId
);
