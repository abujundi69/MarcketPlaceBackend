using System.Security.Claims;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/vendor/stores")]
    [Authorize(Roles = nameof(UserRole.Vendor))]
    public class VendorStoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VendorStoresController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VendorStoreListItemDto>>> GetMyStores(
            CancellationToken cancellationToken)
        {
            var vendorId = await GetCurrentVendorIdAsync(cancellationToken);
            if (vendorId is null)
                return Ok(Array.Empty<VendorStoreListItemDto>());

            var stores = await _context.Stores
                .AsNoTracking()
                .Where(x => x.VendorId == vendorId.Value)
                .OrderBy(x => x.NameAr)
                .ThenBy(x => x.NameEn)
                .Select(x => new VendorStoreListItemDto
                {
                    Id = x.Id,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn
                })
                .ToListAsync(cancellationToken);

            return Ok(stores);
        }

        [HttpGet("working-hours")]
        public async Task<ActionResult<IReadOnlyList<VendorStoreWorkingHoursDto>>> GetMyStoresWorkingHours(
            CancellationToken cancellationToken)
        {
            var vendorId = await GetCurrentVendorIdAsync(cancellationToken);
            if (vendorId is null)
                return Ok(Array.Empty<VendorStoreWorkingHoursDto>());

            var stores = await _context.Stores
                .AsNoTracking()
                .Where(x => x.VendorId == vendorId.Value)
                .OrderBy(x => x.NameAr)
                .ThenBy(x => x.NameEn)
                .ToListAsync(cancellationToken);

            if (stores.Count == 0)
                return Ok(Array.Empty<VendorStoreWorkingHoursDto>());

            var setting = await _context.SystemSettings
                .AsNoTracking()
                .Include(x => x.WorkingHours)
                .FirstOrDefaultAsync(cancellationToken);

            var result = stores
                .Select(store => MapStoreWorkingHours(store, setting?.WorkingHours))
                .ToList();

            return Ok(result);
        }

        [HttpGet("{storeId:int}/working-hours")]
        public async Task<ActionResult<VendorStoreWorkingHoursDto>> GetStoreWorkingHours(
            int storeId,
            CancellationToken cancellationToken)
        {
            var store = await GetVendorStoreAsync(storeId, cancellationToken);
            if (store is null)
                return NotFound();

            var setting = await _context.SystemSettings
                .AsNoTracking()
                .Include(x => x.WorkingHours)
                .FirstOrDefaultAsync(cancellationToken);

            return Ok(MapStoreWorkingHours(store, setting?.WorkingHours));
        }

        [HttpPut("{storeId:int}/working-hours")]
        public async Task<ActionResult<VendorStoreWorkingHoursDto>> UpdateStoreWorkingHours(
            int storeId,
            [FromBody] UpdateVendorStoreWorkingHoursRequest dto,
            CancellationToken cancellationToken)
        {
            var store = await GetVendorStoreAsync(storeId, cancellationToken);
            if (store is null)
                return NotFound();

            ValidateWorkingHours(dto.WorkingHours);

            var setting = await _context.SystemSettings
                .Include(x => x.WorkingHours)
                .FirstOrDefaultAsync(cancellationToken);

            if (setting is null)
                return NotFound("تعذر العثور على إعدادات النظام.");

            ApplyWorkingHours(setting, dto.WorkingHours);
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(MapStoreWorkingHours(store, setting.WorkingHours));
        }

        private async Task<int?> GetCurrentVendorIdAsync(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            return await _context.Vendors
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<Store?> GetVendorStoreAsync(int storeId, CancellationToken cancellationToken)
        {
            var vendorId = await GetCurrentVendorIdAsync(cancellationToken);
            if (vendorId is null)
                return null;

            return await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == storeId && x.VendorId == vendorId.Value,
                    cancellationToken);
        }

        private int GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(raw) || !int.TryParse(raw, out var userId))
                throw new UnauthorizedAccessException("المستخدم غير مسجل الدخول.");
            return userId;
        }

        private static void ValidateWorkingHours(
            List<UpdateVendorStoreWorkingHourItemDto> workingHours)
        {
            if (workingHours is null || workingHours.Count == 0)
                throw new InvalidOperationException("ساعات العمل مطلوبة.");

            var duplicatedDays = workingHours
                .GroupBy(x => x.Day)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatedDays.Count > 0)
                throw new InvalidOperationException("يوجد تكرار في أيام ساعات العمل.");

            foreach (WeekDayEnum day in Enum.GetValues(typeof(WeekDayEnum)))
            {
                if (!workingHours.Any(x => x.Day == day))
                    throw new InvalidOperationException($"يجب إرسال ساعات العمل لليوم: {day}");
            }

            foreach (var item in workingHours)
            {
                if (item.IsClosed)
                    continue;

                if (!item.OpenTime.HasValue)
                    throw new InvalidOperationException($"وقت الفتح مطلوب لليوم: {item.Day}");

                if (!item.CloseTime.HasValue)
                    throw new InvalidOperationException($"وقت الإغلاق مطلوب لليوم: {item.Day}");

                if (item.CloseTime.Value <= item.OpenTime.Value)
                    throw new InvalidOperationException($"وقت الإغلاق يجب أن يكون بعد وقت الفتح لليوم: {item.Day}");
            }
        }

        private static void ApplyWorkingHours(
            SystemSetting setting,
            List<UpdateVendorStoreWorkingHourItemDto> workingHours)
        {
            foreach (WeekDayEnum day in Enum.GetValues(typeof(WeekDayEnum)))
            {
                var dtoItem = workingHours.First(x => x.Day == day);
                var entity = setting.WorkingHours.FirstOrDefault(x => x.Day == day);

                if (entity is null)
                {
                    entity = new MarketWorkingHour
                    {
                        Day = day,
                        CreatedAt = DateTime.UtcNow
                    };
                    setting.WorkingHours.Add(entity);
                }

                entity.IsClosed = dtoItem.IsClosed;
                entity.OpenTime = dtoItem.IsClosed ? null : dtoItem.OpenTime;
                entity.CloseTime = dtoItem.IsClosed ? null : dtoItem.CloseTime;
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static VendorStoreWorkingHoursDto MapStoreWorkingHours(
            Store store,
            IEnumerable<MarketWorkingHour>? workingHours)
        {
            return new VendorStoreWorkingHoursDto
            {
                StoreId = store.Id,
                NameAr = store.NameAr,
                NameEn = store.NameEn,
                WorkingHours = (workingHours ?? Enumerable.Empty<MarketWorkingHour>())
                    .OrderBy(x => x.Day)
                    .Select(x => new VendorStoreWorkingHourItemDto
                    {
                        Day = x.Day,
                        OpenTime = x.OpenTime,
                        CloseTime = x.CloseTime,
                        IsClosed = x.IsClosed
                    })
                    .ToList()
            };
        }
    }

    public class VendorStoreListItemDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
    }

    public class VendorStoreWorkingHoursDto
    {
        public int StoreId { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public List<VendorStoreWorkingHourItemDto> WorkingHours { get; set; } = new();
    }

    public class VendorStoreWorkingHourItemDto
    {
        public WeekDayEnum Day { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool IsClosed { get; set; }
    }

    public class UpdateVendorStoreWorkingHoursRequest
    {
        public List<UpdateVendorStoreWorkingHourItemDto> WorkingHours { get; set; } = new();
    }

    public class UpdateVendorStoreWorkingHourItemDto
    {
        public WeekDayEnum Day { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool IsClosed { get; set; }
    }
}
