using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class StoreWorkingHourConfiguration : IEntityTypeConfiguration<StoreWorkingHour>
    {
        public void Configure(EntityTypeBuilder<StoreWorkingHour> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.Day)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.OpenTime)
                   .HasColumnType("time")
                   .IsRequired(false);

            builder.Property(x => x.CloseTime)
                   .HasColumnType("time")
                   .IsRequired(false);

            builder.Property(x => x.IsClosed)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.HasIndex(x => new { x.StoreId, x.Day })
                   .IsUnique();

            builder.HasOne(x => x.Store)
                   .WithMany(x => x.WorkingHours)
                   .HasForeignKey(x => x.StoreId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("StoreWorkingHours");
        }
    }
}