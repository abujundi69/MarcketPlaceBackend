using MarcketPlace.Application.Admin.Products.Dtos;

namespace MarcketPlace.Application.Admin.Products
{
    public interface IAdminProductService
    {
        Task<AdminProductDto> CreateAsync(
            CreateAdminProductDto dto,
            CancellationToken cancellationToken = default);

        Task<AdminProductDto> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<AdminProductDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminProductDto>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<AdminProductDto> UpdateAsync(int id, UpdateAdminProductDto dto, CancellationToken cancellationToken = default);
    }
}