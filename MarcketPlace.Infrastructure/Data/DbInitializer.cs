using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Infrastructure.Data
{
    /// <summary>
    /// Seeds default data. Uses C# strings (Unicode) to avoid SQL file encoding issues.
    /// </summary>
    public static class DbInitializer
    {
        private const string SuperAdminPhone = "+970599000000";
        private const string SuperAdminPassword = "0599000000@@";

        public static async Task EnsureSuperAdminExistsAsync(AppDbContext context, bool resetPasswordInDev = false, CancellationToken cancellationToken = default)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == SuperAdminPhone, cancellationToken);

            var hasher = new PasswordHasher<User>();

            if (user is null)
            {
                user = new User
                {
                    FullName = "Super Admin",
                    PhoneNumber = SuperAdminPhone,
                    Role = UserRole.SuperAdmin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                user.PasswordHash = hasher.HashPassword(user, SuperAdminPassword);
                context.Users.Add(user);
            }
            else if (resetPasswordInDev)
            {
                user.PasswordHash = hasher.HashPassword(user, SuperAdminPassword);
                user.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// متجر افتراضي واحد "مستودع نبض المدينة" يُستخدم لطلبات المنتجات من التجار.
        /// </summary>
        public static async Task EnsureSystemStoreExistsAsync(AppDbContext context, CancellationToken cancellationToken = default)
        {
            var exists = await context.Stores
                .AnyAsync(x => x.VendorId == null && x.NameAr == "مستودع نبض المدينة", cancellationToken);

            if (exists)
                return;

            var store = new Store
            {
                VendorId = null,
                NameAr = "مستودع نبض المدينة",
                NameEn = "Nabd City Warehouse",
                DescriptionAr = "المستودع المركزي",
                DescriptionEn = "Central Warehouse",
                PhoneNumber = "0599000000",
                AddressText = "عنوان المستودع المركزي",
                Latitude = 0,
                Longitude = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Stores.Add(store);
            await context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// يضمن وجود منطقة توصيل افتراضية ويصلح البيانات المشوهة (مشكلة الترميز عند تنفيذ SQL يدوياً).
        /// </summary>
        public static async Task EnsureDefaultDeliveryZoneExistsAsync(AppDbContext context, CancellationToken cancellationToken = default)
        {
            var zone = await context.DeliveryZones.FirstOrDefaultAsync(x => x.Id == 1, cancellationToken);
            var correctNameAr = "منطقة افتراضية";
            var correctNameEn = "Default Zone";

            if (zone is null)
            {
                var now = DateTime.UtcNow;
                await context.Database.ExecuteSqlRawAsync(
                    "SET IDENTITY_INSERT DeliveryZones ON; INSERT INTO DeliveryZones (Id, NameAr, NameEn, DeliveryFee, CreatedAt, UpdatedAt) VALUES (1, {0}, {1}, 8.00, {2}, NULL); SET IDENTITY_INSERT DeliveryZones OFF;",
                    correctNameAr, correctNameEn, now);
                return;
            }

            if (IsLikelyCorrupted(zone.NameAr))
            {
                zone.NameAr = correctNameAr;
                zone.NameEn = correctNameEn;
                zone.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        /// <summary>
        /// يضمن وجود إعدادات نظام افتراضية ويصلح البيانات المشوهة (مشكلة الترميز).
        /// </summary>
        public static async Task EnsureDefaultSystemSettingsExistsAsync(AppDbContext context, CancellationToken cancellationToken = default)
        {
            var setting = await context.SystemSettings.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);

            if (setting is null)
            {
                var newSetting = new SystemSetting
                {
                    SystemNameAr = "نبض المدينة",
                    SystemNameEn = "City Pulse",
                    FooterAr = "© نبض المدينة - جميع الحقوق محفوظة",
                    FooterEn = "© City Pulse - All rights reserved",
                    PickupNameAr = "المستودع الرئيسي",
                    PickupNameEn = "Main Warehouse",
                    PickupAddressText = "الموقع الافتراضي",
                    PickupLatitude = 31.5m,
                    PickupLongitude = 34.5m,
                    UpdatedAt = DateTime.UtcNow
                };
                context.SystemSettings.Add(newSetting);
                await context.SaveChangesAsync(cancellationToken);
                await EnsureWorkingHoursForSetting(context, newSetting, cancellationToken);
                return;
            }

            var needsFix = IsLikelyCorrupted(setting.SystemNameAr)
                || IsLikelyCorrupted(setting.FooterAr)
                || IsLikelyCorrupted(setting.PickupNameAr);

            if (needsFix)
            {
                setting.SystemNameAr = "نبض المدينة";
                setting.SystemNameEn = "City Pulse";
                setting.FooterAr = "© نبض المدينة - جميع الحقوق محفوظة";
                setting.FooterEn = "© City Pulse - All rights reserved";
                setting.PickupNameAr = "المستودع الرئيسي";
                setting.PickupNameEn = "Main Warehouse";
                setting.PickupAddressText = "الموقع الافتراضي";
                setting.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private static async Task EnsureWorkingHoursForSetting(
            AppDbContext context,
            SystemSetting setting,
            CancellationToken cancellationToken)
        {
            foreach (Domain.Enums.WeekDayEnum day in Enum.GetValues(typeof(Domain.Enums.WeekDayEnum)))
            {
                if (setting.WorkingHours.Any(x => x.Day == day)) continue;
                setting.WorkingHours.Add(new MarketWorkingHour
                {
                    Day = day,
                    IsClosed = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// يكتشف النص العربي المشوه (mojibake) الناتج عن ترميز خاطئ.
        /// </summary>
        private static bool IsLikelyCorrupted(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            return text.Contains('\u00D9')  // Ù - شائع في UTF-8 مُفسَّر كـ Latin-1
                || text.Contains('\u00C2')  // Â
                || text.Contains("\u00C2\u00A9");  // Â©
        }
    }
}
