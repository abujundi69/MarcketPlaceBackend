using MarcketPlace.Application.Admin.Drivers.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Drivers
{
    public class DriverAdminService : IDriverAdminService
    {
        private readonly AppDbContext _context;

        public DriverAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<DriverListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var drivers = await _context.Drivers
                .AsNoTracking()
                .Select(x => new DriverListItemDto
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    FullName = x.User.FullName,
                    PhoneNumber = x.User.PhoneNumber,
                    VehicleType = x.VehicleType,
                    VehicleNumber = x.VehicleNumber,
                    IsActive = x.User.IsActive,
                    CreatedAt = x.CreatedAt,

                    RatingsCount = x.DriverRatings.Count(),
                    AverageRating = x.DriverRatings
                        .Select(r => (double?)r.Score)
                        .Average() ?? 0
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            foreach (var driver in drivers)
            {
                driver.StatusText = driver.IsActive ? "Active" : "Inactive";
                driver.CreatedAtText = driver.CreatedAt.ToString("yyyy-MM-dd hh:mm tt");
                driver.AverageRatingText = driver.RatingsCount > 0
                    ? $"{driver.AverageRating:0.0} / 5"
                    : "No ratings";
            }

            return drivers;
        }

        public async Task<DriverDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var driver = await _context.Drivers
                .AsNoTracking()
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (driver is null)
                throw new KeyNotFoundException("Driver not found.");

            return MapToDetails(driver);
        }

        public async Task<DriverDetailsDto> CreateAsync(CreateDriverDto dto, CancellationToken cancellationToken = default)
        {
            ValidateCreateOrUpdate(dto.FullName, dto.PhoneNumber, dto.VehicleType, dto.VehicleNumber);

            var phoneNumber = dto.PhoneNumber.Trim();
            var vehicleNumber = dto.VehicleNumber.Trim();

            var phoneExists = await _context.Users
                .AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

            if (phoneExists)
                throw new InvalidOperationException("Phone number already exists.");

            var vehicleExists = await _context.Drivers
                .AnyAsync(x => x.VehicleNumber == vehicleNumber, cancellationToken);

            if (vehicleExists)
                throw new InvalidOperationException("Vehicle number already exists.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                PhoneNumber = phoneNumber,
                Role = UserRole.Driver,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var driver = new Driver
            {
                UserId = user.Id,
                VehicleType = dto.VehicleType.Trim(),
                VehicleNumber = vehicleNumber,
                CreatedAt = DateTime.UtcNow
            };

            _context.Drivers.Add(driver);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return await GetByIdAsync(driver.Id, cancellationToken);
        }

        public async Task<DriverDetailsDto> UpdateAsync(int id, UpdateDriverDto dto, CancellationToken cancellationToken = default)
        {
            ValidateCreateOrUpdate(dto.FullName, dto.PhoneNumber, dto.VehicleType, dto.VehicleNumber);

            var driver = await _context.Drivers
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (driver is null)
                throw new KeyNotFoundException("Driver not found.");

            var phoneNumber = dto.PhoneNumber.Trim();
            var vehicleNumber = dto.VehicleNumber.Trim();

            var phoneExists = await _context.Users
                .AnyAsync(x => x.PhoneNumber == phoneNumber && x.Id != driver.UserId, cancellationToken);

            if (phoneExists)
                throw new InvalidOperationException("Phone number already exists.");

            var vehicleExists = await _context.Drivers
                .AnyAsync(x => x.VehicleNumber == vehicleNumber && x.Id != driver.Id, cancellationToken);

            if (vehicleExists)
                throw new InvalidOperationException("Vehicle number already exists.");

            driver.User.FullName = dto.FullName.Trim();
            driver.User.PhoneNumber = phoneNumber;
            driver.User.IsActive = dto.IsActive;
            driver.User.UpdatedAt = DateTime.UtcNow;

            driver.VehicleType = dto.VehicleType.Trim();
            driver.VehicleNumber = vehicleNumber;

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDetails(driver);
        }

        public async Task<DriverDetailsDto> UpdateStatusAsync(int id, UpdateDriverStatusDto dto, CancellationToken cancellationToken = default)
        {
            var driver = await _context.Drivers
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (driver is null)
                throw new KeyNotFoundException("Driver not found.");

            driver.User.IsActive = dto.IsActive;
            driver.User.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDetails(driver);
        }

        private static void ValidateCreateOrUpdate(
            string fullName,
            string phoneNumber,
            string vehicleType,
            string vehicleNumber)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new InvalidOperationException("Full name is required.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new InvalidOperationException("Phone number is required.");

            if (string.IsNullOrWhiteSpace(vehicleType))
                throw new InvalidOperationException("Vehicle type is required.");

            if (string.IsNullOrWhiteSpace(vehicleNumber))
                throw new InvalidOperationException("Vehicle number is required.");
        }

        private static DriverDetailsDto MapToDetails(Driver driver)
        {
            return new DriverDetailsDto
            {
                Id = driver.Id,
                UserId = driver.UserId,
                FullName = driver.User.FullName,
                PhoneNumber = driver.User.PhoneNumber,
                VehicleType = driver.VehicleType,
                VehicleNumber = driver.VehicleNumber,
                IsActive = driver.User.IsActive,
                StatusText = driver.User.IsActive ? "Active" : "Inactive",
                CreatedAt = driver.CreatedAt,
                UpdatedAt = driver.User.UpdatedAt,
                CreatedAtText = driver.CreatedAt.ToString("yyyy-MM-dd hh:mm tt"),
                UpdatedAtText = driver.User.UpdatedAt?.ToString("yyyy-MM-dd hh:mm tt")
            };
        }
    }
}