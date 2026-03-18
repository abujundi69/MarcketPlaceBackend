using MarcketPlace.Application.Admin.Vendors.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Vendors
{
    public class VendorAdminService : IVendorAdminService
    {
        private readonly AppDbContext _context;

        public VendorAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<VendorAdminListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var vendors = await _context.Vendors
                .AsNoTracking()
                .Select(x => new VendorAdminListItemDto
                {
                    VendorId = x.Id,
                    UserId = x.UserId,
                    FullName = x.User.FullName,
                    PhoneNumber = x.User.PhoneNumber,
                    IsApproved = x.IsApproved,
                    IsActive = x.User.IsActive,
                    CreatedAt = x.CreatedAt,
                    StoresCount = 0,
                    StoreId = null,
                    StoreNameAr = null,
                    StoreNameEn = null,
                    StoreAverageRating = 0,
                    StoreRatingsCount = 0
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            foreach (var v in vendors)
            {
                v.CreatedAtText = v.CreatedAt.ToString("yyyy-MM-dd hh:mm tt");
            }

            return vendors;
        }

        public async Task<VendorAdminListItemDto?> GetByIdAsync(int vendorId, CancellationToken cancellationToken = default)
        {
            var vendor = await _context.Vendors
                .AsNoTracking()
                .Where(x => x.Id == vendorId)
                .Select(x => new VendorAdminListItemDto
                {
                    VendorId = x.Id,
                    UserId = x.UserId,
                    FullName = x.User.FullName,
                    PhoneNumber = x.User.PhoneNumber,
                    IsApproved = x.IsApproved,
                    IsActive = x.User.IsActive,
                    CreatedAt = x.CreatedAt,
                    StoresCount = 0,
                    StoreId = null,
                    StoreNameAr = null,
                    StoreNameEn = null,
                    StoreAverageRating = 0,
                    StoreRatingsCount = 0
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (vendor is null)
                return null;

            vendor.CreatedAtText = vendor.CreatedAt.ToString("yyyy-MM-dd hh:mm tt");
            return vendor;
        }

        public async Task<VendorAdminListItemDto> CreateAsync(CreateVendorDto dto, CancellationToken cancellationToken = default)
        {
            ValidateCreateOrUpdate(dto.FullName, dto.PhoneNumber);

            var phoneNumber = dto.PhoneNumber.Trim();
            var phoneExists = await _context.Users
                .AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

            if (phoneExists)
                throw new InvalidOperationException("رقم الهاتف مستخدم مسبقًا.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                PhoneNumber = phoneNumber,
                Role = UserRole.Vendor,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var rawPassword = $"{phoneNumber}@@";
            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, rawPassword);

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var vendor = new Domain.Entities.Vendor
            {
                UserId = user.Id,
                IsApproved = dto.IsApproved,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return (await GetByIdAsync(vendor.Id, cancellationToken))!;
        }

        public async Task<VendorAdminListItemDto> UpdateAsync(int vendorId, UpdateVendorDto dto, CancellationToken cancellationToken = default)
        {
            ValidateCreateOrUpdate(dto.FullName, dto.PhoneNumber);

            var vendor = await _context.Vendors
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == vendorId, cancellationToken)
                ?? throw new KeyNotFoundException("التاجر غير موجود.");

            var phoneNumber = dto.PhoneNumber.Trim();
            var phoneExists = await _context.Users
                .AnyAsync(x => x.PhoneNumber == phoneNumber && x.Id != vendor.UserId, cancellationToken);

            if (phoneExists)
                throw new InvalidOperationException("رقم الهاتف مستخدم من قبل مستخدم آخر.");

            vendor.User.FullName = dto.FullName.Trim();
            vendor.User.PhoneNumber = phoneNumber;
            vendor.User.IsActive = dto.IsActive;
            vendor.User.UpdatedAt = DateTime.UtcNow;
            vendor.IsApproved = dto.IsApproved;

            await _context.SaveChangesAsync(cancellationToken);

            return (await GetByIdAsync(vendor.Id, cancellationToken))!;
        }

        private static void ValidateCreateOrUpdate(string fullName, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new InvalidOperationException("الاسم مطلوب.");
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new InvalidOperationException("رقم الهاتف مطلوب.");
        }
    }
}
