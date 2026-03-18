using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class ProductOptionValueConfiguration : IEntityTypeConfiguration<ProductOptionValue>
    {
        public void Configure(EntityTypeBuilder<ProductOptionValue> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.ValueAr)
                   .HasMaxLength(100)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.ValueEn)
                   .HasMaxLength(100)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.ColorHex)
                   .HasMaxLength(20)
                   .IsUnicode(false)
                   .IsRequired(false);

            builder.Property(x => x.SortOrder)
                   .HasDefaultValue(0)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasIndex(x => x.ProductOptionId);
            builder.HasIndex(x => new { x.ProductOptionId, x.SortOrder });

            builder.HasOne(x => x.ProductOption)
                   .WithMany(x => x.Values)
                   .HasForeignKey(x => x.ProductOptionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("ProductOptionValues");
        }
    }
}