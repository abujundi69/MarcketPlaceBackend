using MarcketPlace.Application.Vendor.Categories.Dtos;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Vendor.Categories
{
    public class VendorCategoryService : IVendorCategoryService
    {
        private readonly AppDbContext _context;

        public VendorCategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<VendorCategoryListItemDto>> GetActiveCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.NameAr)
                .Select(x => new VendorCategoryListItemDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn
                })
                .ToListAsync(cancellationToken);
        }
    }
}
