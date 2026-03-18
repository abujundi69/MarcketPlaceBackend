using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.NameAr)
                   .HasMaxLength(250)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.NameEn)
                   .HasMaxLength(250)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.DescriptionAr)
                   .HasMaxLength(2000)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.DescriptionEn)
                   .HasMaxLength(2000)
                   .IsUnicode(true)
                   .IsRequired(false);

            builder.Property(x => x.Image)
                   .IsRequired(false);

            builder.Property(x => x.ProductType)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(x => x.PurchaseInputMode)
                   .HasConversion<int>()
                   .IsRequired();

            builder.Property(x => x.AllowDecimalQuantity)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(x => x.Price)
                   .HasPrecision(18, 2)
                   .IsRequired();

            builder.Property(x => x.SalePrice)
                   .HasPrecision(18, 2)
                   .IsRequired(false);

            builder.Property(x => x.CostPrice)
                   .HasPrecision(18, 2)
                   .IsRequired(false);

            builder.Property(x => x.StockQuantity)
                   .HasPrecision(18, 3)
                   .IsRequired();

            builder.Property(x => x.MinStockQuantity)
                   .HasPrecision(18, 3)
                   .HasDefaultValue(0m)
                   .IsRequired();

            builder.Property(x => x.MinPurchaseQuantity)
                   .HasPrecision(18, 3)
                   .HasDefaultValue(1m)
                   .IsRequired();

            builder.Property(x => x.MaxPurchaseQuantity)
                   .HasPrecision(18, 3)
                   .IsRequired(false);

            builder.Property(x => x.QuantityStep)
                   .HasPrecision(18, 3)
                   .HasDefaultValue(1m)
                   .IsRequired();

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .IsRequired(false);

            builder.HasOne(x => x.Category)
                   .WithMany(x => x.Products)
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Unit)
                   .WithMany(x => x.Products)
                   .HasForeignKey(x => x.UnitId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(x => x.Options)
                   .WithOne(x => x.Product)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Variants)
                   .WithOne(x => x.Product)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.CategoryId);
            builder.HasIndex(x => x.UnitId);
            builder.HasIndex(x => x.ProductType);
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.NameAr);
            builder.HasIndex(x => x.NameEn);
        }
    }
}