using MarcketPlace.Application.Admin.Notifications.Dtos;

namespace MarcketPlace.Application.Admin.Notifications
{
    public interface IAdminNotificationService
    {
        Task<IReadOnlyList<AdminNotificationDto>> GetForUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<int> GetUnreadCountAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task MarkAsReadAsync(
            int notificationId,
            int userId,
            CancellationToken cancellationToken = default);

        Task CreateProductRequestNotificationAsync(
            int productRequestId,
            string productNameAr,
            string storeNameAr,
            string categoryNameAr,
            CancellationToken cancellationToken = default);
    }
}
