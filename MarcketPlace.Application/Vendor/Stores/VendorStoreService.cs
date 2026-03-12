using MarcketPlace.Application.Vendor.Stores.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Vendor.Stores
{
    public class VendorStoreService : IVendorStoreService
    {
        private readonly AppDbContext _context;

        public VendorStoreService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<VendorStoreListItemDto>> GetMyStoresAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var stores = await _context.Stores
                .AsNoTracking()
                .Where(x => x.Vendor != null && x.Vendor.UserId == userId && x.IsActive)
                .OrderBy(x => x.NameAr)
                .Select(x => new
                {
                    x.Id,
                    x.NameAr,
                    x.NameEn,
                    x.Logo
                })
                .ToListAsync(cancellationToken);

            return stores
                .Select(x => new VendorStoreListItemDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    HasLogo = x.Logo != null && x.Logo.Length > 0,
                    LogoBase64 = x.Logo != null && x.Logo.Length > 0
                        ? Convert.ToBase64String(x.Logo)
                        : null
                })
                .ToList();
        }

        public async Task<IReadOnlyList<VendorStoreWorkingHoursDto>> GetMyStoresWorkingHoursAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var stores = await _context.Stores
                .AsNoTracking()
                .Include(x => x.WorkingHours)
                .Where(x => x.Vendor != null && x.Vendor.UserId == userId)
                .OrderBy(x => x.NameAr)
                .ToListAsync(cancellationToken);

            return stores
                .Select(MapStoreWorkingHours)
                .ToList();
        }

        public async Task<VendorStoreWorkingHoursDto> GetStoreWorkingHoursAsync(
            int userId,
            int storeId,
            CancellationToken cancellationToken = default)
        {
            if (storeId <= 0)
                throw new InvalidOperationException("—ﬁ„ «·„ Ã— €Ì— ’«·Õ.");

            var store = await _context.Stores
                .AsNoTracking()
                .Include(x => x.WorkingHours)
                .FirstOrDefaultAsync(
                    x => x.Id == storeId &&
                         x.Vendor != null &&
                         x.Vendor.UserId == userId,
                    cancellationToken);

            if (store is null)
                throw new KeyNotFoundException("«·„ Ã— €Ì— „ÊÃÊœ √Ê ·« Ì »⁄ ·Â–« «· «Ã—.");

            return MapStoreWorkingHours(store);
        }

        public async Task<VendorStoreWorkingHoursDto> UpdateStoreWorkingHoursAsync(
            int userId,
            int storeId,
            UpdateVendorStoreWorkingHoursDto dto,
            CancellationToken cancellationToken = default)
        {
            if (storeId <= 0)
                throw new InvalidOperationException("—ﬁ„ «·„ Ã— €Ì— ’«·Õ.");

            if (dto is null)
                throw new InvalidOperationException("«·»Ì«‰«  «·„—”·… €Ì— ’«·Õ….");

            if (dto.WorkingHours is null || dto.WorkingHours.Count == 0)
                throw new InvalidOperationException("Ì—ÃÏ ≈œŒ«· ”«⁄«  «·œÊ«„.");

            ValidateWorkingHours(dto.WorkingHours);

            var store = await _context.Stores
                .Include(x => x.WorkingHours)
                .FirstOrDefaultAsync(
                    x => x.Id == storeId &&
                         x.Vendor != null &&
                         x.Vendor.UserId == userId,
                    cancellationToken);

            if (store is null)
                throw new KeyNotFoundException("«·„ Ã— €Ì— „ÊÃÊœ √Ê ·« Ì »⁄ ·Â–« «· «Ã—.");

            var utcNow = DateTime.UtcNow;

            foreach (var item in dto.WorkingHours)
            {
                var existing = store.WorkingHours
                    .FirstOrDefault(x => x.Day == item.Day);

                if (existing is null)
                {
                    existing = new StoreWorkingHour
                    {
                        StoreId = store.Id,
                        Day = item.Day,
                        CreatedAt = utcNow
                    };

                    store.WorkingHours.Add(existing);
                }

                existing.IsClosed = item.IsClosed;
                existing.OpenTime = item.IsClosed ? null : item.OpenTime;
                existing.CloseTime = item.IsClosed ? null : item.CloseTime;
                existing.UpdatedAt = utcNow;
            }

            store.UpdatedAt = utcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return MapStoreWorkingHours(store);
        }

        private static void ValidateWorkingHours(
            List<UpdateVendorStoreWorkingHourItemDto> items)
        {
            var duplicatedDays = items
                .GroupBy(x => x.Day)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatedDays.Count > 0)
                throw new InvalidOperationException("·« Ì„ﬂ‰  ﬂ—«— ‰›” «·ÌÊ„ √ﬂÀ— „‰ „—….");

            foreach (var item in items)
            {
                if (!Enum.IsDefined(typeof(StoreDayEnum), item.Day))
                    throw new InvalidOperationException("«·ÌÊ„ «·„Õœœ €Ì— ’ÕÌÕ.");

                if (item.IsClosed)
                    continue;

                if (!item.OpenTime.HasValue || !item.CloseTime.HasValue)
                    throw new InvalidOperationException("Ì—ÃÏ ≈œŒ«· Êﬁ  «·› Õ ÊÊﬁ  «·≈€·«ﬁ ··ÌÊ„ «·„› ÊÕ.");

                if (item.CloseTime <= item.OpenTime)
                    throw new InvalidOperationException("Êﬁ  «·≈€·«ﬁ ÌÃ» √‰ ÌﬂÊ‰ »⁄œ Êﬁ  «·› Õ.");
            }
        }

        private static VendorStoreWorkingHoursDto MapStoreWorkingHours(Store store)
        {
            var existingByDay = store.WorkingHours
                .GroupBy(x => x.Day)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.UpdatedAt ?? y.CreatedAt).First());

            var allDays = Enum.GetValues<StoreDayEnum>()
                .OrderBy(x => (int)x)
                .Select(day =>
                {
                    if (existingByDay.TryGetValue(day, out var workingHour))
                    {
                        return new VendorStoreWorkingHourItemDto
                        {
                            Day = workingHour.Day,
                            OpenTime = workingHour.OpenTime,
                            CloseTime = workingHour.CloseTime,
                            IsClosed = workingHour.IsClosed
                        };
                    }

                    return new VendorStoreWorkingHourItemDto
                    {
                        Day = day,
                        OpenTime = null,
                        CloseTime = null,
                        IsClosed = true
                    };
                })
                .ToList();

            return new VendorStoreWorkingHoursDto
            {
                StoreId = store.Id,
                NameAr = store.NameAr,
                NameEn = store.NameEn,
                WorkingHours = allDays
            };
        }
    }
}