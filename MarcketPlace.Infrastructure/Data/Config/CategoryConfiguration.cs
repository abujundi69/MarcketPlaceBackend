using MarcketPlace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcketPlace.Infrastructure.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.NameAr)
                   .HasMaxLength(150)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.NameEn)
                   .HasMaxLength(150)
                   .IsUnicode(true)
                   .IsRequired();

            builder.Property(x => x.Image)
                   .HasColumnType("varbinary(max)")
                   .IsRequired(false);

            builder.Property(x => x.DisplayOrder)
                   .HasDefaultValue(0)
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

            builder.HasOne(x => x.Parent)
                   .WithMany(x => x.Children)
                   .HasForeignKey(x => x.ParentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Products)
                   .WithOne(x => x.Category)
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ParentId);
            builder.HasIndex(x => new { x.IsActive, x.DisplayOrder });
            builder.HasIndex(x => x.NameAr);
            builder.HasIndex(x => x.NameEn);
        }
    }
}