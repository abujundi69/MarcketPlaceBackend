using MarcketPlace.Application.Admin.ProductRequests.Dtos;

namespace MarcketPlace.Application.Admin.ProductRequests
{
    public interface IAdminProductRequestService
    {
        Task<IReadOnlyList<AdminProductRequestDto>> GetPendingAsync(CancellationToken cancellationToken = default);

        Task<AdminProductRequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<AdminProductRequestDto> ApproveAsync(
            int productRequestId,
            int adminUserId,
            CancellationToken cancellationToken = default);

        Task<AdminProductRequestDto> RejectAsync(
            int productRequestId,
            int adminUserId,
            RejectProductRequestDto dto,
            CancellationToken cancellationToken = default);
    }
}
