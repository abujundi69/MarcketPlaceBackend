using MarcketPlace.Application.Admin.SystemSettings.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.SystemSettings
{
    public class SystemSettingAdminService : ISystemSettingAdminService
    {
        private readonly AppDbContext _context;

        public SystemSettingAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SystemSettingDto> GetAsync(CancellationToken cancellationToken = default)
        {
            var setting = await _context.SystemSettings
                .Include(x => x.WorkingHours)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var needsSave = false;

            if (setting is null)
            {
                setting = new SystemSetting
                {
                    SystemNameAr = "اسم النظام",
                    SystemNameEn = "System Name",
                    FooterAr = "تذييل النظام",
                    FooterEn = "System Footer",
                    CustomerPromoMessage = null,
                    Logo = null,

                    PickupNameAr = "نقطة الاستلام",
                    PickupNameEn = "Pickup Point",
                    PickupAddressText = "عنوان نقطة الاستلام",
                    PickupLatitude = 0,
                    PickupLongitude = 0,

                    UpdatedAt = DateTime.UtcNow
                };

                _context.SystemSettings.Add(setting);
                needsSave = true;
            }

            EnsureAllWeekDaysExist(setting);

            if (needsSave || _context.ChangeTracker.HasChanges())
                await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(setting);
        }

        public async Task<SystemSettingDto> UpdateAsync(
            UpdateSystemSettingDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            var systemNameAr = dto.SystemNameAr?.Trim();
            var systemNameEn = dto.SystemNameEn?.Trim();
            var footerAr = dto.FooterAr?.Trim();
            var footerEn = dto.FooterEn?.Trim();
            var pickupNameAr = dto.PickupNameAr?.Trim();
            var pickupNameEn = dto.PickupNameEn?.Trim();
            var pickupAddressText = dto.PickupAddressText?.Trim();
            var promoMessage = string.IsNullOrWhiteSpace(dto.CustomerPromoMessage)
                ? null
                : dto.CustomerPromoMessage.Trim();

            if (string.IsNullOrWhiteSpace(systemNameAr))
                throw new InvalidOperationException("اسم النظام بالعربي مطلوب.");

            if (string.IsNullOrWhiteSpace(systemNameEn))
                throw new InvalidOperationException("اسم النظام بالإنجليزي مطلوب.");

            if (string.IsNullOrWhiteSpace(footerAr))
                throw new InvalidOperationException("الفوتر بالعربي مطلوب.");

            if (string.IsNullOrWhiteSpace(footerEn))
                throw new InvalidOperationException("الفوتر بالإنجليزي مطلوب.");

            if (string.IsNullOrWhiteSpace(pickupNameAr))
                throw new InvalidOperationException("اسم نقطة الاستلام بالعربي مطلوب.");

            if (string.IsNullOrWhiteSpace(pickupNameEn))
                throw new InvalidOperationException("اسم نقطة الاستلام بالإنجليزي مطلوب.");

            if (string.IsNullOrWhiteSpace(pickupAddressText))
                throw new InvalidOperationException("عنوان نقطة الاستلام مطلوب.");

            if (dto.PickupLatitude < -90 || dto.PickupLatitude > 90)
                throw new InvalidOperationException("PickupLatitude غير صالحة.");

            if (dto.PickupLongitude < -180 || dto.PickupLongitude > 180)
                throw new InvalidOperationException("PickupLongitude غير صالحة.");

            ValidateWorkingHours(dto.WorkingHours);

            var setting = await _context.SystemSettings
                .Include(x => x.WorkingHours)
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (setting is null)
            {
                setting = new SystemSetting
                {
                    SystemNameAr = systemNameAr,
                    SystemNameEn = systemNameEn,
                    FooterAr = footerAr,
                    FooterEn = footerEn,
                    CustomerPromoMessage = promoMessage,
                    Logo = dto.Logo,
                    PickupNameAr = pickupNameAr,
                    PickupNameEn = pickupNameEn,
                    PickupAddressText = pickupAddressText,
                    PickupLatitude = dto.PickupLatitude,
                    PickupLongitude = dto.PickupLongitude,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.SystemNameAr = systemNameAr;
                setting.SystemNameEn = systemNameEn;
                setting.FooterAr = footerAr;
                setting.FooterEn = footerEn;
                setting.CustomerPromoMessage = promoMessage;
                setting.Logo = dto.Logo;
                setting.PickupNameAr = pickupNameAr;
                setting.PickupNameEn = pickupNameEn;
                setting.PickupAddressText = pickupAddressText;
                setting.PickupLatitude = dto.PickupLatitude;
                setting.PickupLongitude = dto.PickupLongitude;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            ApplyWorkingHours(setting, dto.WorkingHours);

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(setting);
        }

        public async Task<string?> GetCustomerPromoMessageAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SystemSettings
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => x.CustomerPromoMessage)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private void ValidateWorkingHours(List<UpdateSystemSettingWorkingHourDto> workingHours)
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
                {
                    continue;
                }

                if (!item.OpenTime.HasValue)
                    throw new InvalidOperationException($"وقت الفتح مطلوب لليوم: {item.Day}");

                if (!item.CloseTime.HasValue)
                    throw new InvalidOperationException($"وقت الإغلاق مطلوب لليوم: {item.Day}");

                if (item.CloseTime.Value <= item.OpenTime.Value)
                    throw new InvalidOperationException($"وقت الإغلاق يجب أن يكون بعد وقت الفتح لليوم: {item.Day}");
            }
        }

        private void ApplyWorkingHours(
            SystemSetting setting,
            List<UpdateSystemSettingWorkingHourDto> workingHours)
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

        private void EnsureAllWeekDaysExist(SystemSetting setting)
        {
            foreach (WeekDayEnum day in Enum.GetValues(typeof(WeekDayEnum)))
            {
                var exists = setting.WorkingHours.Any(x => x.Day == day);
                if (exists)
                    continue;

                setting.WorkingHours.Add(new MarketWorkingHour
                {
                    Day = day,
                    IsClosed = true,
                    OpenTime = null,
                    CloseTime = null,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        private static SystemSettingDto MapToDto(SystemSetting setting)
        {
            return new SystemSettingDto
            {
                Id = setting.Id,
                SystemNameAr = setting.SystemNameAr,
                SystemNameEn = setting.SystemNameEn,
                FooterAr = setting.FooterAr,
                FooterEn = setting.FooterEn,
                CustomerPromoMessage = setting.CustomerPromoMessage,
                Logo = setting.Logo,
                PickupNameAr = setting.PickupNameAr,
                PickupNameEn = setting.PickupNameEn,
                PickupAddressText = setting.PickupAddressText,
                PickupLatitude = setting.PickupLatitude,
                PickupLongitude = setting.PickupLongitude,
                UpdatedAt = setting.UpdatedAt,
                WorkingHours = setting.WorkingHours
                    .OrderBy(x => x.Day)
                    .Select(x => new SystemSettingWorkingHourDto
                    {
                        Id = x.Id,
                        Day = x.Day,
                        OpenTime = x.OpenTime,
                        CloseTime = x.CloseTime,
                        IsClosed = x.IsClosed
                    })
                    .ToList()
            };
        }
    }
}