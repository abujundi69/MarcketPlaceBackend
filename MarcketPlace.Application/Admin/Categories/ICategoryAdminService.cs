using MarcketPlace.Application.Admin.Categories.Dtos;

namespace MarcketPlace.Application.Admin.Categories
{
    public interface ICategoryAdminService
    {
        Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}