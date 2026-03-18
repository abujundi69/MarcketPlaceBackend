using MarcketPlace.Application.Admin.Categories.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Categories
{
    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly AppDbContext _context;

        public AdminCategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminCategoryDto> CreateAsync(
            CreateCategoryDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            await ValidateCategoryInputAsync(
                currentCategoryId: null,
                dto.NameAr,
                dto.NameEn,
                dto.ParentId,
                cancellationToken);

            var entity = new Category
            {
                NameAr = dto.NameAr.Trim(),
                NameEn = dto.NameEn.Trim(),
                Image = dto.Image,
                DisplayOrder = dto.DisplayOrder,
                ParentId = dto.ParentId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        public async Task<IReadOnlyList<AdminCategoryListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var items = await _context.Categories
                .AsNoTracking()
                .Select(x => new AdminCategoryListItemDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    Image = x.Image,
                    DisplayOrder = x.DisplayOrder,
                    ParentId = x.ParentId,
                    ParentNameAr = x.Parent != null ? x.Parent.NameAr : null,
                    ParentNameEn = x.Parent != null ? x.Parent.NameEn : null,
                    IsActive = x.IsActive,
                    ChildrenCount = x.Children.Count,
                    ProductsCount = x.Products.Count
                })
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.NameAr)
                .ToListAsync(cancellationToken);

            return items;
        }

        public async Task<AdminCategoryDto> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var item = await _context.Categories
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new AdminCategoryDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    Image = x.Image,
                    DisplayOrder = x.DisplayOrder,
                    ParentId = x.ParentId,
                    ParentNameAr = x.Parent != null ? x.Parent.NameAr : null,
                    ParentNameEn = x.Parent != null ? x.Parent.NameEn : null,
                    IsActive = x.IsActive,
                    ChildrenCount = x.Children.Count,
                    ProductsCount = x.Products.Count,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item is null)
                throw new KeyNotFoundException("الكاتيجوري المطلوبة غير موجودة.");

            return item;
        }

        public async Task<AdminCategoryDto> UpdateAsync(
            int id,
            UpdateCategoryDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            var entity = await _context.Categories
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity is null)
                throw new KeyNotFoundException("الكاتيجوري المطلوبة غير موجودة.");

            await ValidateCategoryInputAsync(
                currentCategoryId: id,
                dto.NameAr,
                dto.NameEn,
                dto.ParentId,
                cancellationToken);

            entity.NameAr = dto.NameAr.Trim();
            entity.NameEn = dto.NameEn.Trim();
            entity.Image = dto.Image;
            entity.DisplayOrder = dto.DisplayOrder;
            entity.ParentId = dto.ParentId;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(entity.Id, cancellationToken);
        }

        private async Task ValidateCategoryInputAsync(
            int? currentCategoryId,
            string nameAr,
            string nameEn,
            int? parentId,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(nameAr))
                throw new InvalidOperationException("الاسم العربي مطلوب.");

            if (string.IsNullOrWhiteSpace(nameEn))
                throw new InvalidOperationException("الاسم الإنجليزي مطلوب.");

            if (parentId.HasValue)
            {
                if (currentCategoryId.HasValue && parentId.Value == currentCategoryId.Value)
                    throw new InvalidOperationException("لا يمكن جعل الكاتيجوري أبًا لنفسها.");

                var parentExists = await _context.Categories
                    .AnyAsync(x => x.Id == parentId.Value, cancellationToken);

                if (!parentExists)
                    throw new InvalidOperationException("الكاتيجوري الأب غير موجودة.");

                if (currentCategoryId.HasValue)
                {
                    var createsCycle = await WouldCreateCycleAsync(
                        currentCategoryId.Value,
                        parentId.Value,
                        cancellationToken);

                    if (createsCycle)
                        throw new InvalidOperationException("لا يمكن ربط الكاتيجوري بأحد أبنائها.");
                }
            }

            var duplicateAr = await _context.Categories
                .AnyAsync(x =>
                    x.Id != (currentCategoryId ?? 0) &&
                    x.NameAr == nameAr.Trim(),
                    cancellationToken);

            if (duplicateAr)
                throw new InvalidOperationException("يوجد كاتيجوري أخرى بنفس الاسم العربي.");

            var duplicateEn = await _context.Categories
                .AnyAsync(x =>
                    x.Id != (currentCategoryId ?? 0) &&
                    x.NameEn == nameEn.Trim(),
                    cancellationToken);

            if (duplicateEn)
                throw new InvalidOperationException("يوجد كاتيجوري أخرى بنفس الاسم الإنجليزي.");
        }

        private async Task<bool> WouldCreateCycleAsync(
            int currentCategoryId,
            int newParentId,
            CancellationToken cancellationToken)
        {
            var visited = new HashSet<int>();
            var parentId = newParentId;

            while (true)
            {
                if (parentId == currentCategoryId)
                    return true;

                if (!visited.Add(parentId))
                    return true;

                var parent = await _context.Categories
                    .AsNoTracking()
                    .Where(x => x.Id == parentId)
                    .Select(x => new { x.ParentId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (parent is null || !parent.ParentId.HasValue)
                    return false;

                parentId = parent.ParentId.Value;
            }
        }
    }
}