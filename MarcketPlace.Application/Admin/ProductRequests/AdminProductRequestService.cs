using MarcketPlace.Application.Admin.ProductRequests.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.ProductRequests
{
    public class AdminProductRequestService : IAdminProductRequestService
    {
        private readonly AppDbContext _context;

        public AdminProductRequestService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AdminProductRequestListItemDto>> GetPendingAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductRequests
                .AsNoTracking()
                .Where(x => x.Status == ProductApprovalStatus.Pending)
                .OrderByDescending(x => x.RequestedAt)
                .Select(x => new AdminProductRequestListItemDto
                {
                    Id = x.Id,
                    VendorId = x.VendorId,
                    VendorOwnerName = x.Vendor.User.FullName,
                    StoreId = x.StoreId,
                    StoreNameAr = x.Store.NameAr,
                    StoreNameEn = x.Store.NameEn,
                    CategoryId = x.CategoryId,
                    CategoryNameAr = x.Category.NameAr,
                    CategoryNameEn = x.Category.NameEn,
                    ProductNameAr = x.NameAr,
                    ProductNameEn = x.NameEn,
                    Status = x.Status,
                    RequestedAt = x.RequestedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<AdminProductRequestDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.ProductRequests
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new AdminProductRequestDetailsDto
                {
                    Id = x.Id,
                    VendorId = x.VendorId,
                    VendorOwnerName = x.Vendor.User.FullName,
                    StoreId = x.StoreId,
                    StoreNameAr = x.Store.NameAr,
                    StoreNameEn = x.Store.NameEn,
                    CategoryId = x.CategoryId,
                    CategoryNameAr = x.Category.NameAr,
                    CategoryNameEn = x.Category.NameEn,
                    ProductNameAr = x.NameAr,
                    ProductNameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    Image = x.Image,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    MinStockQuantity = x.MinStockQuantity,
                    Status = x.Status,
                    AdminNote = x.AdminNote,
                    RequestedAt = x.RequestedAt,
                    ReviewedAt = x.ReviewedAt,
                    ReviewedByUserId = x.ReviewedByUserId,
                    ProductId = x.ProductId
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AdminProductRequestDetailsDto?> ApproveAsync(int requestId, int? adminUserId, string? note, CancellationToken cancellationToken = default)
        {
            var request = await _context.ProductRequests
                .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

            if (request is null)
                return null;

            if (request.Status != ProductApprovalStatus.Pending)
                throw new Exception("تمت مراجعة هذا الطلب مسبقًا.");

            var storeExists = await _context.Stores
                .AnyAsync(x => x.Id == request.StoreId && x.VendorId == request.VendorId && x.IsActive, cancellationToken);

            if (!storeExists)
                throw new Exception("المتجر غير موجود أو غير نشط أو لا يتبع لهذا التاجر.");

            var categoryExists = await _context.Categories
                .AnyAsync(x => x.Id == request.CategoryId && x.IsActive, cancellationToken);

            if (!categoryExists)
                throw new Exception("التصنيف غير موجود أو غير نشط.");

            var duplicateProductExists = await _context.Products
                .AnyAsync(x =>
                    x.StoreId == request.StoreId &&
                    x.CategoryId == request.CategoryId &&
                    (x.NameAr == request.NameAr || x.NameEn == request.NameEn),
                    cancellationToken);

            if (duplicateProductExists)
                throw new Exception("يوجد منتج بنفس الاسم العربي أو الإنجليزي داخل نفس المتجر والتصنيف.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var product = new Product
            {
                StoreId = request.StoreId,
                CategoryId = request.CategoryId,
                NameAr = request.NameAr,
                NameEn = request.NameEn,
                DescriptionAr = request.DescriptionAr,
                DescriptionEn = request.DescriptionEn,
                Image = request.Image,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                MinStockQuantity = request.MinStockQuantity,
                ApprovalStatus = ProductApprovalStatus.Approved,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            request.Status = ProductApprovalStatus.Approved;
            request.AdminNote = note?.Trim();
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByUserId = adminUserId;
            request.ProductId = product.Id;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await GetByIdAsync(request.Id, cancellationToken);
        }

        public async Task<AdminProductRequestDetailsDto?> RejectAsync(int requestId, int? adminUserId, string? note, CancellationToken cancellationToken = default)
        {
            var request = await _context.ProductRequests
                .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

            if (request is null)
                return null;

            if (request.Status != ProductApprovalStatus.Pending)
                throw new Exception("تمت مراجعة هذا الطلب مسبقًا.");

            request.Status = ProductApprovalStatus.Rejected;
            request.AdminNote = note?.Trim();
            request.ReviewedAt = DateTime.UtcNow;
            request.ReviewedByUserId = adminUserId;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(request.Id, cancellationToken);
        }
    }
}