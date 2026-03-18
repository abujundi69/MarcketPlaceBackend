using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class ProductVariantOptionValueConfiguration : IEntityTypeConfiguration<ProductVariantOptionValue>
    {
        public void Configure(EntityTypeBuilder<ProductVariantOptionValue> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.HasIndex(x => x.ProductVariantId);
            builder.HasIndex(x => x.ProductOptionValueId);

            builder.HasIndex(x => new { x.ProductVariantId, x.ProductOptionValueId })
                   .IsUnique();

            builder.HasOne(x => x.ProductVariant)
                   .WithMany(x => x.VariantOptionValues)
                   .HasForeignKey(x => x.ProductVariantId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProductOptionValue)
                   .WithMany(x => x.VariantOptionValues)
                   .HasForeignKey(x => x.ProductOptionValueId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("ProductVariantOptionValues");
        }
    }
}