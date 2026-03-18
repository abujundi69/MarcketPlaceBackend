using MarcketPlace.Application.Customer.DriverRatings.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Customer.DriverRatings
{
    public class CustomerDriverRatingService : ICustomerDriverRatingService
    {
        private readonly AppDbContext _context;

        public CustomerDriverRatingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDriverRatingDto> RateOrderDriverAsync(
            int customerUserId,
            int orderId,
            CreateCustomerDriverRatingDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            if (dto.Score < 1 || dto.Score > 5)
                throw new InvalidOperationException("التقييم يجب أن يكون بين 1 و 5.");

            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var order = await _context.Orders
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == orderId && x.CustomerId == customerId, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            if (order.CancelledAt.HasValue || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("لا يمكن تقييم سائق لطلب ملغي.");

            if (order.Status != OrderStatus.Delivered)
                throw new InvalidOperationException("يمكن تقييم السائق فقط بعد تسليم الطلب.");

            if (!order.DriverId.HasValue || order.Driver is null)
                throw new InvalidOperationException("هذا الطلب لا يحتوي على سائق لتقييمه.");

            var rating = await _context.DriverRatings
                .FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);

            if (rating is null)
            {
                rating = new DriverRating
                {
                    OrderId = order.Id,
                    DriverId = order.DriverId.Value,
                    CustomerId = customerId,
                    Score = dto.Score,
                    Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.DriverRatings.Add(rating);
            }
            else
            {
                rating.Score = dto.Score;
                rating.Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new CustomerDriverRatingDto
            {
                Id = rating.Id,
                OrderId = rating.OrderId,
                DriverId = rating.DriverId,
                DriverName = order.Driver.User.FullName,
                DriverPhoneNumber = order.Driver.User.PhoneNumber,
                Score = rating.Score,
                Comment = rating.Comment,
                CreatedAt = rating.CreatedAt
            };
        }

        public async Task<CustomerDriverRatingDto> GetByOrderAsync(
            int customerUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var rating = await _context.DriverRatings
                .AsNoTracking()
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.OrderId == orderId && x.CustomerId == customerId, cancellationToken);

            if (rating is null)
                throw new KeyNotFoundException("لا يوجد تقييم لهذا الطلب.");

            return new CustomerDriverRatingDto
            {
                Id = rating.Id,
                OrderId = rating.OrderId,
                DriverId = rating.DriverId,
                DriverName = rating.Driver.User.FullName,
                DriverPhoneNumber = rating.Driver.User.PhoneNumber,
                Score = rating.Score,
                Comment = rating.Comment,
                CreatedAt = rating.CreatedAt
            };
        }

        public async Task<IReadOnlyList<CustomerDriverRatingDto>> GetMyRatingsAsync(
            int customerUserId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var items = await _context.DriverRatings
                .AsNoTracking()
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return items.Select(x => new CustomerDriverRatingDto
            {
                Id = x.Id,
                OrderId = x.OrderId,
                DriverId = x.DriverId,
                DriverName = x.Driver.User.FullName,
                DriverPhoneNumber = x.Driver.User.PhoneNumber,
                Score = x.Score,
                Comment = x.Comment,
                CreatedAt = x.CreatedAt
            }).ToList();
        }

        private async Task<int> GetCustomerIdAsync(
            int customerUserId,
            CancellationToken cancellationToken)
        {
            var customerId = await _context.Customers
                .Where(x => x.UserId == customerUserId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (!customerId.HasValue)
                throw new KeyNotFoundException("الزبون غير موجود.");

            return customerId.Value;
        }
    }
}