using MarcketPlace.Application.Customer.Ratings.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Customer.Ratings
{
    public class CustomerRatingService : ICustomerRatingService
    {
        private readonly AppDbContext _context;

        public CustomerRatingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerOrderRatingAvailabilityDto> GetOrderRatingAvailabilityAsync(
            int customerUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var (customerId, order) = await LoadOwnedOrderAsync(customerUserId, orderId, cancellationToken);

            var hasDriverRating = false;

            if (order.DriverId.HasValue)
            {
                hasDriverRating = await _context.DriverRatings
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.OrderId == order.Id &&
                        x.DriverId == order.DriverId.Value &&
                        x.CustomerId == customerId,
                        cancellationToken);
            }

            var ratedStoreIds = await _context.StoreRatings
                .AsNoTracking()
                .Where(x => x.OrderId == order.Id && x.CustomerId == customerId)
                .Select(x => x.StoreId)
                .ToListAsync(cancellationToken);

            var canRateDriverBecauseDelivered =
                order.DriverId.HasValue &&
                order.Status == OrderStatus.Delivered;

            var canRateDriverBecauseDriverCancelled =
                order.DriverId.HasValue &&
                order.Status == OrderStatus.Cancelled &&
                order.CancelledByUserId.HasValue &&
                order.Driver is not null &&
                order.Driver.UserId == order.CancelledByUserId.Value;

            return new CustomerOrderRatingAvailabilityDto
            {
                OrderId = order.Id,
                OrderStatus = order.Status,

                CanRateDriver = (canRateDriverBecauseDelivered || canRateDriverBecauseDriverCancelled) && !hasDriverRating,
                HasDriverRating = hasDriverRating,
                DriverRatingOpenedBecauseDelivered = canRateDriverBecauseDelivered,
                DriverRatingOpenedBecauseDriverCancelled = canRateDriverBecauseDriverCancelled,

                Driver = order.Driver is null
                    ? null
                    : new CustomerOrderRatingDriverInfoDto
                    {
                        DriverId = order.Driver.Id,
                        FullName = order.Driver.User?.FullName ?? string.Empty,
                        PhoneNumber = order.Driver.User?.PhoneNumber ?? string.Empty
                    },

                Stores = order.OrderStores
                    .OrderBy(x => x.Id)
                    .Select(x => new CustomerOrderStoreRatingAvailabilityItemDto
                    {
                        StoreId = x.StoreId,
                        StoreNameAr = x.Store?.NameAr ?? string.Empty,
                        StoreNameEn = x.Store?.NameEn ?? string.Empty,
                        StoreOrderStatus = x.Status,
                        HasRating = ratedStoreIds.Contains(x.StoreId),
                        CanRate =
                            order.Status == OrderStatus.Delivered &&
                            x.Status != OrderStoreStatus.Cancelled &&
                            !ratedStoreIds.Contains(x.StoreId)
                    })
                    .ToList()
            };
        }

        public async Task<CustomerDriverRatingDto> CreateDriverRatingAsync(
            int customerUserId,
            int orderId,
            CreateDriverRatingDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            ValidateScore(dto.Score);

            var (customerId, order) = await LoadOwnedOrderAsync(customerUserId, orderId, cancellationToken);

            if (order.DriverId is null || order.Driver is null)
                throw new InvalidOperationException("لا يوجد سائق مرتبط بهذا الطلب.");

            var canRateBecauseDelivered = order.Status == OrderStatus.Delivered;

            var canRateBecauseDriverCancelled =
                order.Status == OrderStatus.Cancelled &&
                order.CancelledByUserId.HasValue &&
                order.Driver.UserId == order.CancelledByUserId.Value;

            if (!canRateBecauseDelivered && !canRateBecauseDriverCancelled)
                throw new InvalidOperationException("لا يمكن تقييم السائق إلا بعد التسليم أو إذا قام السائق بإلغاء الطلب.");

            var alreadyRated = await _context.DriverRatings
                .AsNoTracking()
                .AnyAsync(x =>
                    x.OrderId == order.Id &&
                    x.DriverId == order.DriverId.Value &&
                    x.CustomerId == customerId,
                    cancellationToken);

            if (alreadyRated)
                throw new InvalidOperationException("تم تقييم السائق مسبقًا لهذا الطلب.");

            var rating = new DriverRating
            {
                OrderId = order.Id,
                DriverId = order.DriverId.Value,
                CustomerId = customerId,
                Score = dto.Score,
                Comment = NormalizeComment(dto.Comment),
                CreatedAt = DateTime.UtcNow
            };

            _context.DriverRatings.Add(rating);
            await _context.SaveChangesAsync(cancellationToken);

            return new CustomerDriverRatingDto
            {
                Id = rating.Id,
                OrderId = rating.OrderId,
                DriverId = rating.DriverId,
                CustomerId = rating.CustomerId,
                Score = rating.Score,
                Comment = rating.Comment,
                CreatedAt = rating.CreatedAt
            };
        }

        public async Task<CustomerStoreRatingDto> CreateStoreRatingAsync(
            int customerUserId,
            int orderId,
            int storeId,
            CreateStoreRatingDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            if (storeId <= 0)
                throw new InvalidOperationException("رقم المتجر غير صالح.");

            ValidateScore(dto.Score);

            var (customerId, order) = await LoadOwnedOrderAsync(customerUserId, orderId, cancellationToken);

            var orderStore = order.OrderStores.FirstOrDefault(x => x.StoreId == storeId);

            if (orderStore is null)
                throw new KeyNotFoundException("هذا المتجر غير موجود ضمن الطلب.");

            if (order.Status != OrderStatus.Delivered)
                throw new InvalidOperationException("لا يمكن تقييم المتجر قبل تسليم الطلب.");

            if (orderStore.Status == OrderStoreStatus.Cancelled)
                throw new InvalidOperationException("لا يمكن تقييم متجر تم إلغاء طلبه داخل هذا الطلب.");

            var alreadyRated = await _context.StoreRatings
                .AsNoTracking()
                .AnyAsync(x =>
                    x.OrderId == order.Id &&
                    x.StoreId == storeId &&
                    x.CustomerId == customerId,
                    cancellationToken);

            if (alreadyRated)
                throw new InvalidOperationException("تم تقييم المتجر مسبقًا لهذا الطلب.");

            var rating = new StoreRating
            {
                OrderId = order.Id,
                StoreId = storeId,
                CustomerId = customerId,
                Score = dto.Score,
                Comment = NormalizeComment(dto.Comment),
                CreatedAt = DateTime.UtcNow
            };

            _context.StoreRatings.Add(rating);
            await _context.SaveChangesAsync(cancellationToken);

            return new CustomerStoreRatingDto
            {
                Id = rating.Id,
                OrderId = rating.OrderId,
                StoreId = rating.StoreId,
                CustomerId = rating.CustomerId,
                Score = rating.Score,
                Comment = rating.Comment,
                CreatedAt = rating.CreatedAt
            };
        }

        private async Task<(int CustomerId, Order Order)> LoadOwnedOrderAsync(
            int customerUserId,
            int orderId,
            CancellationToken cancellationToken)
        {
            if (orderId <= 0)
                throw new InvalidOperationException("رقم الطلب غير صالح.");

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == customerUserId, cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException("الزبون غير موجود.");

            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Driver)
                    .ThenInclude(x => x!.User)
                .Include(x => x.OrderStores)
                    .ThenInclude(x => x.Store)
                .FirstOrDefaultAsync(
                    x => x.Id == orderId && x.CustomerId == customer.Id,
                    cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            return (customer.Id, order);
        }

        private static void ValidateScore(int score)
        {
            if (score < 1 || score > 5)
                throw new InvalidOperationException("التقييم يجب أن يكون بين 1 و 5.");
        }

        private static string? NormalizeComment(string? comment)
        {
            return string.IsNullOrWhiteSpace(comment)
                ? null
                : comment.Trim();
        }
    }
}