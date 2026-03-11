using MarcketPlace.Application.Vendor.Stores.Dtos;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Vendor.Stores
{
    public class VendorStoreService : IVendorStoreService
    {
        private readonly AppDbContext _context;

        public VendorStoreService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<VendorStoreListItemDto>> GetMyStoresAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var stores = await _context.Stores
                .AsNoTracking()
                .Where(x => x.Vendor != null && x.Vendor.UserId == userId && x.IsActive)
                .OrderBy(x => x.NameAr)
                .Select(x => new
                {
                    x.Id,
                    x.NameAr,
                    x.NameEn,
                    x.Logo
                })
                .ToListAsync(cancellationToken);

            return stores
                .Select(x => new VendorStoreListItemDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    HasLogo = x.Logo != null && x.Logo.Length > 0,
                    LogoBase64 = x.Logo != null && x.Logo.Length > 0
                        ? Convert.ToBase64String(x.Logo)
                        : null
                })
                .ToList();
        }
    }
}