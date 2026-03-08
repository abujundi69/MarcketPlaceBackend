using MarcketPlace.Application.Admin.ProductRequests.Dtos;

namespace MarcketPlace.Application.Admin.ProductRequests
{
    public interface IAdminProductRequestService
    {
        Task<IReadOnlyList<AdminProductRequestListItemDto>> GetPendingAsync(CancellationToken cancellationToken = default);
        Task<AdminProductRequestDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<AdminProductRequestDetailsDto?> ApproveAsync(int requestId, int? adminUserId, string? note, CancellationToken cancellationToken = default);
        Task<AdminProductRequestDetailsDto?> RejectAsync(int requestId, int? adminUserId, string? note, CancellationToken cancellationToken = default);
    }
}