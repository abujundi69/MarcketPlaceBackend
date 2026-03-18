using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
    {
        public void Configure(EntityTypeBuilder<SystemSetting> builder)
        {
            builder.ToTable("SystemSettings");

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
                   .HasMaxLength(500)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.FooterEn)
                   .HasMaxLength(500)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.CustomerPromoMessage)
                   .HasMaxLength(1000)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.Logo)
                   .IsRequired(false);

            builder.Property(x => x.PickupNameAr)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.PickupNameEn)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.PickupAddressText)
                   .HasMaxLength(500)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.PickupLatitude)
                   .HasPrecision(18, 6)
                   .IsRequired();

            builder.Property(x => x.PickupLongitude)
                   .HasPrecision(18, 6)
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .IsRequired();

            builder.HasMany(x => x.WorkingHours)
                   .WithOne(x => x.SystemSetting)
                   .HasForeignKey(x => x.SystemSettingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}