using MarcketPlace.Application.Admin.Notifications.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Notifications
{
    public class AdminNotificationService : IAdminNotificationService
    {
        private const string TypeProductRequest = "ProductRequest";
        private static readonly TimeSpan HideReadAfter = TimeSpan.FromHours(24);

        private readonly AppDbContext _context;

        public AdminNotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AdminNotificationDto>> GetForUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow - HideReadAfter;

            return await _context.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Where(x => !x.IsRead || (x.ReadAt != null && x.ReadAt.Value > cutoff))
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .Select(x => new AdminNotificationDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Body = x.Body,
                    Type = x.Type,
                    ReferenceId = x.ReferenceId,
                    IsRead = x.IsRead,
                    ReadAt = x.ReadAt,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetUnreadCountAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var cutoff = DateTime.UtcNow - HideReadAfter;

            return await _context.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.IsRead)
                .CountAsync(cancellationToken);
        }

        public async Task MarkAsReadAsync(
            int notificationId,
            int userId,
            CancellationToken cancellationToken = default)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, cancellationToken);

            if (notification is null)
                return;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task CreateProductRequestNotificationAsync(
            int productRequestId,
            string productNameAr,
            string storeNameAr,
            string categoryNameAr,
            CancellationToken cancellationToken = default)
        {
            var adminUserIds = await _context.Users
                .AsNoTracking()
                .Where(x => x.Role == UserRole.SuperAdmin && x.IsActive)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (adminUserIds.Count == 0)
                return;

            var title = "طلب إضافة منتج جديد";
            var body = $"متجر {storeNameAr} طلب إضافة منتج «{productNameAr}» في تصنيف {categoryNameAr}";

            var notifications = adminUserIds.Select(adminId => new Notification
            {
                UserId = adminId,
                Title = title,
                Body = body,
                Type = TypeProductRequest,
                ReferenceId = productRequestId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
