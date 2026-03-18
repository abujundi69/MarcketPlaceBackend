using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.Quantity)
                   .HasColumnType("decimal(18,3)")
                   .IsRequired();

            builder.Property(x => x.RequestedAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired(false);

            builder.Property(x => x.UnitPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.LineTotal)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.Property(x => x.ProductVariantId)
                   .IsRequired(false);

            // صار الربط unique على (Customer + Product + Variant)
            // حتى نفس المنتج يمكن إضافته أكثر من مرة إذا كان كل مرة Variant مختلف
            builder.HasIndex(x => new { x.CustomerId, x.ProductId, x.ProductVariantId })
                   .IsUnique();

            builder.HasOne(x => x.Customer)
                   .WithMany(x => x.CartItems)
                   .HasForeignKey(x => x.CustomerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Product)
                   .WithMany(x => x.CartItems)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductVariant)
                   .WithMany(x => x.CartItems)
                   .HasForeignKey(x => x.ProductVariantId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("CartItems");
        }
    }
}