using MarcketPlace.Application.Admin.Customers.Dtos;
using CustomerEntity = MarcketPlace.Domain.Entities.Customer;
using UserEntity = MarcketPlace.Domain.Entities.User;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Customers
{
    public class CustomerAdminService : ICustomerAdminService
    {
        private readonly AppDbContext _context;

        public CustomerAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CustomerListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .Select(x => new CustomerListItemDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    FullName = x.User.FullName,
                    PhoneNumber = x.User.PhoneNumber,
                    IsActive = x.User.IsActive,
                    CreatedAt = x.CreatedAt,
                    OrdersCount = x.Orders.Count(),
                    DriverRatingsCount = x.DriverRatings.Count()
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            foreach (var customer in customers)
            {
                customer.StatusText = customer.IsActive ? "نشط" : "غير نشط";
                customer.CreatedAtText = customer.CreatedAt.ToString("yyyy-MM-dd hh:mm tt");
            }

            return customers;
        }

        public async Task<CustomerDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new CustomerDetailsDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    FullName = x.User.FullName,
                    PhoneNumber = x.User.PhoneNumber,
                    IsActive = x.User.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.User.UpdatedAt,
                    OrdersCount = x.Orders.Count(),
                    DriverRatingsCount = x.DriverRatings.Count()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException("العميل غير موجود.");

            customer.StatusText = customer.IsActive ? "نشط" : "غير نشط";
            customer.CreatedAtText = customer.CreatedAt.ToString("yyyy-MM-dd hh:mm tt");
            customer.UpdatedAtText = customer.UpdatedAt?.ToString("yyyy-MM-dd hh:mm tt");

            return customer;
        }

        public async Task<CustomerDetailsDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
        {
            ValidateCreateOrUpdate(dto.FullName, dto.PhoneNumber);

            var fullName = dto.FullName.Trim();
            var phoneNumber = dto.PhoneNumber.Trim();

            var phoneExists = await _context.Users
                .AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

            if (phoneExists)
                throw new InvalidOperationException("رقم الهاتف مستخدم مسبقًا.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var user = new UserEntity
            {
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Role = UserRole.Customer,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            var rawPassword = $"{phoneNumber}@@";
            var passwordHasher = new PasswordHasher<UserEntity>();
            user.PasswordHash = passwordHasher.HashPassword(user, rawPassword);

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var customer = new CustomerEntity
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return await GetByIdAsync(customer.Id, cancellationToken);
        }

        public async Task<CustomerDetailsDto> UpdateAsync(int id, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
        {
            ValidateCreateOrUpdate(dto.FullName, dto.PhoneNumber);

            var customer = await _context.Customers
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException("العميل غير موجود.");

            var fullName = dto.FullName.Trim();
            var phoneNumber = dto.PhoneNumber.Trim();

            var phoneExists = await _context.Users
                .AnyAsync(x => x.PhoneNumber == phoneNumber && x.Id != customer.UserId, cancellationToken);

            if (phoneExists)
                throw new InvalidOperationException("رقم الهاتف مستخدم مسبقًا.");

            customer.User.FullName = fullName;
            customer.User.PhoneNumber = phoneNumber;
            customer.User.IsActive = dto.IsActive;
            customer.User.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(customer.Id, cancellationToken);
        }

        public async Task<CustomerDetailsDto> UpdateStatusAsync(int id, UpdateCustomerStatusDto dto, CancellationToken cancellationToken = default)
        {
            var customer = await _context.Customers
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException("العميل غير موجود.");

            customer.User.IsActive = dto.IsActive;
            customer.User.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(customer.Id, cancellationToken);
        }

        private static void ValidateCreateOrUpdate(string fullName, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new InvalidOperationException("اسم العميل مطلوب.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new InvalidOperationException("رقم هاتف العميل مطلوب.");
        }
    }
}