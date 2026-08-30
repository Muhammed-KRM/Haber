using KursuTV.Business.DTOs;

namespace KursuTV.Business.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllActiveTreeAsync(CancellationToken cancellationToken = default);
    Task<List<CategoryDto>> GetAllFlatAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<int> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
