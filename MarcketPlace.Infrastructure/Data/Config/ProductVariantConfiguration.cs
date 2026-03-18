using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.UnitId)
                   .IsRequired(false);

            builder.Property(x => x.NameAr)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.NameEn)
                   .HasMaxLength(200)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.SKU)
                   .HasMaxLength(100)
                   .IsUnicode(false)
                   .IsRequired(false);

            builder.Property(x => x.Barcode)
                   .HasMaxLength(100)
                   .IsUnicode(false)
                   .IsRequired(false);

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
                   .IsRequired(false);

            builder.Property(x => x.MaxPurchaseQuantity)
                   .HasColumnType("decimal(18,3)")
                   .IsRequired(false);

            builder.Property(x => x.QuantityStep)
                   .HasColumnType("decimal(18,3)")
                   .IsRequired(false);

            builder.Property(x => x.IsDefault)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true)
                   .IsRequired();

            builder.Property(x => x.SortOrder)
                   .HasDefaultValue(0)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.HasIndex(x => x.ProductId);
            builder.HasIndex(x => x.UnitId);
            builder.HasIndex(x => x.IsDefault);
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.SKU);
            builder.HasIndex(x => x.Barcode);

            builder.HasOne(x => x.Product)
                   .WithMany(x => x.Variants)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Unit)
                   .WithMany(x => x.ProductVariants)
                   .HasForeignKey(x => x.UnitId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("ProductVariants");
        }
    }
}