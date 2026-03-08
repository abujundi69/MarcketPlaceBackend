using MarcketPlace.Application.Admin.DeliveryZones.Dtos;

namespace MarcketPlace.Application.Admin.DeliveryZones
{
    public interface IDeliveryZoneAdminService
    {
        Task<DeliveryZoneDto> CreateAsync(CreateDeliveryZoneDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<DeliveryZoneDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DeliveryZoneDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<DeliveryZoneDto> UpdateAsync(int id, UpdateDeliveryZoneDto dto, CancellationToken cancellationToken = default);
    }
}