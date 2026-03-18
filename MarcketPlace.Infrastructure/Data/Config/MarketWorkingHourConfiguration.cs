using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class MarketWorkingHourConfiguration : IEntityTypeConfiguration<MarketWorkingHour>
    {
        public void Configure(EntityTypeBuilder<MarketWorkingHour> builder)
        {
            builder.ToTable("MarketWorkingHours");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.Day)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(x => x.OpenTime)
                   .IsRequired(false);

            builder.Property(x => x.CloseTime)
                   .IsRequired(false);

            builder.Property(x => x.IsClosed)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .IsRequired(false);

            builder.HasIndex(x => new { x.SystemSettingId, x.Day })
                   .IsUnique();

            builder.HasOne(x => x.SystemSetting)
                   .WithMany(x => x.WorkingHours)
                   .HasForeignKey(x => x.SystemSettingId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}