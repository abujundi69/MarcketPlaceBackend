using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Config
{
    public class ProductUnitConfiguration : IEntityTypeConfiguration<ProductUnit>
    {
        public void Configure(EntityTypeBuilder<ProductUnit> builder)
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

            builder.Property(x => x.Symbol)
                   .HasMaxLength(20)
                   .IsUnicode(false)
                   .IsRequired();

            builder.Property(x => x.MeasurementType)
                   .HasConversion<string>()
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(x => x.FactorToBaseUnit)
                   .HasColumnType("decimal(18,6)")
                   .HasDefaultValue(1m)
                   .IsRequired();

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true)
                   .IsRequired();

            builder.Property(x => x.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(x => x.UpdatedAt)
                   .HasColumnType("datetime2")
                   .IsRequired(false);

            builder.HasIndex(x => x.Symbol).IsUnique();
            builder.HasIndex(x => x.MeasurementType);
            builder.HasIndex(x => x.IsActive);

            builder.ToTable("ProductUnits");
        }
    }
}