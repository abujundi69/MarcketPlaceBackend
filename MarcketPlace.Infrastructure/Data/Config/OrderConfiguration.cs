using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.OrderNumber)
                   .HasMaxLength(50)
                   .IsUnicode(false)
                   .IsRequired();

            builder.HasIndex(x => x.OrderNumber)
                   .IsUnique();

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.AddressText)
                   .HasMaxLength(500)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.Latitude)
                   .HasColumnType("decimal(10,8)")
                   .IsRequired();

            builder.Property(x => x.Longitude)
                   .HasColumnType("decimal(11,8)")
                   .IsRequired();

            builder.Property(x => x.PickupLocationNameAr)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.PickupLocationNameEn)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.PickupAddressText)
                   .HasMaxLength(500)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.PickupLatitude)
                   .HasColumnType("decimal(10,8)")
                   .IsRequired();

            builder.Property(x => x.PickupLongitude)
                   .HasColumnType("decimal(11,8)")
                   .IsRequired();

            builder.Property(x => x.CustomerNote)
                   .HasMaxLength(1000)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.CustomerWhatsAppNumber)
                   .HasMaxLength(20)
                   .IsUnicode(false)
                   .IsRequired(false);

            builder.Property(x => x.Subtotal)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.DeliveryFee)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.TotalAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.DriverAssignedAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.Property(x => x.PickedUpAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.Property(x => x.DeliveredAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.Property(x => x.CancelReason)
                   .HasMaxLength(1000)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.CancelledAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.HasOne(x => x.Customer)
                   .WithMany(x => x.Orders)
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Driver)
                   .WithMany(x => x.Orders)
                   .HasForeignKey(x => x.DriverId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DeliveryZone)
                   .WithMany(x => x.Orders)
                   .HasForeignKey(x => x.DeliveryZoneId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CancelledByUser)
                   .WithMany()
                   .HasForeignKey(x => x.CancelledByUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.DriverId);
            builder.HasIndex(x => x.DeliveryZoneId);
            builder.HasIndex(x => new { x.Status, x.DriverId });

            builder.ToTable("Orders");
        }
    }
}