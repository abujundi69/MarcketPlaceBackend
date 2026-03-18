using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.ProductNameAr)
                   .HasMaxLength(250)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.ProductNameEn)
                   .HasMaxLength(250)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.ProductImage)
                   .IsRequired(false);

            builder.Property(x => x.VariantNameAr)
                   .HasMaxLength(250)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.VariantNameEn)
                   .HasMaxLength(250)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.UnitSymbol)
                   .HasMaxLength(20)
                   .IsUnicode(false)
                   .IsRequired(false);

            builder.Property(x => x.UnitPrice)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(x => x.Quantity)
                   .HasPrecision(18, 3)
                   .IsRequired();

            builder.Property(x => x.RequestedAmount)
                   .HasPrecision(18, 2)
                   .IsRequired(false);

            builder.Property(x => x.PurchaseInputMode)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(x => x.LineTotal)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.HasOne(x => x.Order)
                   .WithMany(x => x.Items)
                   .HasForeignKey(x => x.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Product)
                   .WithMany(x => x.OrderItems)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductVariant)
                   .WithMany(x => x.OrderItems)
                   .HasForeignKey(x => x.ProductVariantId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.OrderId);
            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => x.ProductVariantId);
        }
    }
}