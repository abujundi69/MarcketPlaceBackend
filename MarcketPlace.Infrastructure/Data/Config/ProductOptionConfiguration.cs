using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class ProductOptionConfiguration : IEntityTypeConfiguration<ProductOption>
    {
        public void Configure(EntityTypeBuilder<ProductOption> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.NameAr)
                   .HasMaxLength(100)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.NameEn)
                   .HasMaxLength(100)
                   .IsUnicode(true)
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
            builder.HasIndex(x => new { x.ProductId, x.SortOrder });

            builder.HasOne(x => x.Product)
                   .WithMany(x => x.Options)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.ToTable("ProductOptions");
        }
    }
}