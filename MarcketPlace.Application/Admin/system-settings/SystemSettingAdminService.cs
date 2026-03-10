using MarcketPlace.Application.Admin.SystemSettings.Dtos;
using MarcketPlace.Domain.Entities;
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
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (setting is null)
            {
                setting = new SystemSetting
                {
                    SystemNameAr = "اسم النظام",
                    SystemNameEn = "System Name",
                    FooterAr = "تذييل النظام",
                    FooterEn = "System Footer",
                    Logo = null,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.SystemSettings.Add(setting);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return MapToDto(setting);
        }

        public async Task<SystemSettingDto> UpdateAsync(UpdateSystemSettingDto dto, CancellationToken cancellationToken = default)
        {
            var systemNameAr = dto.SystemNameAr?.Trim();
            var systemNameEn = dto.SystemNameEn?.Trim();
            var footerAr = dto.FooterAr?.Trim();
            var footerEn = dto.FooterEn?.Trim();

            if (string.IsNullOrWhiteSpace(systemNameAr))
                throw new InvalidOperationException("اسم النظام بالعربي مطلوب.");

            if (string.IsNullOrWhiteSpace(systemNameEn))
                throw new InvalidOperationException("اسم النظام بالإنجليزي مطلوب.");

            if (string.IsNullOrWhiteSpace(footerAr))
                throw new InvalidOperationException("الفوتر بالعربي مطلوب.");

            if (string.IsNullOrWhiteSpace(footerEn))
                throw new InvalidOperationException("الفوتر بالإنجليزي مطلوب.");

            var setting = await _context.SystemSettings
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
                    Logo = dto.Logo,
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
                setting.Logo = dto.Logo;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(setting);
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
                Logo = setting.Logo,
                UpdatedAt = setting.UpdatedAt
            };
        }
    }
}