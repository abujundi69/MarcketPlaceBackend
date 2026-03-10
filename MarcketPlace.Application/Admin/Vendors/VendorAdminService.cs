using MarcketPlace.Application.Admin.Vendors.Dtos;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StoreEntity = MarcketPlace.Domain.Entities.Store;
using UserEntity = MarcketPlace.Domain.Entities.User;
using VendorEntity = MarcketPlace.Domain.Entities.Vendor;

namespace MarcketPlace.Application.Admin.Vendors
{
    public class VendorAdminService : IVendorAdminService
    {
        private readonly AppDbContext _context;

        public VendorAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<VendorAdminDetailsDto> CreateAsync(
            CreateVendorByAdminDto dto,
            CancellationToken cancellationToken = default)
        {
            var fullName = dto.FullName?.Trim();
            var phoneNumber = dto.PhoneNumber?.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                throw new InvalidOperationException("اسم التاجر مطلوب.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new InvalidOperationException("رقم هاتف التاجر مطلوب.");

            var phoneExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

            if (phoneExists)
                throw new InvalidOperationException("رقم الهاتف مستخدم مسبقًا.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var user = new UserEntity
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Role = UserRole.Vendor,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var rawPassword = $"{phoneNumber}@@";
            var passwordHasher = new PasswordHasher<UserEntity>();
            user.PasswordHash = passwordHasher.HashPassword(user, rawPassword);

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var vendor = new VendorEntity
            {
                UserId = user.Id,
                IsApproved = dto.IsApproved,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var createdVendor = await GetByIdAsync(vendor.Id, cancellationToken);
            return createdVendor!;
        }

        public async Task<IReadOnlyList<VendorAdminListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var vendors = await _context.Vendors
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Stores)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            if (vendors.Count == 0)
                return new List<VendorAdminListItemDto>();

            var allStoreIds = vendors
                .SelectMany(x => x.Stores)
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            var ratings = allStoreIds.Count == 0
                ? new List<StoreRatingAggregate>()
                : await _context.StoreRatings
                    .AsNoTracking()
                    .Where(x => allStoreIds.Contains(x.StoreId))
                    .GroupBy(x => x.StoreId)
                    .Select(g => new StoreRatingAggregate
                    {
                        StoreId = g.Key,
                        Count = g.Count(),
                        Sum = g.Sum(x => x.Score)
                    })
                    .ToListAsync(cancellationToken);

            var result = vendors.Select(v =>
            {
                var firstStore = v.Stores.OrderBy(x => x.Id).FirstOrDefault();
                var vendorStoreIds = v.Stores.Select(x => x.Id).ToHashSet();

                var vendorRatings = ratings
                    .Where(x => vendorStoreIds.Contains(x.StoreId))
                    .ToList();

                var ratingsCount = vendorRatings.Sum(x => x.Count);
                var ratingsSum = vendorRatings.Sum(x => x.Sum);

                return new VendorAdminListItemDto
                {
                    VendorId = v.Id,
                    UserId = v.UserId,
                    FullName = v.User.FullName,
                    PhoneNumber = v.User.PhoneNumber,
                    IsApproved = v.IsApproved,
                    IsActive = v.User.IsActive,
                    CreatedAt = v.CreatedAt,
                    CreatedAtText = v.CreatedAt.ToString("yyyy-MM-dd hh:mm tt"),
                    StoresCount = v.Stores.Count,
                    StoreId = firstStore?.Id,
                    StoreNameAr = firstStore?.NameAr,
                    StoreNameEn = firstStore?.NameEn,
                    StoreRatingsCount = ratingsCount,
                    StoreAverageRating = ratingsCount == 0
                        ? 0
                        : Math.Round((decimal)ratingsSum / ratingsCount, 2)
                };
            }).ToList();

            return result;
        }

        public async Task<VendorAdminDetailsDto?> GetByIdAsync(
            int vendorId,
            CancellationToken cancellationToken = default)
        {
            var vendor = await _context.Vendors
                .AsNoTracking()
                .Include(x => x.User)
                .Include(x => x.Stores)
                .FirstOrDefaultAsync(x => x.Id == vendorId, cancellationToken);

            if (vendor is null)
                return null;

            var vendorStoreIds = vendor.Stores
                .Select(x => x.Id)
                .ToList();

            var ratings = vendorStoreIds.Count == 0
                ? new List<StoreRatingAggregate>()
                : await _context.StoreRatings
                    .AsNoTracking()
                    .Where(x => vendorStoreIds.Contains(x.StoreId))
                    .GroupBy(x => x.StoreId)
                    .Select(g => new StoreRatingAggregate
                    {
                        StoreId = g.Key,
                        Count = g.Count(),
                        Sum = g.Sum(x => x.Score)
                    })
                    .ToListAsync(cancellationToken);

            var firstStore = vendor.Stores.OrderBy(x => x.Id).FirstOrDefault();
            var ratingsCount = ratings.Sum(x => x.Count);
            var ratingsSum = ratings.Sum(x => x.Sum);

            return new VendorAdminDetailsDto
            {
                VendorId = vendor.Id,
                UserId = vendor.UserId,
                FullName = vendor.User.FullName,
                PhoneNumber = vendor.User.PhoneNumber,
                IsApproved = vendor.IsApproved,
                IsActive = vendor.User.IsActive,
                CreatedAt = vendor.CreatedAt,
                CreatedAtText = vendor.CreatedAt.ToString("yyyy-MM-dd hh:mm tt"),
                StoresCount = vendor.Stores.Count,
                StoreId = firstStore?.Id,
                StoreNameAr = firstStore?.NameAr,
                StoreNameEn = firstStore?.NameEn,
                StoreRatingsCount = ratingsCount,
                StoreAverageRating = ratingsCount == 0
                    ? 0
                    : Math.Round((decimal)ratingsSum / ratingsCount, 2)
            };
        }

        public async Task<VendorAdminDetailsDto> UpdateAsync(
            int vendorId,
            UpdateVendorByAdminDto dto,
            CancellationToken cancellationToken = default)
        {
            var vendor = await _context.Vendors
                .Include(x => x.User)
                .Include(x => x.Stores)
                .FirstOrDefaultAsync(x => x.Id == vendorId, cancellationToken);

            if (vendor is null)
                throw new KeyNotFoundException("التاجر غير موجود.");

            var fullName = dto.FullName?.Trim();
            var phoneNumber = dto.PhoneNumber?.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                throw new InvalidOperationException("اسم التاجر مطلوب.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new InvalidOperationException("رقم هاتف التاجر مطلوب.");

            var phoneExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(x => x.PhoneNumber == phoneNumber && x.Id != vendor.UserId, cancellationToken);

            if (phoneExists)
                throw new InvalidOperationException("رقم الهاتف مستخدم مسبقًا.");

            StoreEntity? targetStore = null;

            if (dto.StoreId.HasValue)
            {
                targetStore = await _context.Stores
                    .FirstOrDefaultAsync(x => x.Id == dto.StoreId.Value, cancellationToken);

                if (targetStore is null)
                    throw new KeyNotFoundException("المتجر غير موجود.");

                if (targetStore.VendorId.HasValue && targetStore.VendorId.Value != vendor.Id)
                    throw new InvalidOperationException("هذا المتجر مرتبط بالفعل بتاجر آخر.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            vendor.User.FullName = fullName;
            vendor.User.PhoneNumber = phoneNumber;
            vendor.User.IsActive = dto.IsActive;
            vendor.User.UpdatedAt = DateTime.UtcNow;

            vendor.IsApproved = dto.IsApproved;

            var currentStores = await _context.Stores
                .Where(x => x.VendorId == vendor.Id)
                .ToListAsync(cancellationToken);

            foreach (var store in currentStores)
            {
                if (!dto.StoreId.HasValue || store.Id != dto.StoreId.Value)
                    store.VendorId = null;
            }

            if (targetStore is not null)
                targetStore.VendorId = vendor.Id;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var updatedVendor = await GetByIdAsync(vendor.Id, cancellationToken);
            return updatedVendor!;
        }

        private sealed class StoreRatingAggregate
        {
            public int StoreId { get; set; }
            public int Count { get; set; }
            public int Sum { get; set; }
        }
    }
}