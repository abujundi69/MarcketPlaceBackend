using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class ProductRequestConfiguration : IEntityTypeConfiguration<ProductRequest>
    {
        public void Configure(EntityTypeBuilder<ProductRequest> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.UnitId)
                   .IsRequired(false);

            builder.Property(x => x.NameAr)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.NameEn)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.DescriptionAr)
                   .HasMaxLength(1000)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.DescriptionEn)
                   .HasMaxLength(1000)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.Image)
                   .HasColumnType("varbinary(max)")
                   .IsRequired(false);

            builder.Property(x => x.ProductType)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.PurchaseInputMode)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.AllowDecimalQuantity)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.Price)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.SalePrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired(false);

            builder.Property(x => x.CostPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired(false);

            builder.Property(x => x.StockQuantity)
                   .HasColumnType("decimal(18,3)")
                   .HasDefaultValue(0m)
                   .IsRequired();

            builder.Property(x => x.MinStockQuantity)
                   .HasColumnType("decimal(18,3)")
                   .HasDefaultValue(0m)
                   .IsRequired();

            builder.Property(x => x.MinPurchaseQuantity)
                   .HasColumnType("decimal(18,3)")
                   .HasDefaultValue(1m)
                   .IsRequired();

            builder.Property(x => x.MaxPurchaseQuantity)
                   .HasColumnType("decimal(18,3)")
                   .IsRequired(false);

            builder.Property(x => x.QuantityStep)
                   .HasColumnType("decimal(18,3)")
                   .HasDefaultValue(1m)
                   .IsRequired();

            builder.Property(x => x.Status)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.AdminNote)
                   .HasMaxLength(1000)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.RequestedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(x => x.ReviewedAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.Property(x => x.ReviewedByUserId)
                   .IsRequired(false);

            builder.Property(x => x.ProductId)
                   .IsRequired(false);

            builder.HasIndex(x => x.VendorId);
            builder.HasIndex(x => x.StoreId);
            builder.HasIndex(x => x.CategoryId);
            builder.HasIndex(x => x.UnitId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.ProductId);

            builder.HasOne(x => x.Vendor)
                   .WithMany()
                   .HasForeignKey(x => x.VendorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Store)
                   .WithMany()
                   .HasForeignKey(x => x.StoreId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Category)
                   .WithMany()
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Unit)
                   .WithMany()
                   .HasForeignKey(x => x.UnitId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Product)
                   .WithMany()
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("ProductRequests");
        }
    }
}