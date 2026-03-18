using MarcketPlace.Application.Admin.Categories.Dtos;

namespace MarcketPlace.Application.Admin.Categories
{
    public interface IAdminCategoryService
    {
        Task<AdminCategoryDto> CreateAsync(
            CreateCategoryDto dto,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AdminCategoryListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<AdminCategoryDto> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<AdminCategoryDto> UpdateAsync(
            int id,
            UpdateCategoryDto dto,
            CancellationToken cancellationToken = default);
    }
}