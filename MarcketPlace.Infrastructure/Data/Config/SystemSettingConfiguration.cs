using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
    {
        public void Configure(EntityTypeBuilder<SystemSetting> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.SystemNameAr)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.SystemNameEn)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.FooterAr)
                   .HasMaxLength(1000)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.FooterEn)
                   .HasMaxLength(1000)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.LogoUrl)
                   .HasMaxLength(1000)
                   .IsUnicode(false)
                   .IsRequired(false);

            builder.Property(x => x.UpdatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.ToTable("SystemSettings");
        }
    }
}