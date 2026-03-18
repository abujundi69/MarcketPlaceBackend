using MarcketPlace.Application.Customer.Locations.Dtos;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Customer.Locations
{
    public class CustomerLocationService : ICustomerLocationService
    {
        private readonly AppDbContext _context;

        public CustomerLocationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerSavedLocationDto> GetMyLocationAsync(
            int customerUserId,
            CancellationToken cancellationToken = default)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .Include(x => x.DefaultDeliveryZone)
                .FirstOrDefaultAsync(x => x.UserId == customerUserId, cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException("الزبون غير موجود.");

            return new CustomerSavedLocationDto
            {
                DeliveryZoneId = customer.DefaultDeliveryZoneId,
                DeliveryZoneNameAr = customer.DefaultDeliveryZone?.NameAr,
                DeliveryZoneNameEn = customer.DefaultDeliveryZone?.NameEn,
                AddressText = customer.DefaultAddressText,
                Latitude = customer.DefaultLatitude,
                Longitude = customer.DefaultLongitude,
                LocationUpdatedAt = customer.LocationUpdatedAt
            };
        }

        public async Task<CustomerSavedLocationDto> UpdateMyLocationAsync(
            int customerUserId,
            UpdateCustomerSavedLocationDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            if (dto.DeliveryZoneId <= 0)
                throw new InvalidOperationException("منطقة التوصيل غير صالحة.");

            if (string.IsNullOrWhiteSpace(dto.AddressText))
                throw new InvalidOperationException("عنوان التوصيل مطلوب.");

            if (dto.Latitude < -90 || dto.Latitude > 90)
                throw new InvalidOperationException("خط العرض غير صالح.");

            if (dto.Longitude < -180 || dto.Longitude > 180)
                throw new InvalidOperationException("خط الطول غير صالح.");

            var customer = await _context.Customers
                .Include(x => x.DefaultDeliveryZone)
                .FirstOrDefaultAsync(x => x.UserId == customerUserId, cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException("الزبون غير موجود.");

            var zone = await _context.DeliveryZones
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.DeliveryZoneId, cancellationToken);

            if (zone is null)
                throw new KeyNotFoundException("منطقة التوصيل غير موجودة.");

            customer.DefaultDeliveryZoneId = dto.DeliveryZoneId;
            customer.DefaultAddressText = dto.AddressText.Trim();
            customer.DefaultLatitude = dto.Latitude;
            customer.DefaultLongitude = dto.Longitude;
            customer.LocationUpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new CustomerSavedLocationDto
            {
                DeliveryZoneId = zone.Id,
                DeliveryZoneNameAr = zone.NameAr,
                DeliveryZoneNameEn = zone.NameEn,
                AddressText = customer.DefaultAddressText,
                Latitude = customer.DefaultLatitude,
                Longitude = customer.DefaultLongitude,
                LocationUpdatedAt = customer.LocationUpdatedAt
            };
        }
    }
}