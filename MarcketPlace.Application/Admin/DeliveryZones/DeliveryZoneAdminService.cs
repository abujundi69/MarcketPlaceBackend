using MarcketPlace.Application.Admin.DeliveryZones.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.DeliveryZones
{
    public class DeliveryZoneAdminService : IDeliveryZoneAdminService
    {
        private readonly AppDbContext _context;

        public DeliveryZoneAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DeliveryZoneDto> CreateAsync(
            CreateDeliveryZoneDto dto,
            CancellationToken cancellationToken = default)
        {
            var nameAr = dto.NameAr?.Trim();
            var nameEn = dto.NameEn?.Trim();

            if (string.IsNullOrWhiteSpace(nameAr))
                throw new InvalidOperationException("اسم المنطقة بالعربي مطلوب.");

            if (string.IsNullOrWhiteSpace(nameEn))
                throw new InvalidOperationException("اسم المنطقة بالإنجليزي مطلوب.");

            if (dto.DeliveryFee < 0)
                throw new InvalidOperationException("رسوم التوصيل لا يمكن أن تكون أقل من صفر.");

            var duplicateExists = await _context.DeliveryZones
                .AsNoTracking()
                .AnyAsync(x => x.NameAr == nameAr || x.NameEn == nameEn, cancellationToken);

            if (duplicateExists)
                throw new InvalidOperationException("توجد منطقة بنفس الاسم العربي أو الإنجليزي.");

            var zone = new DeliveryZone
            {
                NameAr = nameAr,
                NameEn = nameEn,
                DeliveryFee = dto.DeliveryFee,
                CreatedAt = DateTime.UtcNow
            };

            _context.DeliveryZones.Add(zone);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(zone);
        }

        public async Task<IReadOnlyList<DeliveryZoneDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.DeliveryZones
                .AsNoTracking()
                .OrderBy(x => x.NameAr)
                .Select(x => new DeliveryZoneDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DeliveryFee = x.DeliveryFee,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<DeliveryZoneDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.DeliveryZones
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new DeliveryZoneDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DeliveryFee = x.DeliveryFee,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<DeliveryZoneDto> UpdateAsync(
            int id,
            UpdateDeliveryZoneDto dto,
            CancellationToken cancellationToken = default)
        {
            var zone = await _context.DeliveryZones
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (zone is null)
                throw new KeyNotFoundException("المنطقة غير موجودة.");

            var nameAr = dto.NameAr?.Trim();
            var nameEn = dto.NameEn?.Trim();

            if (string.IsNullOrWhiteSpace(nameAr))
                throw new InvalidOperationException("اسم المنطقة بالعربي مطلوب.");

            if (string.IsNullOrWhiteSpace(nameEn))
                throw new InvalidOperationException("اسم المنطقة بالإنجليزي مطلوب.");

            if (dto.DeliveryFee < 0)
                throw new InvalidOperationException("رسوم التوصيل لا يمكن أن تكون أقل من صفر.");

            var duplicateExists = await _context.DeliveryZones
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id != id &&
                    (x.NameAr == nameAr || x.NameEn == nameEn),
                    cancellationToken);

            if (duplicateExists)
                throw new InvalidOperationException("توجد منطقة بنفس الاسم العربي أو الإنجليزي.");

            zone.NameAr = nameAr;
            zone.NameEn = nameEn;
            zone.DeliveryFee = dto.DeliveryFee;
            zone.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(zone);
        }

        private static DeliveryZoneDto MapToDto(DeliveryZone zone)
        {
            return new DeliveryZoneDto
            {
                Id = zone.Id,
                NameAr = zone.NameAr,
                NameEn = zone.NameEn,
                DeliveryFee = zone.DeliveryFee,
                CreatedAt = zone.CreatedAt,
                UpdatedAt = zone.UpdatedAt
            };
        }
    }
}