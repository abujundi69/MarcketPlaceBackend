using MarcketPlace.Application.Admin.Categories.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Categories
{
    public class CategoryAdminService : ICategoryAdminService
    {
        private readonly AppDbContext _context;

        public CategoryAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
        {
            var nameAr = dto.NameAr?.Trim();
            var nameEn = dto.NameEn?.Trim();

            if (string.IsNullOrWhiteSpace(nameAr))
                throw new Exception("الاسم العربي مطلوب.");

            if (string.IsNullOrWhiteSpace(nameEn))
                throw new Exception("الاسم الإنجليزي مطلوب.");

            int? parentId = dto.ParentId;
            if (parentId <= 0)
                parentId = null;

            if (parentId.HasValue)
            {
                var parentExists = await _context.Categories
                    .AnyAsync(x => x.Id == parentId.Value, cancellationToken);

                if (!parentExists)
                    throw new Exception("التصنيف الأب غير موجود.");
            }

            var duplicateExists = await _context.Categories
                .AnyAsync(x =>
                    x.ParentId == parentId &&
                    (x.NameAr == nameAr || x.NameEn == nameEn),
                    cancellationToken);

            if (duplicateExists)
                throw new Exception("يوجد تصنيف بنفس الاسم العربي أو الإنجليزي ضمن نفس التصنيف الأب.");

            var category = new Category
            {
                NameAr = nameAr,
                NameEn = nameEn,
                ParentId = parentId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(category);
        }

        public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(x => x.NameAr)
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    ParentId = x.ParentId,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CategoryDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    ParentId = x.ParentId,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                NameAr = category.NameAr,
                NameEn = category.NameEn,
                ParentId = category.ParentId,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}