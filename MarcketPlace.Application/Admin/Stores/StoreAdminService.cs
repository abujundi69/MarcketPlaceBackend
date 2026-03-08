using MarcketPlace.Application.Admin.Stores.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Stores
{
    public class StoreAdminService : IStoreAdminService
    {
        private readonly AppDbContext _context;

        public StoreAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StoreAdminDetailsDto> CreateAsync(
            CreateStoreByAdminDto dto,
            CancellationToken cancellationToken = default)
        {
            var nameAr = dto.NameAr.Trim();
            var nameEn = dto.NameEn.Trim();
            var phoneNumber = dto.PhoneNumber.Trim();
            var addressText = dto.AddressText.Trim();

            ValidateStoreData(nameAr, nameEn, phoneNumber, addressText, dto.Latitude, dto.Longitude);

            var store = new Store
            {
                NameAr = nameAr,
                NameEn = nameEn,
                DescriptionAr = dto.DescriptionAr?.Trim(),
                DescriptionEn = dto.DescriptionEn?.Trim(),
                PhoneNumber = phoneNumber,
                AddressText = addressText,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                VendorId = null
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync(cancellationToken);

            var createdStore = await GetByIdAsync(store.Id, cancellationToken);
            return createdStore!;
        }

        public async Task<IReadOnlyList<StoreAdminListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var stores = await _context.Stores
                .AsNoTracking()
                .Include(x => x.Vendor)
                    .ThenInclude(x => x.User)
                .Include(x => x.StoreRatings)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var result = stores.Select(store =>
            {
                var ratingsCount = store.StoreRatings.Count;
                var averageRating = ratingsCount == 0
                    ? 0
                    : Math.Round(store.StoreRatings.Average(x => (decimal)x.Score), 2);

                return new StoreAdminListItemDto
                {
                    Id = store.Id,
                    NameAr = store.NameAr,
                    NameEn = store.NameEn,
                    PhoneNumber = store.PhoneNumber,
                    AddressText = store.AddressText,
                    Latitude = store.Latitude,
                    Longitude = store.Longitude,
                    IsActive = store.IsActive,
                    VendorId = store.VendorId,
                    VendorName = store.Vendor?.User?.FullName,
                    VendorPhoneNumber = store.Vendor?.User?.PhoneNumber,
                    VendorIsApproved = store.Vendor?.IsApproved,
                    AverageRating = averageRating,
                    RatingsCount = ratingsCount,
                    CreatedAt = store.CreatedAt,
                    CreatedAtText = store.CreatedAt.ToString("yyyy-MM-dd hh:mm tt")
                };
            }).ToList();

            return result;
        }

        public async Task<StoreAdminDetailsDto?> GetByIdAsync(
            int storeId,
            CancellationToken cancellationToken = default)
        {
            var store = await _context.Stores
                .AsNoTracking()
                .Include(x => x.Vendor)
                    .ThenInclude(x => x.User)
                .Include(x => x.StoreRatings)
                .FirstOrDefaultAsync(x => x.Id == storeId, cancellationToken);

            if (store is null)
                return null;

            var ratingsCount = store.StoreRatings.Count;
            var averageRating = ratingsCount == 0
                ? 0
                : Math.Round(store.StoreRatings.Average(x => (decimal)x.Score), 2);

            return new StoreAdminDetailsDto
            {
                Id = store.Id,
                NameAr = store.NameAr,
                NameEn = store.NameEn,
                DescriptionAr = store.DescriptionAr,
                DescriptionEn = store.DescriptionEn,
                PhoneNumber = store.PhoneNumber,
                AddressText = store.AddressText,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                IsActive = store.IsActive,
                VendorId = store.VendorId,
                VendorName = store.Vendor?.User?.FullName,
                VendorPhoneNumber = store.Vendor?.User?.PhoneNumber,
                VendorIsApproved = store.Vendor?.IsApproved,
                AverageRating = averageRating,
                RatingsCount = ratingsCount,
                CreatedAt = store.CreatedAt,
                CreatedAtText = store.CreatedAt.ToString("yyyy-MM-dd hh:mm tt"),
                UpdatedAt = store.UpdatedAt,
                UpdatedAtText = store.UpdatedAt.HasValue
                    ? store.UpdatedAt.Value.ToString("yyyy-MM-dd hh:mm tt")
                    : null
            };
        }

        public async Task<StoreAdminDetailsDto> UpdateAsync(
            int storeId,
            UpdateStoreByAdminDto dto,
            CancellationToken cancellationToken = default)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(x => x.Id == storeId, cancellationToken);

            if (store is null)
                throw new KeyNotFoundException("المتجر غير موجود.");

            var nameAr = dto.NameAr.Trim();
            var nameEn = dto.NameEn.Trim();
            var phoneNumber = dto.PhoneNumber.Trim();
            var addressText = dto.AddressText.Trim();

            ValidateStoreData(nameAr, nameEn, phoneNumber, addressText, dto.Latitude, dto.Longitude);

            if (dto.VendorId.HasValue)
            {
                var vendorExists = await _context.Vendors
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == dto.VendorId.Value, cancellationToken);

                if (!vendorExists)
                    throw new KeyNotFoundException("التاجر غير موجود.");
            }

            store.NameAr = nameAr;
            store.NameEn = nameEn;
            store.DescriptionAr = dto.DescriptionAr?.Trim();
            store.DescriptionEn = dto.DescriptionEn?.Trim();
            store.PhoneNumber = phoneNumber;
            store.AddressText = addressText;
            store.Latitude = dto.Latitude;
            store.Longitude = dto.Longitude;
            store.IsActive = dto.IsActive;
            store.VendorId = dto.VendorId;
            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var updatedStore = await GetByIdAsync(store.Id, cancellationToken);
            return updatedStore!;
        }

        private static void ValidateStoreData(
            string nameAr,
            string nameEn,
            string phoneNumber,
            string addressText,
            decimal latitude,
            decimal longitude)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new InvalidOperationException("الاسم العربي للمتجر مطلوب.");

            if (string.IsNullOrWhiteSpace(nameEn))
                throw new InvalidOperationException("الاسم الإنجليزي للمتجر مطلوب.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new InvalidOperationException("رقم هاتف المتجر مطلوب.");

            if (string.IsNullOrWhiteSpace(addressText))
                throw new InvalidOperationException("عنوان المتجر مطلوب.");

            if (latitude < -90 || latitude > 90)
                throw new InvalidOperationException("Latitude غير صالح.");

            if (longitude < -180 || longitude > 180)
                throw new InvalidOperationException("Longitude غير صالح.");
        }
    }
}